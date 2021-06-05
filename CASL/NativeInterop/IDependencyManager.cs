// <copyright file="IDependencyManager.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Manages the library dependencies for a native library.
    /// </summary>
    internal interface IDependencyManager
    {
        /// <summary>
        /// Gets the path to a <see cref="Library"/> for a <see cref="LibraryLoader"/> to load.
        /// </summary>
        ReadOnlyCollection<string> LibraryDirPaths { get; }

        /// <summary>
        /// Gets or sets the list of native library names.
        /// </summary>
        ReadOnlyCollection<string> NativeLibraries { get; set; }

        /// <summary>
        /// Gets or sets the native lib path.
        /// </summary>
        string NativeLibPath { get; set; }

        /// <summary>
        /// Sets up all of the dependencies.
        /// </summary>
        void SetupDependencies();
    }
}
