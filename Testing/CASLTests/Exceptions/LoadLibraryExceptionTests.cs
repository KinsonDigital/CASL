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
    public void Ctor_WhenInvokedWithSingleMessageParam_CorrectlySetsMesage()
    {
        // Act
        var exceptionMessage = "test-message";
        var exception = new LoadLibraryException(exceptionMessage);

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
        var deviceException = new LoadLibraryException(exceptionMessage, innerException);

        // Assert
        deviceException.InnerException.Message.Should().Be(innerExceptionMessage);
        deviceException.Message.Should().Be(exceptionMessage);
    }
    #endregion
}
