// <copyright file="OpenALLibrary.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop;

using Exceptions;

/// <summary>
/// Represents the OpenAL library.
/// </summary>
internal sealed class OpenALLibrary : ILibrary
{
    private const string WinLibName = "soft_oal.dll";
    private const string PosixLibName = "libopenal.so";
    private readonly IPlatform platform;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenALLibrary"/> class.
    /// </summary>
    /// <param name="platform">Provides platform specific information.</param>
    public OpenALLibrary(IPlatform platform) => this.platform = platform;

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

        throw new UnknownPlatformException($"The platform '{this.platform.CurrentOSPlatform}' is unknown.");
    }
}
