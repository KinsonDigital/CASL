// <copyright file="UnknownPlatformExceptionTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Exceptions;

using System;
using CASL.Exceptions;
using Xunit;
using FluentAssertions;

/// <summary>
/// Tests the <see cref="UnknownPlatformException"/> class.
/// </summary>
public class UnknownPlatformExceptionTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WhenInvokedWithNoParam_CorrectlySetsMessage()
    {
        // Act
        var exception = new UnknownPlatformException();

        // Assert
        var expectedExceptionMessage = "The platform is unknown.";
        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void Ctor_WhenInvokedWithSingleMessageParam_CorrectlySetsMesage()
    {
        // Act
        var exceptionMessage = "test-message";
        var exception = new UnknownPlatformException(exceptionMessage);

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
        var deviceException = new UnknownPlatformException(exceptionMessage, innerException);

        // Assert
        deviceException.InnerException.Message.Should().Be(innerExceptionMessage);
        deviceException.Message.Should().Be(exceptionMessage);
    }
    #endregion
}