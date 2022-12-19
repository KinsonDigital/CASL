// <copyright file="NativeDependencyManager.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.IO.Abstractions;

    /// <summary>
    /// Manages native dependency libraries.
    /// </summary>
    internal abstract class NativeDependencyManager : IDependencyManager
    {
        private readonly IFile file;
        private readonly IPath path;
        private string[] nativeLibraries = Array.Empty<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeDependencyManager"/> class.
        /// </summary>
        /// <param name="platform">Manages platform specific operations.</param>
        /// <param name="file">Manages file related operations.</param>
        /// <param name="path">Manages file paths.</param>
        /// <param name="application">Gets information about the application.</param>
        /// <param name="nativeLibPathResolver">Resolves native library paths.</param>
        public NativeDependencyManager(
            IPlatform platform,
            IFile file,
            IPath path,
            IApplication application,
            IFilePathResolver nativeLibPathResolver)
        {
            if (platform is null)
            {
                throw new ArgumentNullException(nameof(platform), "The parameter must not be null.");
            }

            if (file is null)
            {
                throw new ArgumentNullException(nameof(file), "The parameter must not be null.");
            }

            if (path is null)
            {
                throw new ArgumentNullException(nameof(path), "The parameter must not be null.");
            }

            if (application is null)
            {
                throw new ArgumentNullException(nameof(application), "The parameter must not be null.");
            }

            if (nativeLibPathResolver is null)
            {
                throw new ArgumentNullException(nameof(nativeLibPathResolver), "The parameter must not be null.");
            }

            this.file = file;
            this.path = path;

            NativeLibDirPath = nativeLibPathResolver.GetDirPath().ToCrossPlatPath().TrimAllFromEnd('/');
        }

        /// <summary>
        /// Gets or sets the list of native library names that a library depends on.
        /// </summary>
        /// <remarks>
        ///     This is not treated like a list of library paths.
        ///     Any directory paths included with the library names will be ignored.
        ///     File extensions are allowed but will be ignored.
        /// </remarks>
        public ReadOnlyCollection<string> NativeLibraries
        {
            get => this.nativeLibraries.ToReadOnlyCollection();
            set
            {
                var result = new List<string>();

                foreach (var lib in value)
                {
                    var extension = this.path.GetExtension(lib);

                    result.Add($"{this.path.GetFileNameWithoutExtension(lib)}{extension}");
                }

                this.nativeLibraries = result.ToArray();
            }
        }

        /// <inheritdoc/>
        public string NativeLibDirPath { get; private set; } = string.Empty;

        /// <inheritdoc/>
        public void VerifyDependencies()
        {
            if (NativeLibraries is null)
            {
                return;
            }

            /* Check each dependency library file to see if it already exists in the
            * destination folder, and if it does not, move it from the runtimes
            * folder to the destination execution folder
            */
            foreach (var library in NativeLibraries)
            {
                var srcFilePath = $@"{NativeLibDirPath}/{library}";

                if (this.file.Exists(srcFilePath))
                {
                    continue;
                }
                else
                {
                    throw new FileNotFoundException($"The native dependency library '{srcFilePath}' does not exist.");
                }
            }
        }
    }
}
