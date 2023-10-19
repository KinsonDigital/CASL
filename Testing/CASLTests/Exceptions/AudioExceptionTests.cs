// <copyright file="AudioExceptionTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Exceptions;

using System;
using CASL.Exceptions;
using Xunit;
using FluentAssertions;

/// <summary>
/// Tests the <see cref="AudioException"/> class.
/// </summary>
public class AudioExceptionTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WhenInvokedWithNoParam_CorrectlySetsMessage()
    {
        // Act
        var exception = new AudioException();

        // Assert
        exception.Message.Should().Be("An audio exception has occurred.");
    }

    [Fact]
    public void Ctor_WhenInvokedWithSingleMessageParam_CorrectlySetsMesage()
    {
        // Act
        var exceptionMessage = "test-message";
        var exception = new AudioException(exceptionMessage);

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
        var deviceException = new AudioException(exceptionMessage, innerException);

        // Assert
        deviceException.InnerException.Message.Should().Be(innerExceptionMessage);
        deviceException.Message.Should().Be(exceptionMessage);
    }
    #endregion
}
