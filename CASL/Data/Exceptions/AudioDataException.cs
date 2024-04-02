// <copyright file="AudioDataException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data.Exceptions;

using System;

/// <summary>
/// Occurs when an OpenAL audio sources does not exist.
/// </summary>
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
}
