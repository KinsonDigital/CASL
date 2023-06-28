// <copyright file="InitializeDeviceException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Devices.Exceptions;

using System;
using System.Runtime.Serialization;
using System.Security;

/// <summary>
/// Occurs when there is an issue initializing an audio device.
/// </summary>
[Serializable]
public sealed class InitializeDeviceException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InitializeDeviceException"/> class.
    /// </summary>
    public InitializeDeviceException()
        : base("There was an issue initializing the audio device.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InitializeDeviceException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public InitializeDeviceException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InitializeDeviceException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public InitializeDeviceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InitializeDeviceException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate the data.</param>
    /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
    /// <exception cref="SecurityException">The caller does not have the required permissions.</exception>
    private InitializeDeviceException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
