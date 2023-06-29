// <copyright file="StringNullOrEmptyException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Exceptions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security;

/// <summary>T
/// Occurs when a string is null or empty.
/// </summary>
[Serializable]
public sealed class StringNullOrEmptyException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StringNullOrEmptyException"/> class.
    /// </summary>
    public StringNullOrEmptyException()
        : base("The string must not be null or empty.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringNullOrEmptyException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public StringNullOrEmptyException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringNullOrEmptyException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a
    ///     null reference if no inner exception is specified.
    /// </param>
    public StringNullOrEmptyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringNullOrEmptyException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate the data.</param>
    /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
    /// <exception cref="SecurityException">The caller does not have the required permissions.</exception>
    [ExcludeFromCodeCoverage(Justification = "No need to test empty private method.")]
    private StringNullOrEmptyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
