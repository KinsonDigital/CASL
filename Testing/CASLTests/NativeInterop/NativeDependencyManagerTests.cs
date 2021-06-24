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
    using System.Linq;
    using CASL;
    using CASL.Exceptions;
    using CASL.NativeInterop;
    using CASLTests.Helpers;
    using Moq;
    using Xunit;
    using Assert = CASLTests.Helpers.AssertExtensions;
#pragma warning restore IDE0001 // Name can be simplified

    /// <summary>
    /// Tests the <see cref="NativeDependencyManager"/> class.
    /// </summary>
    public class NativeDependencyManagerTests
    {
        private readonly Mock<IPlatform> mockPlatform;
        private readonly Mock<IFile> mockFile;
        private readonly Mock<IPath> mockPath;
        private readonly Mock<IApplication> mockApp;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeDependencyManagerTests"/> class.
        /// </summary>
        public NativeDependencyManagerTests()
        {
            this.mockPlatform = new Mock<IPlatform>();
            this.mockFile = new Mock<IFile>();
            this.mockPath = new Mock<IPath>();
            this.mockApp = new Mock<IApplication>();
        }

        #region Constructor Tests
        [Fact]
        public void Ctor_WhenInvokedWithNullPlatform_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new OpenALDependencyManager(null, null, null, null);
            }, "The parameter must not be null. (Parameter 'platform')");
        }

        [Fact]
        public void Ctor_WhenInvokedWithNullFile_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new OpenALDependencyManager(new Mock<IPlatform>().Object, null, this.mockPath.Object, this.mockApp.Object);
            }, "The parameter must not be null. (Parameter 'file')");
        }

        [Fact]
        public void Ctor_WhenInvokedWithNullPath_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new OpenALDependencyManager(new Mock<IPlatform>().Object, this.mockFile.Object, null, this.mockApp.Object);
            }, "The parameter must not be null. (Parameter 'path')");
        }

        [Fact]
        public void Ctor_WhenInvokedWithNullApplication_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new OpenALDependencyManager(new Mock<IPlatform>().Object, this.mockFile.Object, this.mockPath.Object, null);
            }, "The parameter must not be null. (Parameter 'application')");
        }

        [Fact]
        public void Ctor_WithUnknownArchitecture_ThrowsException()
        {
            // Arrange
            MockProcessAsUnknown();

            // Act & Assert
            Assert.ThrowsWithMessage<InvalidOperationException>(() =>
            {
                _ = CreateManager();
            }, "Process Architecture Not Recognized.");
        }

        [Fact]
        public void Ctor_WithUnknownPlatform_ThrowsException()
        {
            // Arrange
            MockProcessAs32Bit();
            MockPlatformAsUnknown();

            // Act & Assert
            Assert.ThrowsWithMessage<UnknownPlatformException>(() =>
            {
                _ = CreateManager();
            }, "Unknown Operating System/Platform.");
        }
        #endregion

        #region Prop Tests
        [Fact]
        public void LibraryDirPaths_WhenGettingValue_ReturnsCorrectResult()
        {
            // Arrange
            var expected = new[] { @"C:\test-dir\" };

            MockProcessAs64Bit();
            MockPlatformAsWindows();

            var manager = CreateManager();

            // Act
            var actual = manager.LibraryDirPaths;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(PlatformType.Windows, ProcessType.x86, @"C:\test-dir\runtimes\win-x86\native\")]
        [InlineData(PlatformType.Posix, ProcessType.x86, @"C:/test-dir/runtimes/linux-x86/native/")]
        [InlineData(PlatformType.Windows, ProcessType.x64, @"C:\test-dir\runtimes\win-x64\native\")]
        [InlineData(PlatformType.Posix, ProcessType.x64, @"C:/test-dir/runtimes/linux-x64/native/")]
        public void NativeLibPath_WhenGettingValue_ReturnsCorrectResult(PlatformType platform, ProcessType processType, string expectedNativePath)
        {
            // Arrange
            MockPlatformAs(platform);
            MockProcessAs(processType);

            var manager = CreateManager();

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
            MockPlatformAsWindows();

            var manager = CreateManager();

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

            var manager = CreateManager();
            // Act
            manager.NativeLibraries = new ReadOnlyCollection<string>(new List<string> { "test-native-lib.dll" });
            var actual = manager.NativeLibraries;

            // Assert
            Assert.Single(actual);
            Assert.Equal("test-native-lib.dll", actual[0]);
        }
        #endregion

        #region Method Tests
        [Fact]
        public void SetupDependencies_WhenLibraryDoesNotExistInDestination_CopiesLibraryFile()
        {
            // Arrange
            var assemblyDirPath = @"C:\test-dir\";
            var srcDirPath = $@"{assemblyDirPath}runtimes\win-x64\native\";

            MockProcessAs64Bit();
            MockPlatformAsWindows();

            this.mockFile.Setup(m => m.Exists($"{srcDirPath}lib-A.dll")).Returns(true);
            this.mockFile.Setup(m => m.Exists($"{srcDirPath}lib-B.dll")).Returns(true);

            var manager = CreateManager();
            manager.NativeLibraries = new ReadOnlyCollection<string>(new[] { "lib-A.dll", "lib-B.dll" }.ToList());

            // Act
            manager.SetupDependencies();

            // Assert
            this.mockFile.Verify(m => m.Copy($"{srcDirPath}lib-A.dll", $"{assemblyDirPath}lib-A.dll", true), Times.Once());
            this.mockFile.Verify(m => m.Copy($"{srcDirPath}lib-B.dll", $"{assemblyDirPath}lib-B.dll", true), Times.Once());
        }

        [Fact]
        public void SetupDependencies_WhenLibraryAlreadyExistsInDestination_DoNotCopyFile()
        {
            // Arrange
            var assemblyDirPath = @"C:\test-dir\";
            var srcDirPath = $@"{assemblyDirPath}runtimes\win-x64\native\";

            MockProcessAs64Bit();
            MockPlatformAsWindows();

            this.mockFile.Setup(m => m.Exists($"{assemblyDirPath}lib.dll")).Returns(true);

            var manager = CreateManager();
            manager.NativeLibraries = new ReadOnlyCollection<string>(new[] { "lib.dll" }.ToList());

            // Act
            manager.SetupDependencies();

            // Assert
            this.mockFile.Verify(m => m.Exists($"{srcDirPath}lib.dll"), Times.Never());
            this.mockFile.Verify(m => m.Copy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Fact]
        public void SetupDependencies_WhenLibrarySrcDoesNotExist_ThrowsException()
        {
            // Arrange
            var assemblyDirPath = @"C:\test-dir\";
            var srcDirPath = $@"{assemblyDirPath}runtimes\win-x64\native\";

            MockProcessAs64Bit();
            MockPlatformAsWindows();

            this.mockFile.Setup(m => m.Exists($"{assemblyDirPath}lib.dll")).Returns(false);
            this.mockFile.Setup(m => m.Exists($"{srcDirPath}lib.dll")).Returns(false);

            var manager = CreateManager();
            manager.NativeLibraries = new ReadOnlyCollection<string>(new[] { "lib.dll" }.ToList());

            // Act & Assert
            Assert.ThrowsWithMessage<FileNotFoundException>(() =>
            {
                manager.SetupDependencies();
            }, $"The native dependency library '{srcDirPath}lib.dll' does not exist.");
        }
        #endregion

        /// <summary>
        /// Creates a new instance of <see cref="OpenALDependencyManager"/> for the purpose of testing.
        /// </summary>
        /// <returns>The instance to test.</returns>
        private OpenALDependencyManager CreateManager()
            => new OpenALDependencyManager(this.mockPlatform.Object, this.mockFile.Object, this.mockPath.Object, this.mockApp.Object);

        /// <summary>
        /// Mocks the platform as a windows platform.
        /// </summary>
        private void MockPlatformAsWindows()
        {
            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(true);
            this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(false);
            this.mockApp.SetupGet(p => p.Location).Returns(@"C:\app-dir\");
            this.mockPath.SetupGet(p => p.DirectorySeparatorChar).Returns('\\');
            this.mockPath.Setup(m => m.GetDirectoryName(It.IsAny<string>())).Returns(@"C:\test-dir");
        }

        /// <summary>
        /// Mocks the platform as a posix platform.
        /// </summary>
        private void MockPlatformAsPosix()
        {
            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(false);
            this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(true);
            this.mockPath.Setup(m => m.DirectorySeparatorChar).Returns('/');
            this.mockApp.SetupGet(p => p.Location).Returns("C:/app-dir/");
            this.mockPath.SetupGet(p => p.DirectorySeparatorChar).Returns('/');
            this.mockPath.Setup(m => m.GetDirectoryName(It.IsAny<string>())).Returns(@"C:/test-dir");
        }

        /// <summary>
        /// Mocks the given <paramref name="platform"/>.
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
        /// Mocks the platform as unknown.
        /// </summary>
        private void MockPlatformAsUnknown()
        {
            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(false);
            this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(false);
            this.mockPath.Setup(m => m.DirectorySeparatorChar).Returns('\\');
        }

        /// <summary>
        /// Mocks the process as 32 bit.
        /// </summary>
        private void MockProcessAs32Bit()
        {
            this.mockPlatform.Setup(m => m.Is32BitProcess()).Returns(true);
            this.mockPlatform.Setup(m => m.Is64BitProcess()).Returns(false);
        }

        /// <summary>
        /// Mocks the process as 64 bit.
        /// </summary>
        private void MockProcessAs64Bit()
        {
            this.mockPlatform.Setup(m => m.Is32BitProcess()).Returns(false);
            this.mockPlatform.Setup(m => m.Is64BitProcess()).Returns(true);
        }

        /// <summary>
        /// Mocks the given <paramref name="process"/>.
        /// </summary>
        /// <param name="process">The type of process to mock.</param>
        private void MockProcessAs(ProcessType process)
        {
            switch (process)
            {
                case ProcessType.x86:
                    MockProcessAs32Bit();
                    break;
                case ProcessType.x64:
                    MockProcessAs64Bit();
                    break;
            }
        }

        /// <summary>
        /// Mocks the process as unknown.
        /// </summary>
        private void MockProcessAsUnknown()
        {
            this.mockPlatform.Setup(m => m.Is32BitProcess()).Returns(false);
            this.mockPlatform.Setup(m => m.Is64BitProcess()).Returns(false);
        }
    }
}
