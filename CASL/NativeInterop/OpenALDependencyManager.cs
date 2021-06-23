// <copyright file="OpenALDependencyManager.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop
{
    using System.IO.Abstractions;

    /// <summary>
    /// Manages the library dependencies for the native SDL library 'SDL2.dll'.
    /// </summary>
    internal class OpenALDependencyManager : NativeDependencyManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALDependencyManager"/> class.
        /// </summary>
        /// <param name="platform">Manages platform specific operations.</param>
        /// <param name="file">Manages file related operations.</param>
        /// <param name="path">Manages pathing strings.</param>
        public OpenALDependencyManager(IPlatform platform, IFile file, IPath path)
            : base(platform, file, path)
        {
        }
    }
}
