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
using System.IO;
using System.IO.Abstractions;
using DotnetWrappers;
using Exceptions;

/// <summary>
/// Loads a native library and returns a pointer for the purpose of interoping with it.
/// </summary>
internal sealed class NativeLibraryLoader : ILibraryLoader
{
    private readonly IAssembly assembly;
    private readonly IPlatform platform;
    private readonly IFile file;
    private readonly IPath path;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeLibraryLoader"/> class.
    /// </summary>
    /// <param name="assembly">Provides assembly related services.</param>
    /// <param name="platform">Provides platform specific information.</param>
    /// <param name="file">Performs operations with files.</param>
    /// <param name="path">Manages file paths.</param>
    /// <param name="library">The library to load.</param>
    public NativeLibraryLoader(
        IAssembly assembly,
        IPlatform platform,
        IFile file,
        IPath path,
        ILibrary library)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(platform);
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(library);

        this.assembly = assembly;
        this.platform = platform;
        this.file = file;
        this.path = path;

        LibraryName = library.GetLibraryName();
    }

    /// <inheritdoc/>
    public string LibraryName { get; }

    /// <inheritdoc/>
    public nint LoadLibrary()
    {
        var libDirPath = this.assembly.Location;
        var libFilePath = $"{libDirPath}{this.path.DirectorySeparatorChar}{LibraryName}";

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

        // Add the library path that is attempting to be loaded
        loadLibExceptionMsg += $"\n\nLibrary Path: '{libraryFilePath}'";

        throw new LoadLibraryException(loadLibExceptionMsg);
    }
}
