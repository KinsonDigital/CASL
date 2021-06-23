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
    using System.Reflection;
    using CASL.Exceptions;

    /// <summary>
    /// Manages native dependency libraries.
    /// </summary>
    internal abstract class NativeDependencyManager : IDependencyManager
    {
        private readonly char dirSeparator = Path.DirectorySeparatorChar;
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
        public NativeDependencyManager(IPlatform platform, IFile file, IPath path)
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

            this.assemblyDirectory = $@"{this.path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{this.dirSeparator}";
            this.libraryPaths = new[] { this.assemblyDirectory };

            NativeLibPath = $@"{this.assemblyDirectory}runtimes{this.dirSeparator}{osPlatform}-{architecture}{this.dirSeparator}native{this.dirSeparator}";
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
            if (NativeLibraries.Count <= 0)
            {
                return;
            }

            // Check each dependency library file to see if it exists and if
            // it doesn't, move it from the runtimes folder to the execution assembly folder
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
                        // TODO: Create better exception message that shows the src and dest paths
                        throw new Exception($"Library '{library}' does not exist.");
                    }
                }
            }
        }
    }
}
