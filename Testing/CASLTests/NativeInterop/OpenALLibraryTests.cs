// <copyright file="OpenALLibraryTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

#pragma warning disable IDE0002 // The name can be simplified
namespace CASLTests.NativeInterop
{
#pragma warning disable IDE0001 // The name can be simplified
    using CASL.Exceptions;
    using CASL.NativeInterop;
    using Moq;
    using Xunit;
    using Assert = CASLTests.Helpers.AssertExtensions;
#pragma warning restore IDE0001 // The name can be simplified

    /// <summary>
    /// Tests the <see cref="OpenALLibrary"/> class.
    /// </summary>
    public class OpenALLibraryTests
    {
        private Mock<IPlatform> mockPlatform;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALLibraryTests"/> class.
        /// </summary>
        public OpenALLibraryTests() => mockPlatform = new Mock<IPlatform>();

        #region Prop Tests
        [Fact]
        public void LibraryName_WhenGettingValueWithWindowsPlatform_ReturnsCorrectResult()
        {
            // Arrange
            MockWindowsPlatform();
            var library = new OpenALLibrary(this.mockPlatform.Object);

            // Act
            var actual = library.LibraryName;

            // Assert
            Assert.Equal("soft_oal.dll", actual);
        }

        [Fact]
        public void LibraryName_WhenGettingValueWithPosixPlatform_ReturnsCorrectResult()
        {
            // Arrange
            MockPosixPlatform();
            var library = new OpenALLibrary(this.mockPlatform.Object);

            // Act
            var actual = library.LibraryName;

            // Assert
            Assert.Equal("libopenal.so", actual);
        }

        [Fact]
        public void LibraryName_WithUnknownPlatform_ThrowsException()
        {
            // Arrange
            this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(false);
            this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(false);
            this.mockPlatform.SetupGet(p => p.CurrentPlatform).Returns("unknown-platform");

            var library = new OpenALLibrary(this.mockPlatform.Object);

            // Act & Assert
            Assert.ThrowsWithMessage<UnknownPlatformException>(() =>
            {
                _ = library.LibraryName;
            }, "The platform 'unknown-platform' is unknown.");
        }
        #endregion

        /// <summary>
        /// Mocks a windows platform.
        /// </summary>
        private void MockWindowsPlatform()
        {
            mockPlatform.Setup(m => m.IsWinPlatform()).Returns(true);
            mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(false);
        }

        /// <summary>
        /// Mocks a posix platform.
        /// </summary>
        private void MockPosixPlatform()
        {
            mockPlatform.Setup(m => m.IsWinPlatform()).Returns(false);
            mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(true);
        }
    }
}
