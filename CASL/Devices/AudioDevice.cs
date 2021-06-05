// <copyright file="AudioDevice.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Devices
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using CASL.Devices.Factories;

    /// <summary>
    /// Manages the audio devices in the system.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class AudioDevice
    {
        private static readonly IAudioDeviceManager AudioManager;

        /// <summary>
        /// Initializes static members of the <see cref="AudioDevice"/> class.
        /// </summary>
        static AudioDevice() => AudioManager = AudioDeviceManagerFactory.CreateDeviceManager();

        /// <summary>
        /// Gets the name of the current audio device in use.
        /// </summary>
        public static string DeviceInUse => AudioManager.DeviceInUse;

        /// <summary>
        /// Gets a list of all the audio devices in the system.
        /// </summary>
        public static string[] AudioDevices => AudioManager.DeviceNames;

        /// <summary>
        /// Changes the audio device for the sound to the given name.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        public static void SetAudioDevice(string name)
        {
            if (AudioManager.DeviceNames.Contains(name) is false)
            {
                throw new ArgumentException("The device name is invalid.", name);
            }

            AudioManager.ChangeDevice(name);
        }
    }
}
