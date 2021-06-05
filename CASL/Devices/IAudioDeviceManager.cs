// <copyright file="IAudioDeviceManager.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Devices
{
    using System;

    /// <summary>
    /// Manages audio devices on the system using OpenAL.
    /// </summary>
    internal interface IAudioDeviceManager : IDisposable
    {
        /// <summary>
        /// Occurs right before the audio device changes.
        /// </summary>
        event EventHandler<EventArgs>? DeviceChanging;

        /// <summary>
        /// Occurs when the audio device has changed.
        /// </summary>
        event EventHandler<EventArgs>? DeviceChanged;

        /// <summary>
        /// Gets a value indicating whether the audio device has been initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the list of audio devices in the system.
        /// </summary>
        /// <exception cref="AudioDeviceManagerNotInitializedException">
        ///     Occurs if this method is executed without initializing the <see cref="IAudioDeviceManager."/>.
        ///     This can be done by invoking the <see cref="InitDevice(string?)"/>.
        /// </exception>
        string[] DeviceNames { get; }

        /// <summary>
        /// Gets the name of the current audio device that is use.
        /// </summary>
        string DeviceInUse { get; }

        /// <summary>
        /// Initializes an audio device that matches the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the device to initialize for use.</param>
        /// <remarks>The value of null will initialize the current default device.</remarks>
        void InitDevice(string? name = null);

        /// <summary>
        /// Initializes a sound and returns a relative sound source and buffer id.
        /// </summary>
        /// <returns>
        ///     srcId: The OpenAL source id.
        ///     bufferId: The OpenAL id of the sound buffer.
        /// </returns>
        /// <exception cref="AudioDeviceManagerNotInitializedException">
        ///     Occurs if this method is executed without initializing the <see cref="IAudioDeviceManager."/>.
        ///     This can be done by invoking the <see cref="InitDevice(string?)"/>.
        /// </exception>
        (uint srcId, uint bufferId) InitSound();

        /// <summary>
        /// Changes the audio device that matches the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the audio device to change to.</param>
        /// <exception cref="AudioDeviceManagerNotInitializedException">
        ///     Occurs if this method is executed without initializing the <see cref="IAudioDeviceManager."/>.
        ///     This can be done by invoking the <see cref="InitDevice(string?)"/>.
        /// </exception>
        /// <exception cref="AudioDeviceDoesNotExistException">
        ///     Occurs if attempting to change to a device that does not exist on the system.
        /// </exception>
        void ChangeDevice(string name);

        /// <summary>
        /// Updates the sound source using the given <paramref name="soundSrc"/>.
        /// </summary>
        /// <param name="soundSrc">The OpenAL source id.</param>
        /// <remarks>
        ///     The sound source is found using the given params <see cref="SoundSource.SourceId"/> value.
        /// </remarks>
        /// <exception cref="AudioDeviceManagerNotInitializedException">
        ///     Occurs if this method is executed without initializing the <see cref="IAudioDeviceManager."/>.
        ///     This can be done by invoking the <see cref="InitDevice(string?)"/>.
        /// </exception>
        /// <exception cref="SoundSourceDoesNotExistException">
        ///     Occurs if the <see cref="SoundSource.SourceId"/> of the given param <paramref name="soundSrc"/> does not exist.
        /// </exception>
        void UpdateSoundSource(SoundSource soundSrc);

        /// <summary>
        /// Removes a sound source with the given source ID.
        /// </summary>
        /// <param name="sourceId">The ID of the source to delete.</param>
        void RemoveSoundSource(uint sourceId);
    }
}
