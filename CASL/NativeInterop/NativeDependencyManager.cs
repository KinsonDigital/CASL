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

    /// <summary>
    /// Manages native dependency libraries.
    /// </summary>
    internal abstract class NativeDependencyManager : IDependencyManager
    {
        private readonly char DirSeparator = Path.DirectorySeparatorChar;
        private readonly IPlatform platform;
        private readonly IFile file;
        private readonly string[] libraryPaths = Array.Empty<string>();
        private readonly string assemblyDirectory = string.Empty;
        private string[] nativeLibraries = Array.Empty<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeDependencyManager"/> class.
        /// </summary>
        /// <param name="platform">Manages platform specific operations.</param>
        /// <param name="file">Manages file related operations.</param>
        public NativeDependencyManager(IPlatform platform, IFile file)
        {
            if (platform is null)
            {
                throw new ArgumentNullException(nameof(platform), "The platform must not be null.");
            }

            this.platform = platform;
            this.file = file;

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
                throw new Exception("Process Architecture Not Recognized.");
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
                throw new Exception("Unknown Operating System/Platform");
            }

            if (this.platform.IsWinPlatform())
            {
                this.assemblyDirectory = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{this.DirSeparator}";
                this.libraryPaths = new[] { this.assemblyDirectory };

                // TODO: SHould not have to point to this dir path created by the nuget package.
                // Try this at the assembly path
                NativeLibPath = $@"{this.assemblyDirectory}runtimes{this.DirSeparator}{osPlatform}-{architecture}{this.DirSeparator}native{this.DirSeparator}";
            }
            else if (this.platform.IsPosixPlatform())
            {
                this.assemblyDirectory = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{this.DirSeparator}";
                this.libraryPaths = new[] { this.assemblyDirectory };

                NativeLibPath = $@"{this.assemblyDirectory}runtimes{this.DirSeparator}{osPlatform}-{architecture}{this.DirSeparator}native{this.DirSeparator}";
            }
        }

        /// <summary>
        /// Gets the path to the <see cref="Library"/> for a <see cref="LibraryLoader"/> to load.
        /// </summary>
        public ReadOnlyCollection<string> LibraryDirPaths => new (this.libraryPaths);

        /// <summary>
        /// Gets or sets the list of native library dependencies.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the native lib path.
        /// </summary>
        public string NativeLibPath { get; set; } = string.Empty;

        /// <summary>
        /// Sets up all of the dependencies.
        /// </summary>
        public void SetupDependencies()
        {
            if (NativeLibraries.Count <= 0)
            {
                throw new Exception("Must set a list of native libraries.");
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
