// <copyright file="InitializeDeviceExceptionTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Devices.Exceptions
{
    using System;
    using CASL.Devices.Exceptions;
    using Xunit;

    /// <summary>
    /// Tests the <see cref="InitializeDeviceException"/> class.
    /// </summary>
    public class InitializeDeviceExceptionTests
    {
        #region Constructor Tests
        [Fact]
        public void Ctor_WhenInvokedWithNoParam_CorrectlySetsMessage()
        {
            // Act
            var exception = new InitializeDeviceException();

            // Assert
            Assert.Equal("There was an issue initializing the audio device.", exception.Message);
        }

        [Fact]
        public void Ctor_WhenInvokedWithSingleMessageParam_CorrectlySetsMesage()
        {
            // Act
            var exception = new InitializeDeviceException("test-message");

            // Assert
            Assert.Equal("test-message", exception.Message);
        }

        [Fact]
        public void Ctor_WhenInvokedWithMessageAndDeviceNameParams_CorrectlySetsMessage()
        {
            // Act
            var exception = new InitializeDeviceException("test-message");

            // Assert
            Assert.Equal("test-message", exception.Message);
        }

        [Fact]
        public void Ctor_WhenInvokedWithMessageAndInnerException_ThrowsException()
        {
            // Arrange
            var innerException = new Exception("inner-exception");

            // Act
            var deviceException = new InitializeDeviceException("test-exception", innerException);

            // Assert
            Assert.Equal("inner-exception", deviceException.InnerException.Message);
            Assert.Equal("test-exception", deviceException.Message);
        }
        #endregion
    }
}
