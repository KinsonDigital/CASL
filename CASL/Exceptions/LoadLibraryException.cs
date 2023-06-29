// <copyright file="LoadLibraryException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Exceptions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security;

/// <summary>
/// Occurs when an issue has been encountered loading a library.
/// </summary>
[Serializable]
public sealed class LoadLibraryException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoadLibraryException"/> class.
    /// </summary>
    public LoadLibraryException()
        : base("There was an issue loading the library.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadLibraryException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LoadLibraryException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadLibraryException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a
    ///     null reference if no inner exception is specified.
    /// </param>
    public LoadLibraryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadLibraryException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate the data.</param>
    /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
    /// <exception cref="SecurityException">The caller does not have the required permissions.</exception>
    [ExcludeFromCodeCoverage(Justification = "No need to test empty private method.")]
    private LoadLibraryException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
