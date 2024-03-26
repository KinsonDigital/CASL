// <copyright file="IAudioBuffer.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data;

using System;

/// <summary>
/// Represents an audio buffer that can be used to play audio data.
/// </summary>
internal interface IAudioBuffer : IDisposable
{
    /// <summary>
    /// Gets the total number of seconds of audio.
    /// </summary>
    float TotalSeconds { get; }

    /// <summary>
    /// Gets the current time position of the audio.
    /// </summary>
    SoundTime Position { get; }

    /// <summary>
    /// Returns a value indicating whether the audio is currently set to loop.
    /// </summary>
    bool IsLooping { get; }

    /// <summary>
    /// Initializes the buffer.
    /// </summary>
    /// <param name="filePath">The fully qualified file path to the audio file that contains the audio data.</param>
    /// <returns>The OpenAL source id.</returns>
    uint Init(string filePath);

    /// <summary>
    /// Uploads the audio data to the audio hardware.
    /// </summary>
    void Upload();

    /// <summary>
    /// Removes the buffer from the audio hardware.
    /// </summary>
    void RemoveBuffer();
}
