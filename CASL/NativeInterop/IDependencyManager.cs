// <copyright file="IDependencyManager.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop;

using System.Collections.ObjectModel;

/// <summary>
/// Manages the library dependencies for a native library.
/// </summary>
internal interface IDependencyManager
{
    /// <summary>
    /// Gets or sets the list of native library names that a library depends on.
    /// </summary>
    /// <remarks>
    ///     This is not treated like a list of library paths.
    ///     Any directory paths included with the library names will be ignored.
    /// </remarks>
    ReadOnlyCollection<string> NativeLibraries { get; set; }

    /// <summary>
    /// Gets the native library directory path.
    /// </summary>
    string NativeLibDirPath { get; }

    /// <summary>
    /// Verifies that all of the dependencies exist.
    /// </summary>
    void VerifyDependencies();
}
