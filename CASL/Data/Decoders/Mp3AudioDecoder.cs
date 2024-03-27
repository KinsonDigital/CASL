// <copyright file="Mp3AudioDecoder.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data.Decoders;

using System;
using System.Diagnostics.CodeAnalysis;
using MP3Sharp;
using OpenAL;

/// <summary>
/// Decodes mp3 audio data from a mp3 file.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Directly interacts with audio file.")]
internal sealed class Mp3AudioDecoder : IAudioFileDecoder<byte>
{
    // NOTE: the Mp3Sharp decoder library only deals with 16bit mp3 files.  Which is 99% of what is used now days.
    private const float BytesPerSample = 4f;
    private readonly string filePath;
    private MP3Stream mp3Stream;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mp3AudioDecoder"/> class.
    /// </summary>
    /// <param name="filePath">The fully qualified path to the ogg audio file.</param>
    public Mp3AudioDecoder(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        this.filePath = filePath;
        this.mp3Stream = new MP3Stream(filePath);
        CalcSamplesAndTime();
    }

    /// <inheritdoc/>
    public int TotalChannels => this.mp3Stream.ChannelCount;

    /// <inheritdoc/>
    public ALFormat Format
    {
        get
        {
            if (string.IsNullOrEmpty(this.filePath))
            {
                return default;
            }

            return this.mp3Stream.Format == SoundFormat.Pcm16BitMono
                ? ALFormat.Mono16
                : ALFormat.Stereo16;
        }
    }

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
    public int ReadSamples(byte[] buffer) => this.mp3Stream.Read(buffer);

    /// <inheritdoc/>
    public int ReadSamples(byte[] buffer, int offset, int count) => this.mp3Stream.Read(buffer, offset, count);

    /// <inheritdoc/>
    public int ReadUpTo(byte[] buffer, uint upTo)
    {
        Flush();

        _ = this.mp3Stream.Read(new byte[upTo].AsSpan());

        // Read the requested samples
        return this.mp3Stream.Read(buffer);
    }

    /// <inheritdoc/>
    public void Flush()
    {
        // NOTE: The Flush() method does not seem to be internally implemented or working
        this.mp3Stream.Dispose();
        this.mp3Stream = new MP3Stream(this.filePath);
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
        var buffer = new byte[bufferSize].AsSpan();
        var totalBytesRead = 0L;
        var totalTimeSec = 0f;
        var totalSamplesRead = 0L;

        while (true)
        {
            var bytesRead = this.mp3Stream.Read(buffer);
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

        this.mp3Stream.Flush();
        this.mp3Stream.Dispose();
        this.mp3Stream = new MP3Stream(this.filePath);

        TotalBytes = totalBytesRead;
        TotalSamples = totalSamplesRead * this.mp3Stream.ChannelCount;
        TotalSeconds = totalTimeSec;
    }
}
