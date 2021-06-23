// <copyright file="LoadLibraryException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Exceptions
{
    using System;

    /// <summary>
    /// Occurs when an issue has been encountered loading a library.
    /// </summary>
    public class LoadLibraryException : Exception
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
    }
}
