// <copyright file="AudioDataException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data.Exceptions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security;

/// <summary>
/// Occurs when an OpenAL audio sources does not exist.
/// </summary>
[Serializable]
public sealed class AudioDataException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDataException"/> class.
    /// </summary>
    public AudioDataException()
        : base("There was an issue with the processing the audio data.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDataException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public AudioDataException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDataException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a
    ///     null reference if no inner exception is specified.
    /// </param>
    public AudioDataException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDataException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate the data.</param>
    /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
    /// <exception cref="SecurityException">The caller does not have the required permissions.</exception>
    [ExcludeFromCodeCoverage(Justification = "No need to test empty private method.")]
    private AudioDataException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
