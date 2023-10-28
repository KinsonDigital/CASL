// <copyright file="LoadLibraryExceptionTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Exceptions;

using System;
using CASL.Exceptions;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests the <see cref="LoadLibraryException"/> class.
/// </summary>
public class LoadLibraryExceptionTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WhenInvokedWithNoParam_CorrectlySetsMessage()
    {
        // Act
        var exception = new LoadLibraryException();

        // Assert
        var expectedExceptionMessage = "There was an issue loading the library.";
        exception.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void Ctor_WhenInvokedWithSingleMessageParam_CorrectlySetsMessage()
    {
        // Arrange
        var expected = "test-message";

        // Act
        var exception = new LoadLibraryException(expected);

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
        var deviceException = new LoadLibraryException(expectedExceptionMessage, innerException);

        // Assert
        deviceException.InnerException.Message.Should().Be(expectedInnerExceptionMessage);
        deviceException.Message.Should().Be(expectedExceptionMessage);
    }
    #endregion
}
