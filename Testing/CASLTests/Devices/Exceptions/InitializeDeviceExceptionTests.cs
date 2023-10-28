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
        // Arrange
        var expected = "There was an issue initializing the audio device.";

        // Act
        var exception = new InitializeDeviceException();

        // Assert
        exception.Message.Should().Be(expected);
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
        var expectedInnerExceptionMessage = "inner-exception";
        var expectedExceptionMessage = "test-exception";
        var innerException = new Exception(expectedInnerExceptionMessage);

        // Act
        var deviceException = new InitializeDeviceException(expectedExceptionMessage, innerException);

        // Assert
        deviceException.InnerException.Message.Should().Be(expectedInnerExceptionMessage);
        deviceException.Message.Should().Be(expectedExceptionMessage);
    }
    #endregion
}
