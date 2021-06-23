// <copyright file="OpenALLibrary.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop
{
    using CASL.Exceptions;

    /// <summary>
    /// Represents the OpenAL library.
    /// </summary>
    internal class OpenALLibrary : ILibrary
    {
        private readonly IPlatform platform;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALLibrary"/> class.
        /// </summary>
        /// <param name="platform">Information about the platform.</param>
        public OpenALLibrary(IPlatform platform) => this.platform = platform;

        /// <inheritdoc/>
        public string LibraryName
        {
            get
            {
                if (this.platform.IsWinPlatform())
                {
                    return "soft_oal.dll";
                }
                else if (this.platform.IsPosixPlatform())
                {
                    return "libopenal.so";
                }
                else
                {
                    throw new UnknownPlatformException($"The platform '{this.platform.CurrentPlatform}' is unknown.");
                }
            }
        }
    }
}
