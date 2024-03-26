// <copyright file="IAudioDeviceManager.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Devices;

using System;
using System.Collections.Immutable;
using Exceptions;

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
    /// Gets the name of the current audio device that is use.
    /// </summary>
    string DeviceInUse { get; }

    /// <summary>
    /// Gets the list of audio devices in the system.
    /// </summary>
    /// <returns>The list of devices.</returns>
    ImmutableArray<string> GetDeviceNames();

    /// <summary>
    /// Initializes an audio device that matches the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the device to initialize for use.</param>
    /// <remarks>The value of null will initialize the current default device.</remarks>
    void InitDevice(string? name = null);

    /// <summary>
    /// Changes the audio device that matches the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the audio device to change to.</param>
    /// <exception cref="AudioDeviceManagerNotInitializedException">
    ///     Occurs if this method is executed without initializing the <see cref="InitDevice"/>() method.
    ///     This can be done by invoking the <see cref="InitDevice(string?)"/>.
    /// </exception>
    /// <exception cref="AudioDeviceDoesNotExistException">
    ///     Occurs if attempting to change to a device that does not exist on the system.
    /// </exception>
    void ChangeDevice(string name);
}
