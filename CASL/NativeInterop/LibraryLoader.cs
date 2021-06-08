// <copyright file="LibraryLoader.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;

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
        private readonly IFile libFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoader"/> class.
        /// </summary>
        /// <param name="dependencyManager">Manages the native library's dependencies.</param>
        /// <param name="platform">Gets required information about the platform.</param>
        /// <param name="libFile">Performs file IO operations.</param>
        /// <param name="library">The library to load.</param>
        public LibraryLoader(IDependencyManager dependencyManager, IPlatform platform, IFile libFile, ILibrary library)
        {
            if (dependencyManager is null)
            {
                throw new ArgumentNullException(nameof(dependencyManager), "The parameter must not be null.");
            }

            if (platform is null)
            {
                throw new ArgumentNullException(nameof(platform), "The parameter must not be null.");
            }

            if (libFile is null)
            {
                throw new ArgumentNullException(nameof(libFile), "The parameter must not be null.");
            }

            if (library is null)
            {
                throw new ArgumentNullException(nameof(library), "The parameter must not be null.");
            }

            this.dependencyManager = dependencyManager;
            this.platform = platform;
            this.libFile = libFile;

            LibraryName = ProcessLibraryExtension(library.LibraryName);

            dependencyManager.SetupDependencies();
        }

        /// <inheritdoc/>
        public string LibraryName { get; }

        /// <inheritdoc/>
        public IntPtr LoadLibrary()
        {
            var missingLibPaths = new List<string>();

            foreach (var libPath in this.dependencyManager.LibraryDirPaths)
            {
                var libDirPath = Path.EndsInDirectorySeparator(libPath) ?
                    libPath :
                    $@"{libPath}{DirSeparator}";

                if (this.libFile.Exists($"{libDirPath}{LibraryName}"))
                {
                    var libPtr = this.platform.LoadLibrary($"{libDirPath}{LibraryName}");

                    if (libPtr == IntPtr.Zero)
                    {
                        var loadLibExceptionMsg = this.platform.GetLastSystemError();

                        // Add the library path that is is attempting to be loaded
                        loadLibExceptionMsg += $"\n\nLibrary Path: '{libDirPath}{LibraryName}'";

                        // Add the link to information about windows system error codes
                        loadLibExceptionMsg += "\n\nSystem Error Codes: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-";

                        // TODO: Create LoadLibraryFailureException class
                        throw new Exception(loadLibExceptionMsg);
                    }

                    return libPtr;
                }

                missingLibPaths.Add(libPath);
            }

            var exceptionMsg = $"Could not find the library '{LibraryName}'.\n\nPaths Checked: \n";

            // Add the missing library paths to the exception message
            foreach (var path in missingLibPaths)
            {
                exceptionMsg += $"\t{path}{DirSeparator}\n";
            }

            throw new Exception(exceptionMsg);
        }

        /// <summary>
        /// Processes the current library name to make sure that it has an extension depending on the current platform.
        /// </summary>
        /// <param name="libraryName">The library name to process.</param>
        /// <returns></returns>
        private string ProcessLibraryExtension(string libraryName)
        {
            if (string.IsNullOrEmpty(libraryName))
            {
                throw new ArgumentNullException(nameof(libraryName), "The library name must not be null or empty.");
            }

            var libExtension = this.platform.GetPlatformLibFileExtension();

            if (Path.HasExtension(libraryName))
            {
                // Corrects the extension if it is incorrect
                if (Path.GetExtension(libraryName) != libExtension)
                {
                    return $"{Path.GetFileNameWithoutExtension(libraryName)}{libExtension}";
                }

                return libraryName;
            }
            else
            {
                return $"{libraryName}{libExtension}";
            }
        }
    }
}
