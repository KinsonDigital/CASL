// <copyright file="OpenALLibrary.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop;

using Exceptions;

/// <summary>
/// Represents the OpenAL library.
/// </summary>
internal class OpenALLibrary : ILibrary
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
    public string LibraryName
    {
        get
        {
            if (this.platform.IsWinPlatform())
            {
                return WinLibName;
            }
            else if (this.platform.IsPosixPlatform())
            {
                return PosixLibName;
            }
            else
            {
                throw new UnknownPlatformException($"The platform '{this.platform.CurrentOSPlatform}' is unknown.");
            }
        }
    }
}
