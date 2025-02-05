// <copyright file="Mp3AudioDecoder.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data.Decoders;

using System;
using MP3Sharp;
using OpenAL;
using Wrappers;

/// <summary>
/// Decodes mp3 audio data from a mp3 file.
/// </summary>
internal sealed class Mp3AudioDecoder : IAudioFileDecoder<byte>
{
    // NOTE: the Mp3Sharp decoder library only deals with 16bit mp3 files.  Which is 99% of what is used now days.
    private const float BytesPerSample = 4f;
    private readonly string filePath;
    private IMP3StreamWrapper mp3Stream;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mp3AudioDecoder"/> class.
    /// </summary>
    /// <param name="filePath">The fully qualified path to the ogg audio file.</param>
    /// <param name="mp3StreamWrapper">Wraps the original mp3 stream object.</param>
    public Mp3AudioDecoder(string filePath, IMP3StreamWrapper mp3StreamWrapper)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(mp3StreamWrapper);

        this.filePath = filePath;
        this.mp3Stream = mp3StreamWrapper;
        this.mp3Stream.Load(filePath);
        CalcSamplesAndTime();
    }

    /// <inheritdoc/>
    public int TotalChannels => this.mp3Stream.ChannelCount;

    /// <inheritdoc/>
    public ALFormat Format =>
        this.mp3Stream.Format == SoundFormat.Pcm16BitMono
            ? ALFormat.Mono16
            : ALFormat.Stereo16;

    /// <inheritdoc/>
    public int SampleRate => this.mp3Stream.Frequency;

    /// <inheritdoc/>
    public long TotalSamples { get; private set; }

    /// <inheritdoc/>
    public long TotalBytes { get; private set; }

    /// <inheritdoc/>
    public float TotalSeconds { get; private set; }

    /// <inheritdoc/>
    public long TotalSampleFrames => TotalSamples / TotalChannels;

    /// <inheritdoc/>
    public int ReadSamples(byte[] buffer) => this.mp3Stream.Read(buffer, 0, buffer.Length);

    /// <inheritdoc/>
    public int ReadSamples(byte[] buffer, int offset, int count)
    {
        var samplesRead = this.mp3Stream.Read(buffer, offset, count);

        if (samplesRead >= buffer.Length)
        {
            return samplesRead;
        }

        // Not enough data was read to completely fill the buffer
        // Set the rest of the buffer data to silence to prevent crunchy
        // sounds at the end of the audio.
        for (var i = samplesRead; i < buffer.Length; i++)
        {
            buffer[i] = 0;
        }

        return samplesRead;
    }

    /// <inheritdoc/>
    public int ReadUpTo(byte[] buffer, uint upTo)
    {
        Flush();
        _ = this.mp3Stream.Read(new byte[upTo], 0, (int)upTo);

        // Read the requested samples
        return this.mp3Stream.Read(buffer, 0, buffer.Length);
    }

    /// <inheritdoc/>
    public void Flush()
    {
        // NOTE: The Flush() method does not seem to be internally implemented or working
        this.mp3Stream.Flush();
        this.mp3Stream.Dispose();
        this.mp3Stream.Load(this.filePath);
    }

    /// <inheritdoc/>
    public void Dispose() => Dispose(disposing: true);

    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// <param name="disposing"><see langword="true"/> to dispose of managed resources.</param>
    private void Dispose(bool disposing)
    {
        if (this.isDisposed)
        {
            return;
        }

        if (disposing)
        {
            this.mp3Stream.Dispose();
        }

        this.isDisposed = true;
    }

    /// <summary>
    /// Calculates the total number of samples in the mp3 audio file.
    /// </summary>
    private void CalcSamplesAndTime()
    {
        var bufferSize = 4096 * this.mp3Stream.ChannelCount;
        var buffer = new byte[bufferSize];
        var totalBytesRead = 0L;
        var totalTimeSec = 0f;
        var totalSamplesRead = 0L;

        while (true)
        {
            var bytesRead = this.mp3Stream.Read(buffer, 0, buffer.Length);
            totalBytesRead += bytesRead;

            var samples = bytesRead / BytesPerSample;
            totalSamplesRead += (long)samples;

            var seconds = samples / this.mp3Stream.Frequency;
            totalTimeSec += seconds;

            if (bytesRead <= 0)
            {
                break;
            }
        }

        Flush();

        TotalBytes = totalBytesRead;
        TotalSamples = totalSamplesRead * this.mp3Stream.ChannelCount;
        TotalSeconds = totalTimeSec;
    }
}
