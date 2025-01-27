// <copyright file="OpenALLibrary.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop;

using System.IO;
using System.IO.Abstractions;
using Exceptions;

/// <summary>
/// Represents the OpenAL library.
/// </summary>
internal sealed class OpenALLibrary : ILibrary
{
    private const string WinLibName = "soft_oal.dll";
    private const string PosixLibName = "libopenal.so";
    private readonly IPlatform platform;
    private readonly IDirectory directory;
    private readonly IFile file;
    private readonly IPath path;
    private readonly string appDirPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenALLibrary"/> class.
    /// </summary>
    /// <param name="platform">Provides platform specific information.</param>
    /// <param name="application">Provides application related services.</param>
    public OpenALLibrary(
        IPlatform platform,
        IDirectory directory,
        IFile file,
        IPath path,
        IApplication application)
    {
        // TODO: Add null checks for all params
        this.platform = platform;
        this.directory = directory;
        this.file = file;
        this.path = path;
        this.appDirPath = application.Location;

        ProcessLibFile();
    }

    /// <inheritdoc/>
    public string GetLibraryName()
    {
        if (this.platform.IsWinPlatform())
        {
            return WinLibName;
        }

        if (this.platform.IsPosixPlatform())
        {
            return PosixLibName;
        }

        throw new UnknownPlatformException($"The platform '{this.platform.CurrentOSPlatform}' is unknown or not supported.");
    }

    /// <summary>
    /// Checks if the OpenAL library located in the same location as CASL.
    /// If not, it will check if the correct runtime/platform directory exists, and if so
    /// copies the library to the same location as CASL.
    /// </summary>
    private void ProcessLibFile()
    {
        var libName = GetLibraryName();
        var fullLibPath = $"{this.appDirPath}{this.path.DirectorySeparatorChar}{libName}";

        // Check if the library exists in the same location as the application
        if (this.file.Exists(fullLibPath))
        {
            return;
        }

        var platformDirName = string.Empty;

        if (this.platform.IsWinPlatform())
        {
            platformDirName = "win";
        }
        else if (this.platform.IsPosixPlatform())
        {
            platformDirName = "linux";
        }

        var platformDirPath = $"{this.appDirPath}{this.path.DirectorySeparatorChar}runtimes{this.path.DirectorySeparatorChar}" +
                              $"{platformDirName}-x64{this.path.DirectorySeparatorChar}native";

        // If the correct runtime/platform directory does not exist
        if (!this.directory.Exists(platformDirPath))
        {
            throw new DirectoryNotFoundException($"The directory '{platformDirPath}' does not exist.");
        }

        var fullPlatLibPath = $"{platformDirPath}{this.path.DirectorySeparatorChar}{libName}";

        // If the library does not exist in the platform directory
        if (!this.file.Exists(fullPlatLibPath))
        {
            throw new FileNotFoundException($"The library '{libName}' does not exist in the platform directory '{platformDirPath}'.");
        }

        // At this point, the library file exist.  Copy the library to the same location as the application.
        this.file.Copy(fullPlatLibPath, fullLibPath);
    }
}
