// <copyright file="LibraryLoader.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using CASL.Exceptions;

    /*Refer to these links for more information
    1. https://dev.to/jeikabu/loading-native-libraries-in-c-fh6
    2. https://github.com/mhowlett/NNanomsg/blob/master/NNanomsg/Interop.cs#L193
    */

    /// <summary>
    /// Loads a native library and returns a pointer for the purpose of interoping with it.
    /// </summary>
    internal class LibraryLoader : ILibraryLoader
    {
        private readonly char DirSeparator = Path.DirectorySeparatorChar;
        private readonly IDependencyManager dependencyManager;
        private readonly IPlatform platform;
        private readonly IDirectory directory;
        private readonly IFile file;

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoader"/> class.
        /// </summary>
        /// <param name="dependencyManager">Manages the native library's dependencies.</param>
        /// <param name="platform">Gets required information about the platform.</param>
        /// <param name="directory">Performs directory IO operations.</param>
        /// <param name="file">Performs file IO operations.</param>
        /// <param name="library">The library to load.</param>
        public LibraryLoader(IDependencyManager dependencyManager, IPlatform platform, IDirectory directory, IFile file, ILibrary library)
        {
            if (dependencyManager is null)
            {
                throw new ArgumentNullException(nameof(dependencyManager), "The parameter must not be null.");
            }

            if (platform is null)
            {
                throw new ArgumentNullException(nameof(platform), "The parameter must not be null.");
            }

            if (directory is null)
            {
                throw new ArgumentNullException(nameof(directory), "The parameter must not be null.");
            }

            if (file is null)
            {
                throw new ArgumentNullException(nameof(file), "The parameter must not be null.");
            }

            if (library is null)
            {
                throw new ArgumentNullException(nameof(library), "The parameter must not be null.");
            }

            this.dependencyManager = dependencyManager;
            this.platform = platform;
            this.directory = directory;
            this.file = file;

            LibraryName = ProcessLibExtension(library.LibraryName);

            dependencyManager.SetupDependencies();
        }

        /// <inheritdoc/>
        public string LibraryName { get; }

        /// <inheritdoc/>
        public nint LoadLibrary() => this.platform.IsWinPlatform() ? LoadWindowsLibrary() : LoadPoxisLibrary();

        /// <summary>
        /// Loads a windows library and returns a pointer to that library.
        /// </summary>
        /// <returns>A pointer to the windows library.</returns>
        private nint LoadWindowsLibrary()
        {
            var missingLibPaths = new List<string>();

            foreach (var libPath in this.dependencyManager.LibraryDirPaths)
            {
                // Add a directory separator if one is missing
                var libDirPath = Path.EndsInDirectorySeparator(libPath) ?
                    libPath :
                    $@"{libPath}{this.DirSeparator}";

                if (this.file.Exists($"{libDirPath}{LibraryName}"))
                {
                    var libPtr = this.platform.LoadLibrary($"{libDirPath}{LibraryName}");

                    if (libPtr == IntPtr.Zero)
                    {
                        var loadLibExceptionMsg = this.platform.GetLastSystemError();

                        // Add the library path that is is attempting to be loaded
                        loadLibExceptionMsg += $"\n\nLibrary Path: '{libDirPath}{LibraryName}'";

                        // TODO: This line below is for Windows specific error codes.
                        // This needs to be reworked to handle posix systems
                        // Add the link to information about windows system error codes
                        loadLibExceptionMsg += "\n\nSystem Error Codes: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-";

                        throw new LoadLibraryException(loadLibExceptionMsg);
                    }

                    return libPtr;
                }

                missingLibPaths.Add(libPath);
            }

            var exceptionMsg = $"Could not find the library '{LibraryName}'.\n\nPaths Checked: \n";

            // Add the missing library paths to the exception message
            foreach (var path in missingLibPaths)
            {
                exceptionMsg += $"\t{path}\n";
            }

            throw new Exception(exceptionMsg);
        }

        /// <summary>
        /// Loads a posix library and returns a pointer to that library.
        /// </summary>
        /// <returns>A pointer to the posix library.</returns>
        private nint LoadPoxisLibrary()
        {
            var missingLibPaths = new List<string>();

            foreach (var libPath in this.dependencyManager.LibraryDirPaths)
            {
                // Add a directory separator if one is missing
                var libDirPath = Path.EndsInDirectorySeparator(libPath) ?
                    libPath :
                    $@"{libPath}{this.DirSeparator}";

                var libraryName = GetLatestPosixLibraryVersion(libDirPath, LibraryName);

                if (string.IsNullOrEmpty(libraryName))
                {
                    missingLibPaths.Add(libPath);
                    continue;
                }

                var fullFilePath = $"{libDirPath}{libraryName}";

                if (this.file.Exists(fullFilePath))
                {
                    var libPtr = this.platform.LoadLibrary(fullFilePath);

                    if (libPtr == IntPtr.Zero)
                    {
                        var loadLibExceptionMsg = this.platform.GetLastSystemError();

                        // Add the library path that is is attempting to be loaded
                        loadLibExceptionMsg += $"\n\nLibrary Path: '{libDirPath}{libraryName}'";

                        throw new LoadLibraryException(loadLibExceptionMsg);
                    }

                    return libPtr;
                }

                missingLibPaths.Add(libPath);
            }

            var exceptionMsg = $"Could not find the library '{LibraryName}'.\nPaths Checked: \n";

            // Add the missing library paths to the exception message
            foreach (var path in missingLibPaths)
            {
                exceptionMsg += $"\t{path}\n";
            }

            // TODO: Create custom exception called LoadLibraryException
            throw new Exception(exceptionMsg);
        }

        /// <summary>
        /// Processes the current windows library name to make sure that it has an extension.
        /// </summary>
        /// <param name="libraryName">The library name to process.</param>
        /// <returns>The name of the library with the extension on it.</returns>
        /// <remarks>
        ///     If the library already has a valid extension, then nothing is changed. It if does not have an extension,
        ///     or the extension is incorrect, it will fix it.
        /// </remarks>
        private string ProcessLibExtension(string libraryName)
        {
            if (string.IsNullOrEmpty(libraryName))
            {
                throw new ArgumentNullException(nameof(libraryName), "The library name must not be null or empty.");
            }

            var libExtension = this.platform.GetPlatformLibFileExtension();

            if (Path.HasExtension(libraryName))
            {
                var libraryNameWithoutExtension = libraryName;

                // Strip any possible extensions off of the library name
                while (Path.HasExtension(libraryNameWithoutExtension))
                {
                    libraryNameWithoutExtension = Path.GetFileNameWithoutExtension(libraryNameWithoutExtension);
                }

                return $"{libraryNameWithoutExtension}{libExtension}";
            }
            else
            {
                return $"{libraryName}{libExtension}";
            }
        }

        /// <summary>
        /// Searchs for and gets the latest version of a posix library that matches the given <paramref name="libraryName"/>.
        /// </summary>
        /// <param name="possibleLibPath">The path to where the libraries might exist.</param>
        /// <param name="libraryName">The library name to process.</param>
        /// <returns>The latest version of the given <paramref name="libraryName"/>.</returns>
        private string GetLatestPosixLibraryVersion(string possibleLibPath, string libraryName)
        {
            var libExtension = this.platform.GetPlatformLibFileExtension();

            var libraryNameNoExt = libraryName;

            // Strip any extensions off of the name
            while (Path.HasExtension(libraryNameNoExt))
            {
                libraryNameNoExt = Path.GetFileNameWithoutExtension(libraryNameNoExt);
            }

            var possibleLibs = (from n in this.directory.GetFiles(possibleLibPath)
                                where Path.GetFileName(n).ToLower().Contains(libraryNameNoExt.ToLower())
                                    && Path.GetFileName(n).ToLower().Contains(".so")
                                select n).ToArray();

            if (possibleLibs.Length <= 0)
            {
                return string.Empty;
            }

            var largestVersion = -1;

            // Find the library name that has the largest version number on it.
            // Example: '.so.1' or '.so.2'
            foreach (var possibleLib in possibleLibs)
            {
                if (possibleLib.Contains(".so."))
                {
                    var sections = possibleLib.Split(".so.");

                    var parseSuccess = int.TryParse(sections[1], out var libVersion);

                    if (parseSuccess && libVersion > largestVersion)
                    {
                        largestVersion = libVersion;
                    }
                }
            }

            var chosenLibName = largestVersion == -1u
                ? Path.GetFileName(possibleLibs[0])
                : $"{libraryNameNoExt}{libExtension}.{largestVersion}";

            return chosenLibName;
        }
    }
}
