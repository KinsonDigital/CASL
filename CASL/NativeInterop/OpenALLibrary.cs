// <copyright file="OpenALLibrary.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System;

namespace CASL.NativeInterop
{
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
                    return "libopenal.so.1";
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
