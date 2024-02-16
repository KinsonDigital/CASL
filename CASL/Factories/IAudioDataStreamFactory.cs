// <copyright file="IAudioDataStreamFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Factories;

using Data;

/// <summary>
/// Creates audio data streams.
/// </summary>
internal interface IAudioDataStreamFactory
{
    /// <summary>
    /// Creates an audio stream for an MP3 file.
    /// </summary>
    /// <param name="filePath">The fully qualified path to the MP3 file.</param>
    /// <returns>The audio stream.</returns>
    IAudioDataStream<byte> CreateMp3AudioStream(string filePath);

    /// <summary>
    /// Creates an audio stream for an OGG file.
    /// </summary>
    /// <param name="filePath">The fully qualified path to the OGG file.</param>
    /// <returns>The audio stream.</returns>
    IAudioDataStream<float> CreateOggAudioStream(string filePath);
}
