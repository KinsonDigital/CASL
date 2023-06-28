// <copyright file="NativeLibPathResolver.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop;

using System.IO.Abstractions;

/// <summary>
/// Resolves paths to native libraries.
/// </summary>
internal class NativeLibPathResolver : IFilePathResolver
{
    private const char CrossPlatDirSeparatorChar = '/';
    private readonly IPlatform platform;
    private readonly IPath path;
    private readonly string basePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeLibPathResolver"/> class.
    /// </summary>
    /// <param name="platform">Holds information about the platform.</param>
    /// <param name="path">Processes paths.</param>
    /// <param name="application">Gets information about the application.</param>
    public NativeLibPathResolver(IPlatform platform, IPath path, IApplication application)
    {
        this.platform = platform;
        this.path = path;

        this.basePath = (this.path.GetDirectoryName(application.Location) ?? string.Empty).ToCrossPlatPath()
            .TrimAllFromEnd(CrossPlatDirSeparatorChar);
    }

    /// <inheritdoc/>
    public string GetDirPath()
    {
        var platformValue = string.Empty;

        if (this.platform.IsWinPlatform())
        {
            platformValue = $"win-{this.platform.GetProcessArchitecture().ToString().ToLower()}";
        }
        else if (this.platform.IsMacOSXPlatform())
        {
            platformValue = $"osx{(this.platform.Is32BitProcess() ? string.Empty : "-x64")}";
        }
        else if (this.platform.IsLinuxPlatform())
        {
            // NOTE: Major linux distros dropped 32 bit support a long time ago
            platformValue = "linux-x64";
        }

        return $@"{this.basePath}{CrossPlatDirSeparatorChar}runtimes{CrossPlatDirSeparatorChar}{platformValue}" +
               $"{CrossPlatDirSeparatorChar}native";
    }

    /// <summary>
    /// Resolves the path to a library with the given <paramref name="libFilePathOrFileName"/>
    /// based on the operating system and process architecture.
    /// </summary>
    /// <param name="libFilePathOrFileName">The name of the library.</param>
    /// <returns>A resolved path with the name of the library.</returns>
    /// <remarks>The <paramref name="libFilePathOrFileName"/> can be with or without a file extension.</remarks>
    public string GetFilePath(string libFilePathOrFileName)
    {
        libFilePathOrFileName = this.path.HasExtension(libFilePathOrFileName)
            ? $"{this.path.GetFileNameWithoutExtension(libFilePathOrFileName)}{this.platform.GetPlatformLibFileExtension()}"
            : $"{libFilePathOrFileName}{this.platform.GetPlatformLibFileExtension()}";

        return $@"{GetDirPath()}{CrossPlatDirSeparatorChar}{libFilePathOrFileName}";
    }
}
