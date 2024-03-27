// <copyright file="AudioDataExceptionTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Data.Exceptions;

using System;
using CASL.Data.Exceptions;
using Xunit;

/// <summary>
/// Tests the <see cref="AudioDataException"/> class.
/// </summary>
public class AudioDataExceptionTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WhenInvokedWithNoParam_CorrectlySetsMessage()
    {
        // Act
        var exception = new AudioDataException();

        // Assert
        Assert.Equal("There was an issue with the processing the audio data.", exception.Message);
    }

    [Fact]
    public void Ctor_WhenInvokedWithSingleMessageParam_CorrectlySetsMessage()
    {
        // Act
        var exception = new AudioDataException("test-message");

        // Assert
        Assert.Equal("test-message", exception.Message);
    }

    [Fact]
    public void Ctor_WhenInvokedWithMessageAndInnerException_ThrowsException()
    {
        // Arrange
        var innerException = new Exception("inner-exception");

        // Act
        var deviceException = new AudioDataException("test-exception", innerException);

        // Assert
        Assert.Equal("inner-exception", deviceException.InnerException.Message);
        Assert.Equal("test-exception", deviceException.Message);
    }
    #endregion
}
