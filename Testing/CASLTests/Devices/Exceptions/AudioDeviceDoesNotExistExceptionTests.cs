// <copyright file="AudioDeviceDoesNotExistExceptionTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Devices.Exceptions;

using CASL.Devices.Exceptions;
using System;
using Xunit;
using FluentAssertions;

/// <summary>
/// Tests the <see cref="AudioDeviceDoesNotExistException"/> class.
/// </summary>
public class AudioDeviceDoesNotExistExceptionTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WhenInvokedWithNoParam_CorrectlySetsMessage()
    {
        // Arrange
        var expected = "The audio device does not exist.";

        // Act
        var exception = new AudioDeviceDoesNotExistException();

        // Assert
        exception.Message.Should().Be(expected);
    }

    [Fact]
    public void Ctor_WhenInvokedWithSingleMessageParam_CorrectlySetsMessage()
    {
        // Arrange
        var expected  = "test-message";

        // Act
        var exception = new AudioDeviceDoesNotExistException(expected);

        // Assert
        exception.Message.Should().Be(expected);
    }

    [Fact]
    public void Ctor_WhenInvokedWithMessageAndDeviceNameParams_CorrectlySetsMessage()
    {
        // Arrange
        var expectedDeviceName = "device";
        var expectedMessage = "test-message";
        var expectedExceptionMessage = $"Device Name: {expectedDeviceName}\n{expectedMessage}";

        // Act
        var exception = new AudioDeviceDoesNotExistException(expectedMessage, expectedDeviceName);

        // Assert
        exception.Message.Should().Be(expectedExceptionMessage);

    }

    [Fact]
    public void Ctor_WhenInvokedWithMessageAndInnerException_ThrowsException()
    {
        // Arrange
        var expectedMessage =  "test-exception";
        var expectedInnerMessage = "inner-exception";
        var innerException = new Exception(expectedInnerMessage);

        // Act
        var deviceException = new AudioDeviceDoesNotExistException(expectedMessage, innerException);

        // Assert
        deviceException.InnerException.Message.Should().Be(expectedInnerMessage);
        deviceException.Message.Should().Be(expectedMessage);
    }
    #endregion
}
