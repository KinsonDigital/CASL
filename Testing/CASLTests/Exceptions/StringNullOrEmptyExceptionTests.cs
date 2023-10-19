// <copyright file="StringNullOrEmptyExceptionTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Exceptions;

using System;
using CASL.Exceptions;
using Xunit;
using FluentAssertions;

/// <summary>
/// Tests the <see cref="StringNullOrEmptyException"/> class.
/// </summary>
public class StringNullOrEmptyExceptionTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WhenInvokedWithNoParam_CorrectlySetsMessage()
    {
        // Act
        var exception = new StringNullOrEmptyException();

        // Assert
        var expectedExceptionMessage = "The string must not be null or empty.";
        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void Ctor_WhenInvokedWithSingleMessageParam_CorrectlySetsMesage()
    {
        // Act
        var exceptionMessage = "test-message";
        var exception = new StringNullOrEmptyException(exceptionMessage);

        // Assert
        exception.Message.Should().Be(exceptionMessage);
    }

    [Fact]
    public void Ctor_WhenInvokedWithMessageAndInnerException_ThrowsException()
    {
        // Arrange
        var innerExceptionMessage = "inner-exception";
        var innerException = new Exception("inner-exception");

        // Act
        var exceptionMessage = "test-exception";
        var deviceException = new StringNullOrEmptyException("test-exception", innerException);

        // Assert
        deviceException.InnerException.Message.Should().Be(innerExceptionMessage);
        deviceException.Message.Should().Be(exceptionMessage);
    }
    #endregion
}