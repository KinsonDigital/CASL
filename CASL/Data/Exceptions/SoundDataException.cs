// <copyright file="SoundDataException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data.Exceptions;

using System;
using System.Runtime.Serialization;
using System.Security;

/// <summary>
/// Occurs when an OpenAL sound sources does not exist.
/// </summary>
[Serializable]
public sealed class SoundDataException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SoundDataException"/> class.
    /// </summary>
    public SoundDataException()
        : base("There was an issue with the processing the sound data.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundDataException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public SoundDataException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundDataException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a
    ///     null reference if no inner exception is specified.
    /// </param>
    public SoundDataException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundDataException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate the data.</param>
    /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
    /// <exception cref="SecurityException">The caller does not have the required permissions.</exception>
    private SoundDataException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
