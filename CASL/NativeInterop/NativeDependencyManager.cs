// <copyright file="NativeDependencyManager.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using CASL.Exceptions;

    /// <summary>
    /// Manages native dependency libraries.
    /// </summary>
    internal abstract class NativeDependencyManager : IDependencyManager
    {
        private readonly IPlatform platform;
        private readonly IFile file;
        private readonly IPath path;
        private readonly string[] libraryPaths = Array.Empty<string>();
        private readonly string assemblyDirectory = string.Empty;
        private string[] nativeLibraries = Array.Empty<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeDependencyManager"/> class.
        /// </summary>
        /// <param name="platform">Manages platform specific operations.</param>
        /// <param name="file">Manages file related operations.</param>
        /// <param name="path">Manages file paths.</param>
        /// <param name="application">Gets information about the application.</param>
        public NativeDependencyManager(IPlatform platform, IFile file, IPath path, IApplication application)
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

            this.platform = platform;
            this.file = file;
            this.path = path;

            string architecture;

            if (this.platform.Is32BitProcess())
            {
                architecture = "x86";
            }
            else if (this.platform.Is64BitProcess())
            {
                architecture = "x64";
            }
            else
            {
                throw new InvalidOperationException("Process Architecture Not Recognized.");
            }

            string osPlatform;

            if (this.platform.IsWinPlatform())
            {
                osPlatform = "win";
            }
            else if (this.platform.IsPosixPlatform())
            {
                osPlatform = "linux";
            }
            else
            {
                throw new UnknownPlatformException("Unknown Operating System/Platform.");
            }

            var separator = this.path.DirectorySeparatorChar;
            this.assemblyDirectory = $@"{this.path.GetDirectoryName(application.Location)}{separator}";
            this.libraryPaths = new[] { this.assemblyDirectory };

            NativeLibPath = $@"{this.assemblyDirectory}runtimes{separator}{osPlatform}-{architecture}{separator}native{separator}";
        }

        /// <inheritdoc/>
        public ReadOnlyCollection<string> LibraryDirPaths => new (this.libraryPaths);

        /// <inheritdoc/>
        public ReadOnlyCollection<string> NativeLibraries
        {
            get => this.nativeLibraries.ToReadOnlyCollection();
            set
            {
                if (value is null)
                {
                    this.nativeLibraries = Array.Empty<string>();
                }
                else
                {
                    this.nativeLibraries = value.ToList().ToArray();
                }
            }
        }

        /// <inheritdoc/>
        public string NativeLibPath { get; private set; } = string.Empty;

        /// <inheritdoc/>
        public void SetupDependencies()
        {
            /* Check each dependency library file to see if it already exists in the
            * destination folder, and if it does not, move it from the runtimes
            * folder to the destination execution folder
            */
            foreach (var library in NativeLibraries)
            {
                var srcFilePath = $@"{NativeLibPath}{library}";
                var destFilePath = $@"{this.assemblyDirectory}{library}";

                if (this.file.Exists(destFilePath))
                {
                    continue;
                }
                else
                {
                    // Check if the runtimes folder contains the library and if not, throws an exception
                    if (this.file.Exists(srcFilePath))
                    {
                        this.file.Copy(srcFilePath, destFilePath, true);
                    }
                    else
                    {
                        throw new FileNotFoundException($"The native dependency library '{srcFilePath}' does not exist.");
                    }
                }
            }
        }
    }
}
