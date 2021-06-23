// <copyright file="LoadLibraryExceptionTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Exceptions
{
    using System;
    using CASL.Exceptions;
    using Xunit;

    /// <summary>
    /// Tests the <see cref="LoadLibraryException"/> class.
    /// </summary>
    public class LoadLibraryExceptionTests
    {
        #region Constructor Tests
        [Fact]
        public void Ctor_WhenInvokedWithNoParam_CorrectlySetsMessage()
        {
            // Act
            var exception = new LoadLibraryException();

            // Assert
            Assert.Equal("There was an issue loading the library.", exception.Message);
        }

        [Fact]
        public void Ctor_WhenInvokedWithSingleMessageParam_CorrectlySetsMesage()
        {
            // Act
            var exception = new LoadLibraryException("test-message");

            // Assert
            Assert.Equal("test-message", exception.Message);
        }

        [Fact]
        public void Ctor_WhenInvokedWithMessageAndInnerException_ThrowsException()
        {
            // Arrange
            var innerException = new Exception("inner-exception");

            // Act
            var deviceException = new LoadLibraryException("test-exception", innerException);

            // Assert
            Assert.Equal("inner-exception", deviceException.InnerException.Message);
            Assert.Equal("test-exception", deviceException.Message);
        }
        #endregion
    }
}
