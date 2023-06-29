// <copyright file="AudioDeviceManagerNotInitializedException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Devices.Exceptions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security;

/// <summary>
/// Occurs when the <see cref="AudioDeviceManager"/> has not been initialized.
/// </summary>
/// <remarks>This is done by invoking the <see cref="AudioDeviceManager.InitDevice(string?)"/> method.</remarks>
[Serializable]
public sealed class AudioDeviceManagerNotInitializedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDeviceManagerNotInitializedException"/> class.
    /// </summary>
    public AudioDeviceManagerNotInitializedException()
        : base("The audio device manager has not been initialized.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDeviceManagerNotInitializedException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public AudioDeviceManagerNotInitializedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDeviceManagerNotInitializedException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a
    ///     null reference if no inner exception is specified.
    /// </param>
    public AudioDeviceManagerNotInitializedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDeviceManagerNotInitializedException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate the data.</param>
    /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
    /// <exception cref="SecurityException">The caller does not have the required permissions.</exception>
    [ExcludeFromCodeCoverage(Justification = "No need to test empty private method.")]
    private AudioDeviceManagerNotInitializedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
