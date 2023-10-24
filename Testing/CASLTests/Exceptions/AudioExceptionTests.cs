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
        // Arrange
        var expected = "test-message";

        // Act
        var exception = new AudioException(expected);

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
        var deviceException = new AudioException(expectedExceptionMessage, innerException);

        // Assert
        deviceException.InnerException.Message.Should().Be(expectedInnerExceptionMessage);
        deviceException.Message.Should().Be(expectedExceptionMessage);
    }
    #endregion
}
