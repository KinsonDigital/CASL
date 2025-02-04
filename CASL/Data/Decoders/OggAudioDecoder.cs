// <copyright file="OggAudioDecoder.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data.Decoders;

using System;
using OpenAL;
using Wrappers;

/// <summary>
/// Decodes ogg audio data from an ogg file.
/// </summary>
internal sealed class OggAudioDecoder : IAudioFileDecoder<float>
{
    private readonly string filePath;
    private readonly IVorbisReaderWrapper vorbisReader;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="OggAudioDecoder"/> class.
    /// </summary>
    /// <param name="filePath">The fully qualified path to the ogg audio file.</param>
    /// <param name="vorbisReaderWrapper">Wraps original vorbis reader object.</param>
    public OggAudioDecoder(string filePath, IVorbisReaderWrapper vorbisReaderWrapper)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(vorbisReaderWrapper);

        this.filePath = filePath;
        this.vorbisReader = vorbisReaderWrapper;
        this.vorbisReader.Load(filePath);
    }

    /// <inheritdoc/>
    public int TotalChannels => string.IsNullOrEmpty(this.filePath) ? 0 : this.vorbisReader.Channels;

    /// <inheritdoc/>
    public ALFormat Format => TotalChannels == 1 ? ALFormat.MonoFloat32Ext : ALFormat.StereoFloat32Ext;

    /// <inheritdoc/>
    public int SampleRate => this.vorbisReader.SampleRate;

    /// <inheritdoc/>
    public long TotalSamples
    {
        get
        {
            // NOTE: The TotalSamples property does not represent the total number of samples
            // for both channels when the audio is stereo.  Only when it is mono.
            var totalSamples = this.vorbisReader.TotalSamples;
            var totalChannels = this.vorbisReader.Channels;

            return totalSamples * totalChannels;
        }
    }

    /// <inheritdoc/>
    public long TotalSampleFrames => TotalSamples / TotalChannels;

    /// <inheritdoc/>
    public long TotalBytes => TotalSamples * sizeof(float);

    /// <inheritdoc/>
    public float TotalSeconds => this.vorbisReader.TotalSeconds;

    /// <inheritdoc/>
    public void Flush()
    {
        this.vorbisReader.Dispose();
        this.vorbisReader.Load(this.filePath);
    }

    /// <inheritdoc/>
    public int ReadUpTo(float[] buffer, uint upTo)
    {
        Flush();

        _ = this.vorbisReader.ReadSamples(new float[upTo], 0, (int)upTo);

        // Read the requested samples
        var samplesRead = this.vorbisReader.ReadSamples(buffer, 0, buffer.Length);

        return samplesRead;
    }

    /// <inheritdoc/>
    public int ReadSamples(float[] buffer, int offset, int count)
    {
        var samplesRead = this.vorbisReader.ReadSamples(buffer, offset, count);

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
    public int ReadSamples(float[] buffer) => this.vorbisReader.ReadSamples(buffer, 0, buffer.Length);

    /// <inheritdoc/>
    public void Dispose() => Dispose(true);

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
            this.vorbisReader.Dispose();
        }

        this.isDisposed = true;
    }
}
