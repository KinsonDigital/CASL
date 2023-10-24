// <copyright file="AudioDeviceManagerNotInitializedExceptionTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Devices.Exceptions;

using CASL.Devices.Exceptions;
using System;
using Xunit;
using FluentAssertions;

/// <summary>
/// Tests the <see cref="AudioDeviceManagerNotInitializedException"/> class.
/// </summary>
public class AudioDeviceManagerNotInitializedExceptionTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WhenInvokedWithNoParam_CorrectlySetsMessage()
    {
        // Act
        var exception = new AudioDeviceManagerNotInitializedException();

        // Assert
        var expectedMessage = "The audio device manager has not been initialized.";
        exception.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public void Ctor_WhenInvokedWithSingleMessageParam_CorrectlySetsMesage()
    {
        // Arrange
        var expected = "test-message";

        // Act
        var exception = new AudioDeviceManagerNotInitializedException(expected);

        // Assert
        exception.Message.Should().Be(expected);
    }

    [Fact]
    public void Ctor_WhenInvokedWithMessageAndInnerException_ThrowsException()
    {
        // Arrange
        var expectedInnerExceptionMessage = "inner-exception";
        var expectedExceptionMessage = "test-exception";
        var innerException = new Exception(expectedInnerExceptionMessage);

        // Act
        var deviceException = new AudioDeviceManagerNotInitializedException(expectedExceptionMessage, innerException);

        // Assert
        deviceException.InnerException.Message.Should().Be(expectedInnerExceptionMessage);
        deviceException.Message.Should().Be(expectedExceptionMessage);
    }
    #endregion
}
