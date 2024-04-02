// <copyright file="IAudioDeviceManager.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Devices;

using System;
using System.Collections.Immutable;

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
    /// Changes the audio device that matches the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the audio device to change to.</param>
    void ChangeDevice(string name);
}
