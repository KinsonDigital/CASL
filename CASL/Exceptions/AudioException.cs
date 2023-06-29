// <copyright file="AudioException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Exceptions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security;

/// <summary>
/// Occurs when an audio related exception has occured.
/// </summary>
[Serializable]
public sealed class AudioException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AudioException"/> class.
    /// </summary>
    public AudioException()
        : base("An audio exception has occurred.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public AudioException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a
    ///     null reference if no inner exception is specified.
    /// </param>
    public AudioException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate the data.</param>
    /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
    /// <exception cref="SecurityException">The caller does not have the required permissions.</exception>
    [ExcludeFromCodeCoverage(Justification = "No need to test empty private method.")]
    private AudioException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
