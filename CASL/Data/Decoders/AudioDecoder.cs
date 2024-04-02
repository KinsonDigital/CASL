// <copyright file="AudioDecoder.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data.Decoders;

using System;
using System.ComponentModel;
using System.IO.Abstractions;
using CASL.Exceptions;
using Factories;
using OpenAL;

/// <inheritdoc/>
internal sealed class AudioDecoder : IAudioDecoder
{
    private const int ChunkSize = 4096;
    private readonly IAudioFileDecoder<float>? oggDataDecoder;
    private readonly IAudioFileDecoder<byte>? mp3DataDecoder;
    private readonly AudioFormatType audioFormatType;
    private readonly int bufferSize;
    private byte[]? mp3Buffer;
    private float[]? oggBuffer;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDecoder"/> class.
    /// </summary>
    /// <param name="filePath">The fully qualified path to the audio file.</param>
    /// <param name="dataDecoderFactory">Creates audio data decoders.</param>
    /// <param name="path">Manages file paths.</param>
    /// <param name="file">Performs operations with files.</param>
    /// <exception cref="ArgumentException">Thrown if the given <paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the following parameters are null.
    ///     <list type="bullet">
    ///         <item><paramref name="dataDecoderFactory"/></item>
    ///         <item><paramref name="path"/></item>
    ///         <item><paramref name="file"/></item>
    ///     </list>
    /// </exception>
    /// <exception cref="InvalidEnumArgumentException">Thrown if the given file path is not an MP3 of OGG file.</exception>
    public AudioDecoder(
        string filePath,
        IAudioDecoderFactory dataDecoderFactory,
        IPath path,
        IFile file)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(dataDecoderFactory);
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(file);

        var extension = path.GetExtension(filePath).ToLower();

        switch (extension)
        {
            case ".mp3":
                this.mp3DataDecoder = dataDecoderFactory.CreateMp3AudioDecoder(filePath);
                this.audioFormatType = AudioFormatType.Mp3;
                break;
            case ".ogg":
                this.oggDataDecoder = dataDecoderFactory.CreateOggAudioDecoder(filePath);
                this.audioFormatType = AudioFormatType.Ogg;
                break;
            default:
                var exMsg = $"The file extension '{extension}' is not supported.";
                exMsg += " Supported extensions are '.ogg' and '.mp3'.";
                throw new AudioException(exMsg);
        }

#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
        TotalSeconds = this.audioFormatType switch
        {
            AudioFormatType.Mp3 => this.mp3DataDecoder!.TotalSeconds,
            AudioFormatType.Ogg => this.oggDataDecoder!.TotalSeconds,
        };

        TotalChannels = this.audioFormatType switch
        {
            AudioFormatType.Mp3 => this.mp3DataDecoder!.TotalChannels,
            AudioFormatType.Ogg => this.oggDataDecoder!.TotalChannels,
        };

        TotalSamples = this.audioFormatType switch
        {
            AudioFormatType.Mp3 => this.mp3DataDecoder!.TotalSamples,
            AudioFormatType.Ogg => this.oggDataDecoder!.TotalSamples,
        };

        Format = this.audioFormatType switch
        {
            AudioFormatType.Mp3 => this.mp3DataDecoder!.Format,
            AudioFormatType.Ogg => this.oggDataDecoder!.Format,
        };

        SampleRate = this.audioFormatType switch
        {
            AudioFormatType.Mp3 => this.mp3DataDecoder!.SampleRate,
            AudioFormatType.Ogg => this.oggDataDecoder!.SampleRate,
        };

        TotalSampleFrames = this.audioFormatType switch
        {
            AudioFormatType.Mp3 => this.mp3DataDecoder!.TotalSampleFrames,
            AudioFormatType.Ogg => this.oggDataDecoder!.TotalSampleFrames,
        };
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

