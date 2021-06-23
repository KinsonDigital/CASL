// <copyright file="UnknownPlatformException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Exceptions
{
    using System;

    /// <summary>
    /// Occurs when an unknown platform has been encountered.
    /// </summary>
    public class UnknownPlatformException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownPlatformException"/> class.
        /// </summary>
        public UnknownPlatformException()
            : base("The platform is unknown.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownPlatformException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public UnknownPlatformException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownPlatformException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception, or a
        ///     null reference if no inner exception is specified.
        /// </param>
        public UnknownPlatformException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
