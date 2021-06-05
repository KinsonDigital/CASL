// <copyright file="InitializeDeviceException.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Devices.Exceptions
{
    using System;

    /// <summary>
    /// Occurs when there is an issue initializing an audio device.
    /// </summary>
    public class InitializeDeviceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeDeviceException"/> class.
        /// </summary>
        public InitializeDeviceException()
            : base("There was an issue initializing the audio device.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeDeviceException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public InitializeDeviceException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeDeviceException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InitializeDeviceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
