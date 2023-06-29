// <copyright file="MP3SoundDecoder.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

/// <summary>
/// Decodes MP3 audio data files.
/// </summary>
internal sealed class MP3SoundDecoder : ISoundDecoder<byte>
{
    private readonly IAudioDataStream<byte> audioDataStream;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MP3SoundDecoder"/> class.
    /// </summary>
    /// <param name="dataStream">Streams the audio data from the file as bytes.</param>
    public MP3SoundDecoder(IAudioDataStream<byte> dataStream) => this.audioDataStream = dataStream;

    /// <summary>
    /// Loads mp3 audio data from an mp3 file using the given <paramref name="fileName"/>.
    /// </summary>
    /// <param name="fileName">The file name/path to the mp3 file.</param>
    /// <returns>The sound and related audio data.</returns>
    public SoundData<byte> LoadData(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentException("The param must not be null or empty.", nameof(fileName));
        }

        if (Path.GetExtension(fileName) != ".mp3")
        {
            throw new ArgumentException("The file name must have an mp3 file extension.", nameof(fileName));
        }

        SoundData<byte> result = default;

        this.audioDataStream.Filename = fileName;

        result.SampleRate = this.audioDataStream.SampleRate;
        result.Channels = this.audioDataStream.Channels;

        var dataResult = new List<byte>();

        const byte bitsPerSample = 16;
        const byte bytesPerSample = bitsPerSample / 8;

        var buffer = new byte[this.audioDataStream.Channels * this.audioDataStream.SampleRate * bytesPerSample];

        while (this.audioDataStream.ReadSamples(buffer, 0, buffer.Length) > 0)
        {
            dataResult.AddRange(buffer);
        }

        result.Format = this.audioDataStream.Format;
        result.BufferData = new ReadOnlyCollection<byte>(dataResult);

        return result;
    }

    /// <inheritdoc/>
    public void Dispose() => Dispose(disposing: true);

    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// <param name="disposing"><see langword="true"/> if the managed resources should be disposed.</param>
    private void Dispose(bool disposing)
    {
        if (this.isDisposed)
        {
            return;
        }

        if (disposing)
        {
            this.audioDataStream.Dispose();
        }

        this.isDisposed = true;
    }
}
