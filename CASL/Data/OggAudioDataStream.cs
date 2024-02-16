// <copyright file="OggAudioDataStream.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data;

using System;
using System.IO;
using CASL.Exceptions;
using NVorbis;

/// <summary>
/// Streams ogg audio data from a ogg file.
/// </summary>
internal sealed class OggAudioDataStream : IAudioDataStream<float>
{
    private VorbisReader? vorbisReader;
    private string? filePath;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="OggAudioDataStream"/> class.
    /// </summary>
    /// <param name="filePath">The fully qualified path to the ogg audio file.</param>
    public OggAudioDataStream(string filePath) => this.filePath = filePath;

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    /// <remarks>
    ///     The <see cref="ReadSamples(float[], int, int)"/> method will
    ///     return 0 samples used if this is null or empty.
    /// </remarks>
    public string Filename
    {
        get => this.filePath ?? string.Empty;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new StringNullOrEmptyException();
            }

            if (!File.Exists(value))
            {
                throw new FileNotFoundException($"The file '{value}' was not found or does not exist", value);
            }

            var valueChanged = this.filePath != value;

            this.filePath = value;

            if (valueChanged)
            {
                Flush();
            }
        }
    }

    /// <inheritdoc/>
    public int Channels
    {
        get
        {
            if (string.IsNullOrEmpty(this.filePath))
            {
                return 0;
            }

            if (this.vorbisReader is null)
            {
                Flush();
            }

            return this.vorbisReader?.Channels ?? 0;
        }
    }

    /// <inheritdoc/>
    public AudioFormat Format
    {
        get
        {
            if (string.IsNullOrEmpty(this.filePath) || this.vorbisReader is null)
            {
                return default;
            }

            return Channels == 1 ? AudioFormat.MonoFloat32 : AudioFormat.StereoFloat32;
        }
    }

    /// <inheritdoc/>
    public int SampleRate
    {
        get
        {
            if (string.IsNullOrEmpty(this.filePath))
            {
                return 0;
            }

            if (this.vorbisReader is null)
            {
                Flush();
            }

            return this.vorbisReader?.SampleRate ?? 0;
        }
    }

    /// <inheritdoc/>
    public long TotalSamples => this.vorbisReader?.TotalSamples ?? 0;

    /// <inheritdoc/>
    public void Flush()
    {
        if (this.vorbisReader is not null)
        {
            this.vorbisReader.Dispose();
        }

        this.vorbisReader = new VorbisReader(this.filePath);
    }

    /// <inheritdoc/>
    public int ReadSamples(float[] buffer, int offset, int count)
    {
        if (string.IsNullOrEmpty(Filename))
        {
            throw new StringNullOrEmptyException();
        }

        if (this.vorbisReader is null)
        {
            Flush();
        }

        return this.vorbisReader?.ReadSamples(buffer, offset, count) ?? 0;
    }

    /// <inheritdoc/>
    public int ReadSamples(Span<float> buffer)
    {
        if (string.IsNullOrEmpty(Filename))
        {
            throw new StringNullOrEmptyException();
        }

        return this.vorbisReader?.ReadSamples(buffer) ?? 0;
    }

    /// <inheritdoc/>
    public int ReadSamples(float[] buffer) => ReadSamples(buffer.AsSpan());

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
            this.vorbisReader?.Dispose();
        }

        this.isDisposed = true;
    }
}
