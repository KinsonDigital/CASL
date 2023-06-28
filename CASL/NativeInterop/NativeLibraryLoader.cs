// <copyright file="NativeLibraryLoader.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

/*
 * Refer to these links for more information
 * 1. https://dev.to/jeikabu/loading-native-libraries-in-c-fh6
 * 2. https://github.com/mhowlett/NNanomsg/blob/master/NNanomsg/Interop.cs#L193
*/

// ReSharper disable UnusedMember.Local
namespace CASL.NativeInterop;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Exceptions;

/// <summary>
/// Loads a native library and returns a pointer for the purpose of interoping with it.
/// </summary>
internal class NativeLibraryLoader : ILibraryLoader
{
    private const char CrossPlatDirSeparatorChar = '/';
    private readonly IDependencyManager dependencyManager;
    private readonly IPlatform platform;
    private readonly IDirectory directory;
    private readonly IFile file;
    private readonly IPath path;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeLibraryLoader"/> class.
    /// </summary>
    /// <param name="dependencyManager">Manages the native library's dependencies.</param>
    /// <param name="platform">Provides platform specific information.</param>
    /// <param name="directory">Performs operations with directories.</param>
    /// <param name="file">Performs operations with files.</param>
    /// <param name="path">Manages file paths.</param>
    /// <param name="library">The library to load.</param>
    public NativeLibraryLoader(
        IDependencyManager dependencyManager,
        IPlatform platform,
        IDirectory directory,
        IFile file,
        IPath path,
        ILibrary library)
    {
        this.dependencyManager = dependencyManager ?? throw new ArgumentNullException(nameof(dependencyManager), "The parameter must not be null.");
        this.platform = platform ?? throw new ArgumentNullException(nameof(platform), "The parameter must not be null.");
        this.directory = directory ?? throw new ArgumentNullException(nameof(directory), "The parameter must not be null.");
        this.file = file ?? throw new ArgumentNullException(nameof(file), "The parameter must not be null.");
        this.path = path ?? throw new ArgumentNullException(nameof(path), "The parameter must not be null.");

        if (library is null)
        {
            throw new ArgumentNullException(nameof(library), "The parameter must not be null.");
        }

        LibraryName = ProcessLibExtension(library.LibraryName);

        dependencyManager.VerifyDependencies();
    }

    /// <inheritdoc/>
    public string LibraryName { get; }

    /// <inheritdoc/>
    public nint LoadLibrary()
    {
        var libDirPath = this.dependencyManager.NativeLibDirPath;

        // Add a directory separator if one is missing
        libDirPath = libDirPath.ToCrossPlatPath().TrimAllFromEnd(CrossPlatDirSeparatorChar);

        var libFilePath = $"{libDirPath}{CrossPlatDirSeparatorChar}{LibraryName}";

        var (exists, libPtr) = LoadLibraryIfExists(libFilePath);

        if (exists)
        {
            return libPtr;
        }

        var exceptionMsg = $"Could not find the library '{LibraryName}' in directory path '{libDirPath}'";

        throw new FileNotFoundException(exceptionMsg, libFilePath);
    }

    /// <summary>
    /// Loads a library at the given <paramref name="libraryFilePath"/> and returns
    /// a pointer to it as well as a success flag.
    /// </summary>
    /// <param name="libraryFilePath">The path to the library.</param>
    /// <returns>
    ///     exists: True if the library was successfully loaded.
    ///     libPtr: The pointer to the library if a successfully loaded.
    /// </returns>
    private (bool exists, nint libPtr) LoadLibraryIfExists(string libraryFilePath)
    {
        if (!this.file.Exists(libraryFilePath))
        {
            return (false, 0);
        }

        var libPtr = this.platform.LoadLibrary(libraryFilePath);

        if (libPtr != nint.Zero)
        {
            return (true, libPtr);
        }

        var loadLibExceptionMsg = this.platform.GetLastSystemError();

        // Add the library path that is is attempting to be loaded
        loadLibExceptionMsg += $"\n\nLibrary Path: '{libraryFilePath}'";

        throw new LoadLibraryException(loadLibExceptionMsg);
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
            throw new ArgumentNullException(nameof(libraryName), "The parameter must not be null or empty.");
        }

        while (this.path.HasExtension(libraryName))
        {
            libraryName = this.path.GetFileNameWithoutExtension(libraryName);
        }

        return $"{libraryName}{this.platform.GetPlatformLibFileExtension()}";
    }

    /// <summary>
    /// Searches for and gets the latest version of a posix library that matches the given <paramref name="libraryName"/>.
    /// </summary>
    /// <param name="possibleLibPath">The path to where the libraries might exist.</param>
    /// <param name="libraryName">The library name to process.</param>
    /// <returns>The latest version of the given <paramref name="libraryName"/>.</returns>
    [SuppressMessage("csharpsquid", "S1144", Justification = "Not referenced internally but might be in the future.")]
    private string GetLatestPosixLibraryVersion(string possibleLibPath, string libraryName)
    {
        var libExtension = this.platform.GetPlatformLibFileExtension();

        libraryName = libraryName.ToLower();

        // Strip any extensions off of the name
        while (this.path.HasExtension(libraryName))
        {
            libraryName = this.path.GetFileNameWithoutExtension(libraryName);
        }

        var possibleLibs = (from n in this.directory.GetFiles(possibleLibPath)
            where this.path.GetFileName(n).ToLower().Contains(libraryName.ToLower())
                  && this.path.GetFileName(n).ToLower().Contains(".so")
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
            if (!possibleLib.Contains(".so."))
            {
                continue;
            }

            var sections = possibleLib.Split(".so.");

            var parseSuccess = int.TryParse(sections[1], out var libVersion);

            if (parseSuccess && libVersion > largestVersion)
            {
                largestVersion = libVersion;
            }
        }

        var chosenLibName = largestVersion == -1u
            ? this.path.GetFileName(possibleLibs[0])
            : $"{libraryName}{libExtension}.{largestVersion}";

        return chosenLibName;
    }
}
