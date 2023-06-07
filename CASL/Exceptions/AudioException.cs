// <copyright file="AudioException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Exceptions

using System;

    /// <summary>
    /// Occurs when an audio related exception has occured.
    /// </summary>
    public class AudioException : Exception
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
    }
