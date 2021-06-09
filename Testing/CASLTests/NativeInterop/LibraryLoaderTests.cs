// <copyright file="LibraryLoaderTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

#pragma warning disable IDE0002 // Name can be simplified
namespace CASLTests
{
#pragma warning disable IDE0001 // Name can be simplified
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.IO.Abstractions;
    using CASL.NativeInterop;
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
        private const string WinLibraryName = "test-lib.dll";
        private const string PosixLibraryName = "test-lib.so";
        private readonly char DirSeperator;
        private readonly string libPath;
        private readonly ReadOnlyCollection<string> libDirPaths;
        private readonly Mock<IDependencyManager> mockDependencyManager;
        private readonly Mock<IPlatform> mockPlatform;
        private readonly Mock<IDirectory> mockDirectory;
        private readonly Mock<IFile> mockFile;
        private readonly Mock<ILibrary> mockLibrary;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoaderTests"/> class.
        /// </summary>
        public LibraryLoaderTests()
        {
            DirSeperator = Path.DirectorySeparatorChar;
            this.libDirPaths = new ReadOnlyCollection<string>(new[] { $@"C:{this.DirSeperator}test-dir{this.DirSeperator}" });
            this.libPath = $"{this.libDirPaths[0]}{WinLibraryName}";
            this.mockDependencyManager = new Mock<IDependencyManager>();
            this.mockDependencyManager.SetupGet(p => p.LibraryDirPaths).Returns(this.libDirPaths);

            this.mockPlatform = new Mock<IPlatform>();
            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(true);
            this.mockPlatform.Setup(m => m.Is64BitProcess()).Returns(true);
            this.mockPlatform.Setup(m => m.LoadLibrary(this.libPath)).Returns(new IntPtr(1234));
            this.mockPlatform.Setup(m => m.GetLastSystemError()).Returns("Could not load module.");
            this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(".dll");

            this.mockDirectory = new Mock<IDirectory>();
            this.mockDirectory.Setup(m => m.Exists(this.libDirPaths[0])).Returns(true);

            this.mockFile = new Mock<IFile>();
            this.mockFile.Setup(m => m.Exists(this.libPath)).Returns(true);

            this.mockLibrary = new Mock<ILibrary>();
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns(WinLibraryName);
        }
        #endregion

