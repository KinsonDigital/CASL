// <copyright file="IAudioBufferFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Factories;

using Data;

/// <summary>
/// Creates audio buffers for loading audio data for playback.
/// </summary>
internal interface IAudioBufferFactory
{
    /// <summary>
    /// Creates a full audio buffer for loading all of the audio file.
    /// </summary>
    /// <param name="filePath">The fully qualified path to the audio file.</param>
    /// <returns>The buffer.</returns>
    IAudioBuffer CreateFullBuffer(string filePath);

    /// <summary>
    /// Creates a stream audio buffer for streaming the audio data.
    /// </summary>
    /// <param name="filePath">The fully qualified path to the audio file.</param>
    /// <returns>The buffer.</returns>
    IAudioBuffer CreateStreamBuffer(string filePath);
}
