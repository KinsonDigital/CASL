// <copyright file="Sound.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using CASL.Data;
using CASL.Data.Exceptions;
using CASL.Devices;
using CASL.Devices.Factories;
using CASL.Exceptions;
using CASL.OpenAL;

/// <summary>
/// A single sound that can be played, paused etc.
/// </summary>
public sealed class Sound : ISound
{
    private const char CrossPlatDirSeparatorChar = '/';
    private const string IsDisposedExceptionMessage = "The sound is disposed.  You must create another sound instance.";
    private readonly IAudioDeviceManager audioManager;
    private readonly ISoundDecoder<float> oggDecoder;
    private readonly ISoundDecoder<byte> mp3Decoder;
    private readonly IOpenALInvoker alInvoker;
    private readonly IPath path;
    private uint srcId;
    private uint bufferId;
    private bool ignoreOpenALCalls;
    private bool isDisposed;
    private float totalSeconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="Sound"/> class.
    /// </summary>
    /// <param name="filePath">The path to the sound file..</param>
    [ExcludeFromCodeCoverage]
    public Sound(string filePath)
    {
        FilePath = filePath.ToCrossPlatPath().TrimAllFromEnd(CrossPlatDirSeparatorChar);

        this.alInvoker = IoC.Container.GetInstance<IOpenALInvoker>();
        this.alInvoker.ErrorCallback += ErrorCallback;

        this.oggDecoder = IoC.Container.GetInstance<ISoundDecoder<float>>();

        this.mp3Decoder = IoC.Container.GetInstance<ISoundDecoder<byte>>();
        this.audioManager = AudioDeviceManagerFactory.CreateDeviceManager();
        this.audioManager.DeviceChanging += AudioManager_DeviceChanging;
        this.audioManager.DeviceChanged += AudioManager_DeviceChanged;
        this.path = IoC.Container.GetInstance<IPath>();

        Init();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sound"/> class.
    /// </summary>
    /// <param name="filePath">The path to the sound file.</param>
    /// <param name="alInvoker">Provides access to OpenAL.</param>
    /// <param name="audioManager">Manages audio related operations.</param>
    /// <param name="oggDecoder">Decodes OGG audio files.</param>
    /// <param name="mp3Decoder">Decodes MP3 audio files.</param>
    /// <param name="path">Manages paths.</param>
    internal Sound(
        string filePath,
        IOpenALInvoker alInvoker,
        IAudioDeviceManager audioManager,
        ISoundDecoder<float> oggDecoder,
        ISoundDecoder<byte> mp3Decoder,
        IPath path)
    {
        FilePath = filePath.ToCrossPlatPath().TrimAllFromEnd(CrossPlatDirSeparatorChar);

        this.alInvoker = alInvoker;
        this.alInvoker.ErrorCallback += ErrorCallback;

        this.oggDecoder = oggDecoder;

        this.mp3Decoder = mp3Decoder;
        this.audioManager = audioManager;
        this.path = path;

        this.audioManager.DeviceChanging += AudioManager_DeviceChanging;
        this.audioManager.DeviceChanged += AudioManager_DeviceChanged;

        Init();
    }

    /// <inheritdoc/>
    public string Name => this.path.GetFileNameWithoutExtension(FilePath);

    /// <inheritdoc/>
    public string FilePath { get; }

    /// <inheritdoc/>
    public float Volume
    {
        get
        {
            if (this.isDisposed)
            {
                throw new InvalidOperationException(IsDisposedExceptionMessage);
            }

            // Get the current volume between 0.0 and 1.0
            return this.ignoreOpenALCalls
                ? 0
                : this.alInvoker.GetSource(this.srcId, ALSourcef.Gain) * 100f;
        }
        set
        {
            if (this.isDisposed)
            {
                throw new InvalidOperationException(IsDisposedExceptionMessage);
            }

            if (this.ignoreOpenALCalls)
            {
                return;
            }

            // Make sure that the incoming value stays between 0 and 100
            value = value > 100f ? 100f : value;
            value = value < 0f ? 0f : value;

            // Convert the value to be between 0 and 1.
            // This is the excepted range by OpenAL
            value /= 100f;

            this.alInvoker.Source(this.srcId, ALSourcef.Gain, (float)Math.Round(value, 4));
        }
    }

    /// <inheritdoc/>
    public SoundTime Position
    {
        get
        {
            if (this.isDisposed)
            {
                throw new InvalidOperationException(IsDisposedExceptionMessage);
            }

            var seconds = this.ignoreOpenALCalls ? 0f : this.alInvoker.GetSource(this.srcId, ALSourcef.SecOffset);

            return new SoundTime(seconds);
        }
    }

    /// <inheritdoc/>
    public SoundTime Length => new (this.totalSeconds);

    /// <inheritdoc/>
    public bool IsLooping
    {
        get
        {
            if (this.isDisposed)
            {
                throw new InvalidOperationException(IsDisposedExceptionMessage);
            }

            return !this.ignoreOpenALCalls && this.alInvoker.GetSource(this.srcId, ALSourceb.Looping);
        }
        set
        {
            if (this.isDisposed)
            {
                throw new InvalidOperationException(IsDisposedExceptionMessage);
            }

            if (this.ignoreOpenALCalls)
            {
                return;
            }

            this.alInvoker.Source(this.srcId, ALSourceb.Looping, value);
        }
    }

    /// <inheritdoc/>
    public SoundState State
    {
        get
        {
            if (this.isDisposed)
            {
                throw new InvalidOperationException(IsDisposedExceptionMessage);
            }

            if (this.ignoreOpenALCalls)
            {
                return SoundState.Stopped;
            }
            else
            {
                var currentState = this.alInvoker.GetSourceState(this.srcId);

                return currentState switch
                {
                    ALSourceState.Playing => SoundState.Playing,
                    ALSourceState.Paused => SoundState.Paused,
                    ALSourceState.Stopped => SoundState.Stopped,
                    ALSourceState.Initial => SoundState.Stopped,
                    _ => throw new AudioException($"The OpenAL sound state of '{nameof(ALSourceState)}: {(int)currentState}' is not valid."),
                };
            }
        }
    }

    /// <inheritdoc/>
    public float PlaySpeed
    {
        get
        {
            if (this.isDisposed)
            {
                throw new InvalidOperationException(IsDisposedExceptionMessage);
            }

            return this.ignoreOpenALCalls
                ? 0f
                : this.alInvoker.GetSource(this.srcId, ALSourcef.Pitch);
        }
        set
        {
            value = value < 0f ? 0.25f : value;
            value = value > 2.0f ? 2.0f : value;

            if (this.ignoreOpenALCalls)
            {
                return;
            }

            this.alInvoker.Source(this.srcId, ALSourcef.Pitch, value);
        }
    }

    /// <inheritdoc/>
    public void Play()
    {
        if (this.isDisposed)
        {
            throw new InvalidOperationException(IsDisposedExceptionMessage);
        }

        if (State == SoundState.Playing)
        {
            return;
        }

        if (this.ignoreOpenALCalls)
        {
            return;
        }

        this.alInvoker.SourcePlay(this.srcId);
    }

    /// <inheritdoc/>
    public void Pause()
    {
        if (this.isDisposed)
        {
            throw new InvalidOperationException(IsDisposedExceptionMessage);
        }

        if (this.ignoreOpenALCalls)
        {
            return;
        }

        this.alInvoker.SourcePause(this.srcId);
    }

    /// <inheritdoc/>
    public void Stop()
    {
        if (this.isDisposed)
        {
            throw new InvalidOperationException(IsDisposedExceptionMessage);
        }

        if (this.ignoreOpenALCalls)
        {
            return;
        }

        this.alInvoker.SourceStop(this.srcId);
    }

    /// <inheritdoc/>
    public void Reset()
    {
        if (this.isDisposed)
        {
            throw new InvalidOperationException(IsDisposedExceptionMessage);
        }

        if (this.ignoreOpenALCalls)
        {
            return;
        }

        this.alInvoker.SourceRewind(this.srcId);
    }

    /// <inheritdoc/>
    public void SetTimePosition(float seconds)
    {
        if (this.isDisposed)
        {
            throw new InvalidOperationException(IsDisposedExceptionMessage);
        }

        if (this.ignoreOpenALCalls)
        {
            return;
        }

        // Prevent negative numbers
        seconds = seconds < 0f ? 0.0f : seconds;

        // Prevent a value past the end of the sound
        seconds = seconds > this.totalSeconds ? this.totalSeconds : seconds;

        this.alInvoker.Source(this.srcId, ALSourcef.SecOffset, seconds);
    }

    /// <inheritdoc/>
    public void Rewind(float seconds)
    {
        if (this.ignoreOpenALCalls)
        {
            return;
        }

        var intendedSeconds = Position.TotalSeconds - seconds;

        // If the request to move backwards will result in
        // a value less then 0, just set to 0
        if (intendedSeconds < 0)
        {
            Reset();
            Play();
            return;
        }

        SetTimePosition(intendedSeconds);
    }

    /// <inheritdoc/>
    public void FastForward(float seconds)
    {
        if (this.ignoreOpenALCalls)
        {
            return;
        }

        var intendedSeconds = Position.TotalSeconds + seconds;

        // If the request to move forward will result in
        // a value past the end of the sound, just set to the end.
        if (intendedSeconds > this.totalSeconds)
        {
            Reset();
            return;
        }

        SetTimePosition(intendedSeconds);
    }

    /// <inheritdoc/>
    public void Dispose() => Dispose(true);

    /// <summary>
    /// Maps the given audio <paramref name="format"/> to the <see cref="ALFormat"/> type equivalent.
    /// </summary>
    /// <param name="format">The format to convert.</param>
    /// <returns>The <see cref="ALFormat"/> result.</returns>
    private static ALFormat MapFormat(AudioFormat format) => format switch
    {
        AudioFormat.Mono8 => ALFormat.Mono8,
        AudioFormat.Mono16 => ALFormat.Mono16,
        AudioFormat.MonoFloat32 => ALFormat.MonoFloat32Ext,
        AudioFormat.Stereo8 => ALFormat.Stereo8,
        AudioFormat.Stereo16 => ALFormat.Stereo16,
        AudioFormat.StereoFloat32 => ALFormat.StereoFloat32Ext,
        _ => throw new AudioException("Invalid or unknown audio format."),
    };

    /// <summary>
    /// The callback invoked when an OpenAL error occurs.
    /// </summary>
    /// <param name="errorMsg">The OpenAL message.</param>
    [ExcludeFromCodeCoverage]
    private static void ErrorCallback(string errorMsg) => throw new AudioException(errorMsg);

    /// <summary>
    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to dispose of managed resources.</param>
    private void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            if (disposing)
            {
                this.oggDecoder.Dispose();
                this.mp3Decoder.Dispose();
                this.audioManager.DeviceChanging -= AudioManager_DeviceChanging;
                this.audioManager.DeviceChanged -= AudioManager_DeviceChanged;
            }

            UnloadSoundData();

            // ReSharper disable HeapView.DelegateAllocation
            this.alInvoker.ErrorCallback -= ErrorCallback;

            // ReSharper restore HeapView.DelegateAllocation
            this.isDisposed = true;
        }
    }

    /// <summary>
    /// Initializes the sound.
    /// </summary>
    private void Init()
    {
        if (!this.audioManager.IsInitialized)
        {
            this.audioManager.InitDevice();
        }

        (this.srcId, this.bufferId) = this.audioManager.InitSound();

        var extension = this.path.GetExtension(FilePath);

        SoundSource soundSrc;

        switch (extension)
        {
            case ".ogg":
                var oggData = this.oggDecoder.LoadData(FilePath);

                UploadOggData(oggData);

                break;
            case ".mp3":
                var mp3Data = this.mp3Decoder.LoadData(FilePath);

                UploadMp3Data(mp3Data);

                break;
            default:
                throw new AudioException($"The file extension '{extension}' is not supported file type.");
        }

        var sizeInBytes = this.alInvoker.GetBuffer(this.bufferId, ALGetBufferi.Size);
        var totalChannels = this.alInvoker.GetBuffer(this.bufferId, ALGetBufferi.Channels);
        var bitDepth = this.alInvoker.GetBuffer(this.bufferId, ALGetBufferi.Bits);
        var frequency = this.alInvoker.GetBuffer(this.bufferId, ALGetBufferi.Frequency);

        var sampleLen = sizeInBytes * 8 / (totalChannels * bitDepth);

        this.totalSeconds = (float)sampleLen / frequency;

        soundSrc.SourceId = this.srcId;
        soundSrc.TotalSeconds = this.totalSeconds;

        this.audioManager.UpdateSoundSource(soundSrc);

        this.ignoreOpenALCalls = false;
    }

    /// <summary>
    /// Uploads Ogg audio data to the sound card.
    /// </summary>
    /// <param name="data">The ogg related sound data to upload.</param>
    private void UploadOggData(SoundData<float> data)
    {
        if (data.BufferData.Count <= 0)
        {
            throw new SoundDataException("No audio data exists.");
        }

        this.alInvoker.BufferData(
            this.bufferId,
            MapFormat(data.Format),
            data.BufferData.ToArray(),
            data.SampleRate);

        // Bind the buffer to the source
        this.alInvoker.Source(this.srcId, ALSourcei.Buffer, (int)this.bufferId);
    }

    /// <summary>
    /// Uploads MP3 audio data to the sound card.
    /// </summary>
    /// <param name="data">The mp3 related sound data to upload.</param>
    private void UploadMp3Data(SoundData<byte> data)
    {
        if (data.BufferData.Count <= 0)
        {
            throw new SoundDataException("No audio data exists.");
        }

        this.alInvoker.BufferData(
            this.bufferId,
            MapFormat(data.Format),
            data.BufferData.ToArray(),
            data.SampleRate);

        // Bind the buffer to the source
        this.alInvoker.Source(this.srcId, ALSourcei.Buffer, (int)this.bufferId);
    }

    /// <summary>
    /// Unloads the sound data from the card.
    /// </summary>
    private void UnloadSoundData()
    {
        if (this.srcId <= 0)
        {
            return;
        }

        if (this.ignoreOpenALCalls)
        {
            return;
        }

        this.alInvoker.DeleteSource(this.srcId);

        if (this.bufferId != 0)
        {
            this.alInvoker.DeleteBuffer(this.bufferId);
        }

        this.audioManager.RemoveSoundSource(this.srcId);
    }

    /// <summary>
    /// Puts the sound into an ignore mode to prevent OpenAL calls from occuring
    /// during an audio device change.
    /// </summary>
    private void AudioManager_DeviceChanging(object? sender, EventArgs e)
        => this.ignoreOpenALCalls = true;

    /// <summary>
    /// Invoked when the audio device has been changed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Contains various event related information.</param>
    private void AudioManager_DeviceChanged(object? sender, EventArgs e) => Init();
}
