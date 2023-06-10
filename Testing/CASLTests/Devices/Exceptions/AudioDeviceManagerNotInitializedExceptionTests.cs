﻿// <copyright file="AudioDeviceManagerNotInitializedExceptionTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Audio.Exceptions;

using CASL.Devices.Exceptions;
using System;
using Xunit;

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
        Assert.Equal("The audio device manager has not been initialized.", exception.Message);
    }

    [Fact]
    public void Ctor_WhenInvokedWithSingleMessageParam_CorrectlySetsMesage()
    {
        // Act
        var exception = new AudioDeviceManagerNotInitializedException("test-message");

        // Assert
        Assert.Equal("test-message", exception.Message);
    }

    [Fact]
    public void Ctor_WhenInvokedWithMessageAndInnerException_ThrowsException()
    {
        // Arrange
        var innerException = new Exception("inner-exception");

        // Act
        var deviceException = new AudioDeviceManagerNotInitializedException("test-exception", innerException);

        // Assert
        Assert.Equal("inner-exception", deviceException.InnerException.Message);
        Assert.Equal("test-exception", deviceException.Message);
    }
    #endregion
}
