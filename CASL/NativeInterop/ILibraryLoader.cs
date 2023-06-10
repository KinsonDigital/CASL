// <copyright file="ILibraryLoader.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop;

using System;

/// <summary>
/// Loads a native library and returns a pointer for the purpose of interoping with it.
/// </summary>
internal interface ILibraryLoader
{
    /// <summary>
    /// Gets the name of the library.
    /// </summary>
    string LibraryName { get; }

    /// <summary>
    /// Loads a library with the set <see cref="LibraryName"/> and returns a pointer to it.
    /// </summary>
    /// <returns>A pointer to the native library.</returns>
    IntPtr LoadLibrary();
}