        this.bufferSize = ChunkSize * (TotalChannels <= 0 ? 1 : TotalChannels);

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (this.audioFormatType)
        {
            case AudioFormatType.Mp3:
                this.mp3Buffer = new byte[this.bufferSize];
                break;
            case AudioFormatType.Ogg:
                this.oggBuffer = new float[this.bufferSize];
                break;
        }
    }

    /// <inheritdoc/>
    public float TotalSeconds { get; }

    /// <inheritdoc/>
    public long TotalSampleFrames { get; }

    /// <inheritdoc/>
    public ALFormat Format { get; }

    /// <inheritdoc/>
    public int TotalChannels { get; }

    /// <inheritdoc/>
    public int SampleRate { get; }

    /// <inheritdoc/>
    public long TotalSamples { get; }

    /// <inheritdoc/>
    public int ReadUpTo(uint upTo)
    {
        switch (this.audioFormatType)
        {
            case AudioFormatType.Mp3:
                ArgumentNullException.ThrowIfNull(this.mp3Buffer);

                return this.mp3DataDecoder?.ReadUpTo(this.mp3Buffer, upTo) ?? 0;
            case AudioFormatType.Ogg:
                ArgumentNullException.ThrowIfNull(this.oggBuffer);

                return this.oggDataDecoder?.ReadUpTo(this.oggBuffer, upTo) ?? 0;
            default:
                throw new InvalidEnumArgumentException(
                    $"this.{nameof(this.audioFormatType)}",
                    (int)this.audioFormatType,
                    typeof(AudioFormatType));
        }
    }

    /// <inheritdoc/>
    public int ReadSamples()
    {
        switch (this.audioFormatType)
        {
            case AudioFormatType.Mp3:
                ArgumentNullException.ThrowIfNull(this.mp3Buffer);

                return this.mp3DataDecoder?.ReadSamples(this.mp3Buffer, 0, this.bufferSize) ?? 0;
            case AudioFormatType.Ogg:
                ArgumentNullException.ThrowIfNull(this.oggBuffer);

                return this.oggDataDecoder?.ReadSamples(this.oggBuffer, 0, this.bufferSize) ?? 0;
            default:
                throw new InvalidEnumArgumentException(
                    $"this.{nameof(this.audioFormatType)}",
                    (int)this.audioFormatType,
                    typeof(AudioFormatType));
        }
    }

    /// <inheritdoc/>
    public void ReadAllSamples()
    {
        switch (this.audioFormatType)
        {
            case AudioFormatType.Mp3:
                ArgumentNullException.ThrowIfNull(this.mp3DataDecoder);

                this.mp3DataDecoder.Flush();

                this.mp3Buffer = new byte[this.mp3DataDecoder.TotalBytes];
                this.mp3DataDecoder.ReadSamples(this.mp3Buffer);

                break;
            case AudioFormatType.Ogg:
                ArgumentNullException.ThrowIfNull(this.oggDataDecoder);

                this.oggDataDecoder.Flush();

                this.oggBuffer = new float[this.oggDataDecoder.TotalSamples];
                this.oggDataDecoder.ReadSamples(this.oggBuffer);

                break;
            default:
                throw new InvalidEnumArgumentException(
                    $"this.{nameof(this.audioFormatType)}",
                    (int)this.audioFormatType,
                    typeof(AudioFormatType));
        }
    }

    /// <inheritdoc/>
    public T[] GetSampleData<T>() =>
        this.audioFormatType switch
        {
            AudioFormatType.Mp3 => this.mp3Buffer as T[] ?? Array.Empty<T>(),
            AudioFormatType.Ogg => this.oggBuffer as T[] ?? Array.Empty<T>(),
            _ => throw new InvalidEnumArgumentException(
                $"this.{nameof(this.audioFormatType)}",
                (int)this.audioFormatType,
                typeof(AudioFormatType)),
        };

    /// <inheritdoc/>
    public void Flush()
    {
        switch (this.audioFormatType)
        {
            case AudioFormatType.Mp3:
                this.mp3DataDecoder?.Flush();
                break;
            case AudioFormatType.Ogg:
                this.oggDataDecoder?.Flush();
                break;
            default:
                throw new InvalidEnumArgumentException(
                    $"this.{nameof(this.audioFormatType)}",
                    (int)this.audioFormatType,
                    typeof(AudioFormatType));
        }
    }

    /// <inheritdoc/>
    public void Dispose() => Dispose(true);

    /// <summary>
    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// </summary>
    /// <param name="disposing">True to dispose of managed resources.</param>
    private void Dispose(bool disposing)
    {
        if (this.isDisposed)
        {
            return;
        }

        if (disposing)
        {
            this.oggDataDecoder?.Dispose();
            this.mp3DataDecoder?.Dispose();
        }

        this.isDisposed = true;
    }
}
