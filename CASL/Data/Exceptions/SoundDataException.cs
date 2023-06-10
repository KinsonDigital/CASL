// <copyright file="SoundDataException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data.Exceptions;

using System;

/// <summary>
/// Occurs when an OpenAL sound sources does not exist.
/// </summary>
internal class SoundDataException : Exception
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
}
