// <copyright file="AudioExceptionTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Exceptions;

using System;
using CASL.Exceptions;
using Xunit;
using Shouldly;

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
        exception.Message.ShouldBe("An audio exception has occurred.");
    }

    [Fact]
    public void Ctor_WhenInvokedWithSingleMessageParam_CorrectlySetsMessage()
    {
        // Arrange
        var expected = "test-message";

        // Act
        var exception = new AudioException(expected);

        // Assert
        exception.Message.ShouldBe(expected);
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
        deviceException.InnerException.Message.ShouldBe(expectedInnerExceptionMessage);
        deviceException.Message.ShouldBe(expectedExceptionMessage);
    }
    #endregion
}
