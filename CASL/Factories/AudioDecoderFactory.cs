// <copyright file="AudioDecoderFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Factories;

using System.Diagnostics.CodeAnalysis;
using Data.Decoders;

/// <inheritdoc/>
[ExcludeFromCodeCoverage(Justification = "No logic to test.")]
internal sealed class AudioDecoderFactory : IAudioDecoderFactory
{
    /// <inheritdoc/>
    public IAudioFileDecoder<byte> CreateMp3AudioDecoder(string filePath) => new Mp3AudioDecoder(filePath);

    /// <inheritdoc/>
    public IAudioFileDecoder<float> CreateOggAudioDecoder(string filePath) => new OggAudioDecoder(filePath);
}
