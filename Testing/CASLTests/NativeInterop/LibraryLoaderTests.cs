// <copyright file="LibraryLoaderTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

#pragma warning disable IDE0002 // Name can be simplified
namespace CASLTests
{
#pragma warning disable IDE0001 // Name can be simplified
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO.Abstractions;
    using CASL.Exceptions;
    using CASL.NativeInterop;
    using CASLTests.Helpers;
    using Moq;
    using Xunit;
    using Assert = CASLTests.Helpers.AssertExtensions;
#pragma warning restore IDE0001 // Name can be simplified

    /// <summary>
    /// Provides tests for the <see cref="LibraryLoader"/> class.
    /// </summary>
    public class LibraryLoaderTests
    {
        #region Private Fields
        private string LibraryName = string.Empty;
        private readonly Mock<IDependencyManager> mockDependencyManager;
        private readonly Mock<IDirectory> mockDirectory;
        private readonly Mock<IFile> mockFile;
        private readonly Mock<IPath> mockPath;
        private readonly Mock<ILibrary> mockLibrary;
        private string? libPath;
        private ReadOnlyCollection<string>? libDirPaths;
        private Mock<IPlatform> mockPlatform;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoaderTests"/> class.
        /// </summary>
        public LibraryLoaderTests()
        {
            this.mockPlatform = new Mock<IPlatform>();
            this.mockPath = new Mock<IPath>();
            this.mockDependencyManager = new Mock<IDependencyManager>();
            this.mockDirectory = new Mock<IDirectory>();
            this.mockFile = new Mock<IFile>();
            this.mockLibrary = new Mock<ILibrary>();
        }
        #endregion

