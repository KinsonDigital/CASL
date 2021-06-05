// <copyright file="LibraryLoaderTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

#pragma warning disable IDE0002 // Name can be simplified
namespace CASLTests
{
#pragma warning disable IDE0001 // Name can be simplified
    using System;
    using System.Collections.ObjectModel;
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
        private const string LibraryName = "test-lib.dll";
        private readonly string libPath;
        private readonly ReadOnlyCollection<string> libDirPath;
        private readonly Mock<IDependencyManager> mockDependencyManager;
        private readonly Mock<IPlatform> mockPlatform;
        private readonly Mock<IFile> mockFile;
        private readonly Mock<ILibrary> mockLibrary;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoaderTests"/> class.
        /// </summary>
        public LibraryLoaderTests()
        {
            this.libDirPath = new ReadOnlyCollection<string>(new[] { $@"C:\test-dir\" });
            this.libPath = $"{this.libDirPath[0]}{LibraryName}";
            this.mockDependencyManager = new Mock<IDependencyManager>();
            this.mockDependencyManager.SetupGet(p => p.LibraryDirPaths).Returns(this.libDirPath);

            this.mockPlatform = new Mock<IPlatform>();
            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(true);
            this.mockPlatform.Setup(m => m.Is64BitProcess()).Returns(true);
            this.mockPlatform.Setup(m => m.LoadLibrary(this.libPath)).Returns(new IntPtr(1234));
            this.mockPlatform.Setup(m => m.GetLastSystemError()).Returns("Could not load module.");
            this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(".dll");

            this.mockFile = new Mock<IFile>();
            this.mockFile.Setup(m => m.Exists(this.libPath)).Returns(true);

            this.mockLibrary = new Mock<ILibrary>();
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns(LibraryName);
        }
        #endregion

        #region Method Tests
        [Fact]
        public void Ctor_WithNullDependencyManager_ThrowsException()
        {
            //Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                var loader = new LibraryLoader(null, this.mockPlatform.Object, this.mockFile.Object, this.mockLibrary.Object);
            }, "The parameter must not be null. (Parameter 'dependencyManager')");
        }

        [Fact]
        public void Ctor_WithNullPlatform_ThrowsException()
        {
            //Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                var loader = new LibraryLoader(this.mockDependencyManager.Object, null, this.mockFile.Object, this.mockLibrary.Object);
            }, "The parameter must not be null. (Parameter 'platform')");
        }

        [Fact]
        public void Ctor_WithNullFileObject_ThrowsException()
        {
            //Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                var loader = new LibraryLoader(this.mockDependencyManager.Object, this.mockPlatform.Object, null, this.mockLibrary.Object);
            }, "The parameter must not be null. (Parameter 'libFile')");
        }


        [Fact]
        public void Ctor_WithNullLibrary_ThrowsException()
        {
            //Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                var loader = new LibraryLoader(this.mockDependencyManager.Object, this.mockPlatform.Object, this.mockFile.Object, null);
            }, "The parameter must not be null. (Parameter 'library')");
        }

        [Fact]
        public void Ctor_WhenUsingLibraryNameWithIncorrectLibraryExtension_FixesExtension()
        {
            //Arrange
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns("test-lib.txt");

            //Act
            var loader = new LibraryLoader(this.mockDependencyManager.Object, this.mockPlatform.Object, this.mockFile.Object, this.mockLibrary.Object);

            //Assert
            Assert.Equal("test-lib.dll", loader.LibraryName);
        }

        [Fact]
        public void Ctor_WhenUsingLibraryNameWithoutExtension_FixesExtension()
        {
            //Arrange
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns("test-lib");

            //Act
            var loader = new LibraryLoader(this.mockDependencyManager.Object, this.mockPlatform.Object, this.mockFile.Object, this.mockLibrary.Object);

            //Assert
            Assert.Equal("test-lib.dll", loader.LibraryName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Ctor_WhenInvokedWithNullOrEmptyLibraryName_ThrowsException(string libName)
        {
            this.mockLibrary.SetupGet(p => p.LibraryName).Returns(libName);

            //Act & //Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new LibraryLoader(this.mockDependencyManager.Object, this.mockPlatform.Object, this.mockFile.Object, this.mockLibrary.Object);
            }, "The library name must not be null or empty. (Parameter 'libraryName')");
        }

        [Fact]
        public void LoadLibrary_WhenLibraryDoesNotLoad_ThrowsException()
        {
            //Arrange
            this.mockPlatform.Setup(m => m.LoadLibrary(this.libPath)).Returns(IntPtr.Zero);

            var loader = new LibraryLoader(this.mockDependencyManager.Object, this.mockPlatform.Object, this.mockFile.Object, this.mockLibrary.Object);

            //Act & Assert
            Assert.ThrowsWithMessage<Exception>(() =>
            {
                loader.LoadLibrary();
            }, "Could not load module.\n\nLibrary Path: 'C:\\test-dir\\test-lib.dll'\n\nSystem Error Codes: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-");
        }

        [Fact]
        public void LoadLibrary_WhenLibraryFileDoesNotExist_ThrowsException()
        {
            //Arrange
            this.mockFile.Setup(m => m.Exists(this.libPath)).Returns(false);

            var loader = new LibraryLoader(this.mockDependencyManager.Object, this.mockPlatform.Object, this.mockFile.Object, this.mockLibrary.Object);

            //Act & Assert
            Assert.ThrowsWithMessage<Exception>(() =>
            {
                loader.LoadLibrary();
            }, $"Could not find the library 'test-lib.dll'.\n\nPaths Checked: \n\tC:\\test-dir\\\\n");
        }

        [Fact]
        public void LoadLibrary_WhenLibPathEndsWithPathSeparator_SuccessfullyLoadsLibrary()
        {
            //Arrange
            var loader = new LibraryLoader(this.mockDependencyManager.Object, this.mockPlatform.Object, this.mockFile.Object, this.mockLibrary.Object);

            //Act
            var actual = loader.LoadLibrary();

            //Assert
            Assert.Equal(new IntPtr(1234), actual);
        }

        [Fact]
        public void LoadLibrary_WhenLibPathEndsWithoutPathSeparator_LoadsLibrary()
        {
            //Arrange
            var libPaths = new ReadOnlyCollection<string>(new[] { $@"C:\test-dir" });
            this.mockPlatform.Setup(m => m.LoadLibrary(libPaths[0])).Returns(new IntPtr(1234));
            this.mockDependencyManager.SetupGet(p => p.LibraryDirPaths).Returns(libPaths);

            var loader = new LibraryLoader(this.mockDependencyManager.Object, this.mockPlatform.Object, this.mockFile.Object, this.mockLibrary.Object);

            //Act
            var actual = loader.LoadLibrary();

            //Assert
            Assert.Equal(new IntPtr(1234), actual);
        }
        #endregion
    }
}
