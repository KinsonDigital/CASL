// <copyright file="Mp3AudioDataStream.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data;

using System;
using System.IO;
using CASL.Exceptions;
using MP3Sharp;

/// <summary>
/// Streams mp3 audio data from a mp3 file.
/// </summary>
internal sealed class Mp3AudioDataStream : IAudioDataStream<byte>
{
    // NOTE: the Mp3Sharp decoder library only deals with 16bit mp3 files.  Which is 99% of what is used now days anyways
    private MP3Stream? mp3Reader;
    private string? filePath;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mp3AudioDataStream"/> class.
    /// </summary>
    /// <param name="filePath">The fully qualified path to the ogg audio file.</param>
    public Mp3AudioDataStream(string filePath) => this.filePath = filePath;

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    /// <remarks>
    ///     The <see cref="ReadSamples(byte[], int, int)"/> method will
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
                throw new FileNotFoundException($"The file '{value}' was not found or does not exist.");
            }

            if (this.mp3Reader is null)
            {
                this.mp3Reader = new MP3Stream(value);
            }
            else
            {
                if (value != this.filePath)
                {
                    this.mp3Reader.Dispose();
                    this.mp3Reader = new MP3Stream(value);
                }
            }

            this.filePath = value;
        }
    }

    /// <inheritdoc/>
    public int Channels => string.IsNullOrEmpty(this.filePath) ? 0 : this.mp3Reader?.ChannelCount ?? 0;

    /// <inheritdoc/>
    public AudioFormat Format
    {
        get
        {
            if (string.IsNullOrEmpty(this.filePath) || this.mp3Reader is null)
            {
                return default;
            }

            return this.mp3Reader.Format == SoundFormat.Pcm16BitMono
                ? AudioFormat.Mono16
                : AudioFormat.Stereo16;
        }
    }

    /// <inheritdoc/>
    public int SampleRate => string.IsNullOrEmpty(this.filePath) ? 0 : this.mp3Reader?.Frequency ?? 0;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Thrown because their is no way to get this value from the <see cref="MP3Stream"/>.
    /// </exception>
    public long TotalSamples =>
        throw new NotImplementedException("No way to get the total samples from an mp3 file.  This is because mp3 files are compressed.");

    /// <inheritdoc/>
    public int ReadSamples(byte[] buffer, int offset, int count)
    {
        if (string.IsNullOrEmpty(Filename))
        {
            throw new StringNullOrEmptyException();
        }

        return this.mp3Reader?.Read(buffer, offset, count) ?? 0;
    }

    /// <inheritdoc/>
    public int ReadSamples(byte[] buffer) => ReadSamples(buffer.AsSpan());

    /// <inheritdoc/>
    public int ReadSamples(Span<byte> buffer)
    {
        if (string.IsNullOrEmpty(Filename))
        {
            throw new StringNullOrEmptyException();
        }

        return this.mp3Reader?.Read(buffer) ?? 0;
    }

    /// <inheritdoc/>
    public void Flush()
    {
        if (this.mp3Reader is not null)
        {
            this.mp3Reader.Dispose();
        }

        this.mp3Reader = new MP3Stream(this.filePath);
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
            this.mp3Reader?.Dispose();
        }

        this.isDisposed = true;
    }
}
