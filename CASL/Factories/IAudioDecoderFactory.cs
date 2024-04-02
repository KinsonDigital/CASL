// <copyright file="IAudioDecoderFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Factories;

using Data.Decoders;

/// <summary>
/// Creates audio data decoders.
/// </summary>
internal interface IAudioDecoderFactory
{
    /// <summary>
    /// Creates an audio decoder for an MP3 file.
    /// </summary>
    /// <param name="filePath">The fully qualified path to the MP3 file.</param>
    /// <returns>The audio decoder.</returns>
    IAudioFileDecoder<byte> CreateMp3AudioDecoder(string filePath);

    /// <summary>
    /// Creates an audio decoder for an OGG file.
    /// </summary>
    /// <param name="filePath">The fully qualified path to the OGG file.</param>
    /// <returns>The audio decoder.</returns>
    IAudioFileDecoder<float> CreateOggAudioDecoder(string filePath);
}
