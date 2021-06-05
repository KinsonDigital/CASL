// <copyright file="OpenALLibrary.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop
{
    /// <summary>
    /// Represents the OpenAL library.
    /// </summary>
    internal class OpenALLibrary : ILibrary
    {
        /// <inheritdoc/>
        public string LibraryName { get; } = "openal32.dll";
    }
}
