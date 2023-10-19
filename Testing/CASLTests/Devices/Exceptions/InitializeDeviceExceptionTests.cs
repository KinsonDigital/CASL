// <copyright file="InitializeDeviceExceptionTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Devices.Exceptions;

using System;
using CASL.Devices.Exceptions;
using Xunit;
using FluentAssertions;

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
        var expectedMessage = "There was an issue initializing the audio device.";
        exception.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public void Ctor_WhenInvokedWithMessageAndDeviceNameParams_CorrectlySetsMessage()
    {
        // Act
        var exceptionMessage = "test-message";
        var exception = new InitializeDeviceException(exceptionMessage);

        // Assert
        exception.Message.Should().Be(exceptionMessage);
    }

    [Fact]
    public void Ctor_WhenInvokedWithMessageAndInnerException_ThrowsException()
    {
        // Arrange
        var innerExceptionMessage = "inner-exception";
        var innerException = new Exception(innerExceptionMessage);

        // Act
        var exceptionMessage = "test-exception";
        var deviceException = new InitializeDeviceException(exceptionMessage, innerException);

        // Assert
        deviceException.InnerException.Message.Should().Be(innerExceptionMessage);
        deviceException.Message.Should().Be(exceptionMessage);
    }
    #endregion
}
