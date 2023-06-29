// <copyright file="OpenALDependencyManager.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop;

using System.IO.Abstractions;

/// <summary>
/// Manages the library dependencies.
/// </summary>
internal class OpenALDependencyManager : NativeDependencyManager
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenALDependencyManager"/> class.
    /// </summary>
    /// <param name="file">Performs operations with files.</param>
    /// <param name="path">Manages file paths.</param>
    /// <param name="nativeLibPathResolver">Resolves native library paths.</param>
    public OpenALDependencyManager(IFile file, IPath path, IFilePathResolver nativeLibPathResolver)
        : base(file, path, nativeLibPathResolver)
    {
    }
}
