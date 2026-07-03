// <copyright file="ILibrary.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop;

/// <summary>
/// Represents a library that can be used.
/// </summary>
internal interface ILibrary
{
    /// <summary>
    /// Gets the name of the library.
    /// </summary>
    /// <returns>The name of the library.</returns>
    string GetLibraryName();

    /// <summary>
    /// Gets the full path to the library.
    /// </summary>
    /// <returns>The full path to the library.</returns>
    string GetLibraryPath();
}
