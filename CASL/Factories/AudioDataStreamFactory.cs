// <copyright file="AudioDataStreamFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Factories;

using System;
using System.IO;
using System.IO.Abstractions;
using Data;

/// <inheritdoc/>
internal class AudioDataStreamFactory : IAudioDataStreamFactory
{
    private readonly IFile file;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDataStreamFactory"/> class.
    /// </summary>
    /// <param name="file">Performs operations with files.</param>
    public AudioDataStreamFactory(IFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        this.file = file;
    }

    /// <inheritdoc/>
    public IAudioDataStream<byte> CreateMp3AudioStream(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        if (!this.file.Exists(filePath))
        {
            throw new FileNotFoundException("The MP3 audio file does not exist.", filePath);
        }

        return new Mp3AudioDataStream(filePath);
    }

    /// <inheritdoc/>
    public IAudioDataStream<float> CreateOggAudioStream(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        if (!this.file.Exists(filePath))
        {
            throw new FileNotFoundException("The OGG audio file does not exist.", filePath);
        }

        return new OggAudioDataStream(filePath);
    }
}
