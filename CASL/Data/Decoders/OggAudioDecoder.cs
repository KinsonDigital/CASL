// <copyright file="OggAudioDecoder.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data.Decoders;

using System;
using System.Diagnostics.CodeAnalysis;
using NVorbis;
using OpenAL;

/// <summary>
/// Decodes ogg audio data from an ogg file.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Directly interacts with audio file.")]
internal sealed class OggAudioDecoder : IAudioFileDecoder<float>
{
    private readonly string filePath;
    private VorbisReader vorbisReader;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="OggAudioDecoder"/> class.
    /// </summary>
    /// <param name="filePath">The fully qualified path to the ogg audio file.</param>
    public OggAudioDecoder(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        this.filePath = filePath;
        this.vorbisReader = new VorbisReader(this.filePath);
    }

    /// <inheritdoc/>
    public int TotalChannels => string.IsNullOrEmpty(this.filePath) ? 0 : this.vorbisReader.Channels;

    /// <inheritdoc/>
    public ALFormat Format => TotalChannels == 1 ? ALFormat.MonoFloat32Ext : ALFormat.StereoFloat32Ext;

    /// <inheritdoc/>
    public int SampleRate
    {
        get
        {
            if (string.IsNullOrEmpty(this.filePath))
            {
                return 0;
            }

            return this.vorbisReader.SampleRate;
        }
    }

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
    public float TotalSeconds => (float)this.vorbisReader.TotalTime.TotalSeconds;

    /// <inheritdoc/>
    public void Flush()
    {
        this.vorbisReader.Dispose();
        this.vorbisReader = new VorbisReader(this.filePath);
    }

    /// <inheritdoc/>
    public int ReadUpTo(float[] buffer, uint upTo)
    {
        Flush();
        _ = this.vorbisReader.ReadSamples(new float[upTo].AsSpan());

        // Read the requested samples
        var samplesRead = this.vorbisReader.ReadSamples(buffer);

        return samplesRead;
    }

    /// <inheritdoc/>
    public int ReadSamples(float[] buffer, int offset, int count) => this.vorbisReader.ReadSamples(buffer, offset, count);

    /// <inheritdoc/>
    public int ReadSamples(float[] buffer) => this.vorbisReader.ReadSamples(buffer);

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
