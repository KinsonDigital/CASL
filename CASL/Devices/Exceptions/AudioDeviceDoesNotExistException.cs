﻿// <copyright file="AudioDeviceDoesNotExistException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Devices.Exceptions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security;

/// <summary>
/// Occurs when an audio device does not exist.
/// </summary>
[Serializable]
public sealed class AudioDeviceDoesNotExistException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDeviceDoesNotExistException"/> class.
    /// </summary>
    public AudioDeviceDoesNotExistException()
        : base("The audio device does not exist.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDeviceDoesNotExistException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public AudioDeviceDoesNotExistException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDeviceDoesNotExistException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="deviceName">The name of the device.</param>
    public AudioDeviceDoesNotExistException(string message, string deviceName)
        : base($"Device Name: {deviceName}\n{message}")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDeviceDoesNotExistException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a
    ///     null reference if no inner exception is specified.
    /// </param>
    public AudioDeviceDoesNotExistException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDeviceDoesNotExistException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate the data.</param>
    /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
    /// <exception cref="SecurityException">The caller does not have the required permissions.</exception>
    [ExcludeFromCodeCoverage(Justification = "No need to test empty private method.")]
    private AudioDeviceDoesNotExistException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
