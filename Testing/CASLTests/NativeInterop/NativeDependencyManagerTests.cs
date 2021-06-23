// <copyright file="NativeDependencyManagerTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

#pragma warning disable IDE0002 // Name can be simplified
namespace CASLTests.NativeInterop
{
#pragma warning disable IDE0001 // Name can be simplified
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.IO.Abstractions;
    using System.Reflection;
    using CASL.Exceptions;
    using CASL.NativeInterop;
    using CASLTests.Fakes;
    using Moq;
    using Xunit;
    using Assert = CASLTests.Helpers.AssertExtensions;
#pragma warning restore IDE0001 // Name can be simplified

    /// <summary>
    /// Tests the <see cref="NativeDependencyManager"/> class.
    /// </summary>
    public class NativeDependencyManagerTests
    {
        private readonly string ExecutingAssemblyPath = Assembly.GetExecutingAssembly().Location;
        private readonly string AssemblyDirName;
        private readonly Mock<IPlatform> mockPlatform;
        private readonly Mock<IFile> mockFile;
        private readonly Mock<IPath> mockPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeDependencyManagerTests"/> class.
        /// </summary>
        public NativeDependencyManagerTests()
        {
            this.mockPlatform = new Mock<IPlatform>();
            this.mockFile = new Mock<IFile>();

            AssemblyDirName = Path.GetDirectoryName(ExecutingAssemblyPath) ?? string.Empty;
            this.mockPath = new Mock<IPath>();
            this.mockPath.Setup(m => m.GetDirectoryName(It.IsAny<string>())).Returns(@"C:\test-dir");
        }

        #region Constructor Tests
        [Fact]
        public void Ctor_WhenInvokedWithNullPlatform_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new NativeDependencyManagerFake(null, null, null);
            }, "The parameter must not be null. (Parameter 'platform')");
        }

        [Fact]
        public void Ctor_WhenInvokedWithNullFile_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new NativeDependencyManagerFake(new Mock<IPlatform>().Object, null, this.mockPath.Object);
            }, "The parameter must not be null. (Parameter 'file')");
        }

        [Fact]
        public void Ctor_WhenInvokedWithNullPath_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new NativeDependencyManagerFake(new Mock<IPlatform>().Object, this.mockFile.Object, null);
            }, "The parameter must not be null. (Parameter 'path')");
        }

        [Fact]
        public void Ctor_WithUnknownArchitecture_ThrowsException()
        {
            // Arrange
            this.mockPlatform.Setup(m => m.Is32BitProcess()).Returns(false);
            this.mockPlatform.Setup(m => m.Is64BitProcess()).Returns(false);

            // Act & Assert
            Assert.ThrowsWithMessage<InvalidOperationException>(() =>
            {
                _ = new NativeDependencyManagerFake(this.mockPlatform.Object, this.mockFile.Object, this.mockPath.Object);
            }, "Process Architecture Not Recognized.");
        }

        [Fact]
        public void Ctor_WithUnknownPlatform_ThrowsException()
        {
            // Arrange
            this.mockPlatform.Setup(m => m.Is32BitProcess()).Returns(false);
            this.mockPlatform.Setup(m => m.Is64BitProcess()).Returns(true);
            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(false);
            this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(false);

            // Act & Assert
            Assert.ThrowsWithMessage<UnknownPlatformException>(() =>
            {
                _ = new NativeDependencyManagerFake(this.mockPlatform.Object, this.mockFile.Object, this.mockPath.Object);
            }, "Unknown Operating System/Platform.");
        }
        #endregion

        #region Prop Tests

        [Fact]
        public void LibraryDirPaths_WhenGettingValue_ReturnsCorrectResult()
        {
            // Arrange
            var expected = new[] { @"C:\test-dir\" };

            this.mockPlatform.Setup(m => m.Is32BitProcess()).Returns(false);
            this.mockPlatform.Setup(m => m.Is64BitProcess()).Returns(true);
            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(true);
            this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(false);

            var manager = new NativeDependencyManagerFake(this.mockPlatform.Object, this.mockFile.Object, this.mockPath.Object);

            // Act
            var actual = manager.LibraryDirPaths;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(true, true, @"C:\test-dir\runtimes\win-x86\native\")]
        [InlineData(true, false, @"C:\test-dir\runtimes\linux-x86\native\")]
        [InlineData(false, true, @"C:\test-dir\runtimes\win-x64\native\")]
        [InlineData(false, false, @"C:\test-dir\runtimes\linux-x64\native\")]
        public void NativeLibPath_WhenGettingValue_ReturnsCorrectResult(bool is32Bit, bool isWindows, string expectedNativePath)
        {
            // Arrange
            this.mockPlatform.Setup(m => m.Is32BitProcess()).Returns(is32Bit);
            this.mockPlatform.Setup(m => m.Is64BitProcess()).Returns(!is32Bit);
            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(isWindows);
            this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(!isWindows);

            var manager = new NativeDependencyManagerFake(this.mockPlatform.Object, this.mockFile.Object, this.mockPath.Object);

            // Act
            var actual = manager.NativeLibPath;

            // Assert
            Assert.Equal(expectedNativePath, actual);
        }

        [Fact]
        public void NativeLibraries_WhenGettingNullValue_ReturnsCorrecResult()
        {
            // Arrange
            this.mockPlatform.Setup(m => m.Is64BitProcess()).Returns(true);
            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(true);

            var manager = new NativeDependencyManagerFake(this.mockPlatform.Object, this.mockFile.Object, this.mockPath.Object);

            // Act
            manager.NativeLibraries = null;
            var actual = manager.NativeLibraries;

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public void NativeLibraries_WhenSettingValue_ReturnsCorrecResult()
        {
            // Arrange
            this.mockPlatform.Setup(m => m.Is64BitProcess()).Returns(true);
            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(true);

            var manager = new NativeDependencyManagerFake(this.mockPlatform.Object, this.mockFile.Object, this.mockPath.Object);
            // Act
            manager.NativeLibraries = new ReadOnlyCollection<string>(new List<string> { "test-native-lib.dll" });
            var actual = manager.NativeLibraries;

            // Assert
            Assert.Single(actual);
            Assert.Equal("test-native-lib.dll", actual[0]);
        }
        #endregion
    }
}
