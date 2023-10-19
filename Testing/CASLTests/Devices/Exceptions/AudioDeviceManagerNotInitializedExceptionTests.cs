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
        // Act
        var exceptionMessage = "test-message";
        var exception = new AudioDeviceManagerNotInitializedException(exceptionMessage);

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
        var deviceException = new AudioDeviceManagerNotInitializedException(exceptionMessage, innerException);

        // Assert
        deviceException.InnerException.Message.Should().Be(innerExceptionMessage);
        deviceException.Message.Should().Be(exceptionMessage);
    }
    #endregion
}
