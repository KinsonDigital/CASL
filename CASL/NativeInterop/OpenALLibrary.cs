// <copyright file="OpenALLibrary.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop
{
    using System;

    /// <summary>
    /// Represents the OpenAL library.
    /// </summary>
    internal class OpenALLibrary : ILibrary
    {
        private readonly IPlatform platform;

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
                    // TODO: Create custom exception for this
                    throw new Exception("Unknown platform");
                }
            }
        }
    }
}