        #region Method Tests
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
                    this.mockLibrary.Object);
            }, "The parameter must not be null. (Parameter 'file')");
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
                    null);
            }, "The parameter must not be null. (Parameter 'library')");
        }

        [Fact]
        public void Ctor_WhenUsingLibraryNameWithIncorrectLibraryExtension_FixesExtension()
        {
            //Arrange
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns("test-lib.txt");

            //Act
            var loader = CreateLoader();

            //Assert
            Assert.Equal("test-lib.dll", loader.LibraryName);
        }

        [Fact]
        public void Ctor_WhenUsingLibraryNameWithoutExtension_FixesExtension()
        {
            //Arrange
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns("test-lib");

            //Act
            var loader = CreateLoader();

            //Assert
            Assert.Equal("test-lib.dll", loader.LibraryName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Ctor_WhenInvokedWithNullOrEmptyWindowsLibraryName_ThrowsException(string winLibraryName)
        {
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns(winLibraryName);

            //Act & //Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = CreateLoader();
            }, "The library name must not be null or empty. (Parameter 'libraryName')");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Ctor_WhenInvokedWithNullOrEmptyPosixLibraryName_ThrowsException(string winLibraryName)
        {
            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(false);
            this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(true);
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns(winLibraryName);

            //Act & //Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = CreateLoader();
            }, "The library name must not be null or empty. (Parameter 'libraryName')");
        }

        [Fact]
        public void LoadLibrary_WhenWindowsLibraryDoesNotLoad_ThrowsException()
        {
            //Arrange
            this.mockPlatform.Setup(m => m.LoadLibrary(this.libPath)).Returns(IntPtr.Zero);

            var loader = CreateLoader();

            //Act & Assert
            Assert.ThrowsWithMessage<Exception>(() =>
            {
                loader.LoadLibrary();
            }, "Could not load module.\n\nLibrary Path: 'C:\\test-dir\\test-lib.dll'\n\nSystem Error Codes: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-");
        }

        [Fact]
        public void LoadLibrary_WhenPosixLibraryDoesNotLoad_ThrowsException()
        {
            // Arrange
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns(PosixLibraryName);

            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(false);
            this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(true);
            this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(".so");

            this.mockPlatform.Setup(m => m.LoadLibrary(It.IsAny<string>())).Returns(0);

            this.mockDirectory.Setup(m => m.GetFiles(this.libDirPaths[0])).Returns(() => new[] { PosixLibraryName });

            this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(true);

            var loader = CreateLoader();

            // Act & Assert
            Assert.ThrowsWithMessage<Exception>(() =>
            {
                loader.LoadLibrary();
            }, "Could not load module.\n\nLibrary Path: 'C:\\test-dir\\test-lib.so'");
        }

        [Fact]
        public void LoadLibrary_WhenWindowsLibraryFileDoesNotExist_ThrowsException()
        {
            //Arrange
            this.mockFile.Setup(m => m.Exists(this.libPath)).Returns(false);

            var loader = CreateLoader();

            //Act & Assert
            Assert.ThrowsWithMessage<Exception>(() =>
            {
                loader.LoadLibrary();
            }, $"Could not find the library 'test-lib.dll'.\n\nPaths Checked: \n\tC:\\test-dir\\\n");
        }

        [Fact]
        public void LoadLibrary_WhenLibPathEndsWithPathSeparator_LoadsLibrary()
        {
            //Arrange
            var loader = CreateLoader();

            //Act
            var actual = loader.LoadLibrary();

            //Assert
            Assert.Equal(1234, actual);
        }

        [Fact]
        public void LoadLibrary_WhenLibPathEndsWithoutPathSeparator_LoadsLibrary()
        {
            //Arrange
            var libPaths = new ReadOnlyCollection<string>(new[] { $@"C:\test-dir" });
            this.mockPlatform.Setup(m => m.LoadLibrary(libPaths[0])).Returns(new IntPtr(1234));
            this.mockDependencyManager.SetupGet(p => p.LibraryDirPaths).Returns(libPaths);

            var loader = CreateLoader();

            //Act
            var actual = loader.LoadLibrary();

            //Assert
            Assert.Equal(1234, actual);
        }

        [Fact]
        public void LoadLibrary_WhenLoadingPosixLibraryThatDoesNotExit_ThrowsException()
        {
            // Arrange
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns(PosixLibraryName);

            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(false);
            this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(true);
            this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(".so");

            this.mockPlatform.Setup(m => m.LoadLibrary($"{this.libDirPaths[0]}{PosixLibraryName}")).Returns(1234);

            this.mockDirectory.Setup(m => m.GetFiles(this.libDirPaths[0]))
                .Returns(Array.Empty<string>());

            this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(true);

            var loader = CreateLoader();

            // Act & Assert
            Assert.ThrowsWithMessage<Exception>(() =>
            {
                loader.LoadLibrary();
            }, "Could not find the library 'test-lib.so'.\nPaths Checked: \n\tC:\\test-dir\\\n");
        }

        [Fact]
        public void LoadLibrary_WhenLoadingPosixLibraryThatCannotBeFound_ThrowsException()
        {
            // Arrange
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns(PosixLibraryName);

            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(false);
            this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(true);
            this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(".so");

            this.mockPlatform.Setup(m => m.LoadLibrary(It.IsAny<string>())).Returns(0);

            this.mockDirectory.Setup(m => m.GetFiles(this.libDirPaths[0]))
                .Returns(new[] { PosixLibraryName });

            this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(false);

            var loader = CreateLoader();

            // Act & Assert
            Assert.ThrowsWithMessage<Exception>(() =>
            {
                loader.LoadLibrary();
            }, "Could not find the library 'test-lib.so'.\nPaths Checked: \n\tC:\\test-dir\\\n");
        }

        [Theory]
        [InlineData(PosixLibraryName)]
        [InlineData("test-lib.so.NaN")]
        [InlineData("test-lib.so.1")]
        [InlineData(PosixLibraryName, "test-lib.so.0", "test-lib.so.1")]
        [InlineData("test-lib.so.1", "test-lib.so.0", PosixLibraryName)]
        [InlineData(PosixLibraryName, "test-lib.so.1", "test-lib.so.0")]
        public void LoadLibrary_WhenLoadingPosixLibrary_LoadsLibrary(params string[] foundLibs)
        {
            // Arrange
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns(PosixLibraryName);
            
            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(false);
            this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(true);
            this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(".so");

            foreach (var lib in foundLibs)
            {
                this.mockPlatform.Setup(m => m.LoadLibrary($"{this.libDirPaths[0]}{lib}")).Returns(1234);
            }

            this.mockDirectory.Setup(m => m.GetFiles(this.libDirPaths[0])).Returns(foundLibs);
            this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(true);

            var loader = CreateLoader();

            // Act
            var actual = loader.LoadLibrary();

            // Assert
            Assert.Equal(1234, actual);
        }
        #endregion

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
                this.mockLibrary.Object);
    }
}