        #region Constructor Tests
        [Fact]
        public void Ctor_WithNullDependencyManager_ThrowsException()
        {
            //Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                var loader = new LibraryLoader(
                    null,
                    this.mockPlatform.Object,
                    this.mockDirectory.Object,
                    this.mockFile.Object,
                    this.mockPath.Object,
                    this.mockLibrary.Object);
            }, "The parameter must not be null. (Parameter 'dependencyManager')");
        }

        [Fact]
        public void Ctor_WithNullPlatform_ThrowsException()
        {
            //Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                var loader = new LibraryLoader(
                    this.mockDependencyManager.Object,
                    null,
                    this.mockDirectory.Object,
                    this.mockFile.Object,
                    this.mockPath.Object,
                    this.mockLibrary.Object);
            }, "The parameter must not be null. (Parameter 'platform')");
        }

        [Fact]
        public void Ctor_WithNullDirectoryObject_ThrowsException()
        {
            //Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                var loader = new LibraryLoader(
                    this.mockDependencyManager.Object,
                    this.mockPlatform.Object,
                    null,
                    this.mockFile.Object,
                    this.mockPath.Object,
                    this.mockLibrary.Object);
            }, "The parameter must not be null. (Parameter 'directory')");
        }

        [Fact]
        public void Ctor_WithNullFileObject_ThrowsException()
        {
            //Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                var loader = new LibraryLoader(
                    this.mockDependencyManager.Object,
                    this.mockPlatform.Object,
                    this.mockDirectory.Object,
                    null,
                    this.mockPath.Object,
                    this.mockLibrary.Object);
            }, "The parameter must not be null. (Parameter 'file')");
        }

        [Fact]
        public void Ctor_WithNullPath_ThrowsException()
        {
            //Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                var loader = new LibraryLoader(
                    this.mockDependencyManager.Object,
                    this.mockPlatform.Object,
                    this.mockDirectory.Object,
                    this.mockFile.Object,
                    null,
                    this.mockLibrary.Object);
            }, "The parameter must not be null. (Parameter 'path')");
        }

        [Fact]
        public void Ctor_WithNullLibrary_ThrowsException()
        {
            //Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                var loader = new LibraryLoader(
                    this.mockDependencyManager.Object,
                    this.mockPlatform.Object,
                    this.mockDirectory.Object,
                    this.mockFile.Object,
                    this.mockPath.Object,
                    null);
            }, "The parameter must not be null. (Parameter 'library')");
        }

        [Fact]
        public void Ctor_WhenLibraryDoesNotExist_ThrowsException()
        {
            // Arrange
            MockPlatformAsWindows();
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns(string.Empty);

            // Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = CreateLoader();
            }, "The parameter must not be null or empty. (Parameter 'libraryName')");
        }

        [Theory]
        [InlineData(PlatformType.Windows)]
        [InlineData(PlatformType.Posix)]
        public void Ctor_WhenUsingLibraryNameWithIncorrectLibraryExtension_FixesExtension(PlatformType platform)
        {
            //Arrange
            MockPlatformAs(platform);
            this.mockPath.Setup(m => m.HasExtension(It.IsAny<string>())).Returns(true);
            this.mockPath.Setup(m => m.HasExtension("test-lib")).Returns(false);
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns("test-lib.txt");

            //Act
            var loader = CreateLoader();

            //Assert
            Assert.Equal(LibraryName, loader.LibraryName);
        }

        [Theory]
        [InlineData(PlatformType.Windows)]
        [InlineData(PlatformType.Posix)]
        public void Ctor_WhenUsingLibraryNameWithoutExtension_FixesExtension(PlatformType platform)
        {
            //Arrange
            MockPlatformAs(platform);
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns("test-lib");

            //Act
            var loader = CreateLoader();

            //Assert
            Assert.Equal(LibraryName, loader.LibraryName);
        }
        #endregion

        #region Method Tests
        [Theory]
        [InlineData(PlatformType.Windows)]
        [InlineData(PlatformType.Posix)]
        public void LoadLibrary_WhenWindowsLibraryDoesNotLoad_ThrowsException(PlatformType platform)
        {
            //Arrange
            MockPlatformAs(platform);
            this.mockPlatform.Setup(m => m.LoadLibrary(this.libPath)).Returns(IntPtr.Zero);
            this.mockFile.Setup(m => m.Exists(this.libPath)).Returns(true);

            var loader = CreateLoader();

            //Act & Assert
            Assert.ThrowsWithMessage<LoadLibraryException>(() =>
            {
                loader.LoadLibrary();
            }, $"Could not load module.\n\nLibrary Path: '{this.libDirPaths[0]}{LibraryName}'");
        }

        [Theory]
        [InlineData(PlatformType.Windows)]
        [InlineData(PlatformType.Posix)]
        public void LoadLibrary_WhenLibraryFileDoesNotExist_ThrowsException(PlatformType platform)
        {
            //Arrange
            MockPlatformAs(platform);
            this.mockFile.Setup(m => m.Exists(this.libPath)).Returns(false);

            var loader = CreateLoader();

            //Act & Assert
            Assert.ThrowsWithMessage<LoadLibraryException>(() =>
            {
                loader.LoadLibrary();
            }, $"Could not find the library '{LibraryName}'.\n\nPaths Checked: \n\t{this.libDirPaths[0]}\n");
        }

        [Theory]
        [InlineData(PlatformType.Windows)]
        [InlineData(PlatformType.Posix)]
        public void LoadLibrary_WhenLibPathEndsWithPathSeparator_LoadsLibrary(PlatformType platform)
        {
            //Arrange
            MockPlatformAs(platform);

            var loader = CreateLoader();

            //Act
            var actual = loader.LoadLibrary();

            //Assert
            Assert.Equal(1234, actual);
        }

        [Theory]
        [InlineData(PlatformType.Windows)]
        [InlineData(PlatformType.Posix)]
        public void LoadLibrary_WhenLibPathEndsWithoutPathSeparator_LoadsLibrary(PlatformType platform)
        {
            //Arrange
            MockPlatformAs(platform);

            // Remove the '/' or '\' character from the end
            this.mockDependencyManager.SetupGet(p => p.LibraryDirPaths).Returns(() =>
            {
                var result = new List<string>();

                foreach (var item in this.libDirPaths)
                {
                    result.Add(item.TrimEnd('/').TrimEnd('\\'));
                }

                return new ReadOnlyCollection<string>(result.ToArray());
            });

            var loader = CreateLoader();

            //Act
            var actual = loader.LoadLibrary();

            //Assert
            Assert.Equal(1234, actual);
        }

        [Theory]
        [InlineData("test-lib.so")]
        [InlineData("test-lib.so.NaN")]
        [InlineData("test-lib.so.NaN", "test-lib.so.1")]
        [InlineData("test-lib.so.1")]
        [InlineData("test-lib.so", "test-lib.so.0", "test-lib.so.1")]
        [InlineData("test-lib.so.1", "test-lib.so.0", "test-lib.so")]
        [InlineData("test-lib.so", "test-lib.so.1", "test-lib.so.0")]
        public void LoadLibrary_WhenLoadingPosixLibrary_LoadsLibrary(params string[] foundLibs)
        {
            // Arrange
            MockPlatformAsPosix();

            this.mockDirectory.Setup(m => m.GetFiles("C:/test-dir/")).Returns(() =>
            {
                var result = new List<string>();

                foreach (var foundLib in foundLibs)
                {
                    result.Add($@"C:/test-dir/{foundLib}");
                }

                return result.ToArray();
            });

            foreach (var foundLib in foundLibs)
            {
                this.mockPath.Setup(m => m.GetFileName($@"C:/test-dir/{foundLib}"))
                    .Returns(foundLib);

                this.mockFile.Setup(m => m.Exists($@"C:/test-dir/{foundLib}")).Returns(true);
            }

            foreach (var lib in foundLibs)
            {
                this.mockPlatform.Setup(m => m.LoadLibrary($"{this.libDirPaths[0]}{lib}")).Returns(1234);
            }

            var loader = CreateLoader();

            // Act
            var actual = loader.LoadLibrary();

            // Assert
            Assert.Equal(1234, actual);
        }

        [Fact]
        public void LoadLibrary_WithMissingPosixLibraryInAllLibraryPaths_ThrowsException()
        {
            // Arrange
            MockPlatformAsPosix();
            this.mockDirectory.Setup(m => m.GetFiles(It.IsAny<string>())).Returns(Array.Empty<string>());

            var loader = CreateLoader();

            //Act & Assert
            Assert.ThrowsWithMessage<LoadLibraryException>(() =>
            {
                loader.LoadLibrary();
            }, $"Could not find the library '{LibraryName}'.\n\nPaths Checked: \n\t{this.libDirPaths[0]}\n");
        }
        #endregion

        /// <summary>
        /// Mocks a windows platform.
        /// </summary>
        private void MockPlatformAsWindows()
        {
            LibraryName = $"test-lib.dll";
            this.libDirPaths = new ReadOnlyCollection<string>(new[] { $@"C:\test-dir\" });
            this.libPath = $"{this.libDirPaths[0]}{LibraryName}";

            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(true);
            this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(false);
            this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(".dll");
            this.mockPlatform.Setup(m => m.Is32BitProcess()).Returns(false);
            this.mockPlatform.Setup(m => m.Is64BitProcess()).Returns(true);
            this.mockPlatform.Setup(m => m.LoadLibrary(this.libPath)).Returns(new IntPtr(1234));
            this.mockPlatform.Setup(m => m.GetLastSystemError()).Returns("Could not load module.");

            this.mockDependencyManager.SetupGet(p => p.LibraryDirPaths).Returns(this.libDirPaths);

            this.mockDirectory.Setup(m => m.Exists(this.libDirPaths[0])).Returns(true);

            this.mockFile.Setup(m => m.Exists(this.libPath)).Returns(true);

            this.mockPath.SetupGet(p => p.DirectorySeparatorChar).Returns('\\');
            this.mockPath.Setup(m => m.HasExtension(LibraryName)).Returns(true);
            this.mockPath.Setup(m => m.GetFileNameWithoutExtension(It.IsAny<string>()))
                .Returns(LibraryName.Replace(".dll", ""));

            this.mockLibrary.SetupGet(p => p.LibraryName).Returns(LibraryName);
        }

        /// <summary>
        /// Mocks a posix platform.
        /// </summary>
        private void MockPlatformAsPosix()
        {
            LibraryName = $"test-lib.so";
            this.libDirPaths = new ReadOnlyCollection<string>(new[] { $@"C:/test-dir/" });
            this.libPath = $"{this.libDirPaths[0]}{LibraryName}";

            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(false);
            this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(true);
            this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(".so");
            this.mockPlatform.Setup(m => m.Is32BitProcess()).Returns(false);
            this.mockPlatform.Setup(m => m.Is64BitProcess()).Returns(true);
            this.mockPlatform.Setup(m => m.LoadLibrary(this.libPath)).Returns(new IntPtr(1234));
            this.mockPlatform.Setup(m => m.GetLastSystemError()).Returns("Could not load module.");

            this.mockDependencyManager.SetupGet(p => p.LibraryDirPaths).Returns(this.libDirPaths);

            this.mockDirectory.Setup(m => m.Exists(this.libDirPaths[0])).Returns(true);
            this.mockDirectory.Setup(m => m.GetFiles(libDirPaths[0]))
                .Returns(() => new[] { $"{libDirPaths[0]}{LibraryName}" });

            this.mockFile.Setup(m => m.Exists(this.libPath)).Returns(true);

            this.mockPath.SetupGet(p => p.DirectorySeparatorChar).Returns('/');
            this.mockPath.Setup(m => m.HasExtension(LibraryName)).Returns(true);
            this.mockPath.Setup(m => m.GetFileName($"{libDirPaths[0]}{LibraryName}")).Returns(LibraryName);
            this.mockPath.Setup(m => m.GetFileNameWithoutExtension(It.IsAny<string>()))
                .Returns(LibraryName.Replace(".so", ""));

            this.mockLibrary.SetupGet(p => p.LibraryName).Returns(LibraryName);
        }

        /// <summary>
        /// Mocks the platform based on the given <paramref name="platform"/> type.
        /// </summary>
        /// <param name="platform">The platform to mock.</param>
        private void MockPlatformAs(PlatformType platform)
        {
            switch (platform)
            {
                case PlatformType.Windows:
                    MockPlatformAsWindows();
                    break;
                case PlatformType.Posix:
                    MockPlatformAsPosix();
                    break;
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LibraryLoader"/> class for the purpose of testing.
        /// </summary>
        /// <returns>The instance to test.</returns>
        private LibraryLoader CreateLoader()
            => new LibraryLoader(
                this.mockDependencyManager.Object,
                this.mockPlatform.Object,
                this.mockDirectory.Object,
                this.mockFile.Object,
                this.mockPath.Object,
                this.mockLibrary.Object);
    }
}
