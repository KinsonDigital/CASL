// <copyright file="Sound.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Data;
using CASL.Data.Exceptions;
using Devices;
using Devices.Factories;
using Exceptions;
using Factories;
using OpenAL;

/// <summary>
/// A single sound that can be played, paused etc.
/// </summary>
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global", Justification = "Users need to inherit.")]
public class Sound : ISound
{
    private const char CrossPlatDirSeparatorChar = '/';
    private const string IsDisposedExceptionMessage = "The sound is disposed.  You must create another sound instance.";
    private readonly IAudioDeviceManager audioManager;
    private readonly IAudioDataStream<float>? oggDataStream;
    private readonly IAudioDataStream<byte>? mp3DataStream;
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
    /// <param name="filePath">The path to the sound file.</param>
    [ExcludeFromCodeCoverage]
    public Sound(string filePath)
    {
        FilePath = filePath.ToCrossPlatPath().TrimAllFromEnd(CrossPlatDirSeparatorChar);

        this.alInvoker = IoC.Container.GetInstance<IOpenALInvoker>();
        this.alInvoker.ErrorCallback += ErrorCallback;

        this.audioManager = AudioDeviceManagerFactory.CreateDeviceManager();
        this.audioManager.DeviceChanging += AudioManager_DeviceChanging;
        this.audioManager.DeviceChanged += AudioManager_DeviceChanged;
        this.path = IoC.Container.GetInstance<IPath>();

        var extension = this.path.GetExtension(FilePath).ToLower();
        var dataStreamFactory = IoC.Container.GetInstance<IAudioDataStreamFactory>();

        switch (extension)
        {
            case ".ogg":
                this.oggDataStream = dataStreamFactory.CreateOggAudioStream(FilePath);
                break;
            case ".mp3":
                this.mp3DataStream = dataStreamFactory.CreateMp3AudioStream(FilePath);
                break;
        }

        Init();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sound"/> class.
    /// </summary>
    /// <param name="filePath">The path to the sound file.</param>
    /// <param name="alInvoker">Provides access to OpenAL.</param>
    /// <param name="audioManager">Manages audio related operations.</param>
    /// <param name="dataStreamFactory">Creates audio data streams.</param>
    /// <param name="path">Manages file paths.</param>
    /// <param name="file">Performs operations with files.</param>
    internal Sound(
        string filePath,
        IOpenALInvoker alInvoker,
        IAudioDeviceManager audioManager,
        IAudioDataStreamFactory dataStreamFactory,
        IPath path,
        IFile file)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(alInvoker);
        ArgumentNullException.ThrowIfNull(audioManager);
        ArgumentNullException.ThrowIfNull(dataStreamFactory);
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(file);

        if (!file.Exists(filePath))
        {
            throw new FileNotFoundException($"The sound file could not be found.", filePath);
        }

        FilePath = filePath.ToCrossPlatPath().TrimAllFromEnd(CrossPlatDirSeparatorChar);

        this.alInvoker = alInvoker;
        this.alInvoker.ErrorCallback += ErrorCallback;

        var extension = path.GetExtension(FilePath).ToLower();

        switch (extension)
        {
            case ".ogg":
                this.oggDataStream = dataStreamFactory.CreateOggAudioStream(FilePath);
                break;
            case ".mp3":
                this.mp3DataStream = dataStreamFactory.CreateMp3AudioStream(FilePath);
                break;
        }

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
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to dispose of managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (this.isDisposed)
        {
            return;
        }

        if (disposing)
        {
            this.oggDataStream?.Dispose();
            this.mp3DataStream?.Dispose();
            this.audioManager.DeviceChanging -= AudioManager_DeviceChanging;
            this.audioManager.DeviceChanged -= AudioManager_DeviceChanged;
        }

        UnloadSound();

        // ReSharper disable HeapView.DelegateAllocation
        this.alInvoker.ErrorCallback -= ErrorCallback;

        // ReSharper restore HeapView.DelegateAllocation
        this.isDisposed = true;
    }

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
                ArgumentNullException.ThrowIfNull(this.oggDataStream);

                this.oggDataStream.Flush();

                var oggData = new AudioData<float>
                {
                    Format = this.oggDataStream.Format,
                    Channels = this.oggDataStream.Channels,
                    SampleRate = this.oggDataStream.SampleRate,
                };

                var oggDataResult = new List<float>();
                var oggBuffer = new float[this.oggDataStream.TotalSamples * this.oggDataStream.Channels];

                this.oggDataStream.ReadSamples(oggBuffer);
                oggDataResult.AddRange(oggBuffer);

                oggData = oggData with { BufferData = new ReadOnlyCollection<float>(oggDataResult) };

                UploadOggData(oggData);

                break;
            case ".mp3":
                ArgumentNullException.ThrowIfNull(this.mp3DataStream);

                this.mp3DataStream.Flush();

                var mp3Data = new AudioData<byte>
                {
                    SampleRate = this.mp3DataStream.SampleRate,
                    Channels = this.mp3DataStream.Channels,
                    Format = this.mp3DataStream.Format,
                };

                var mp3DataResult = new List<byte>();

                const int bytesPerChunk = 32_768;

                var mp3Buffer = new byte[bytesPerChunk];
                while (this.mp3DataStream.ReadSamples(mp3Buffer) > 0)
                {
                    mp3DataResult.AddRange(mp3Buffer);
                }

                mp3Data = mp3Data with { BufferData = new ReadOnlyCollection<byte>(mp3DataResult) };

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
    /// <param name="data">The ogg related audio data to upload.</param>
    private void UploadOggData(AudioData<float> data)
    {
        if (data.BufferData.Count <= 0)
        {
            throw new AudioDataException("No audio data exists.");
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
    /// <param name="data">The mp3 related audio data to upload.</param>
    private void UploadMp3Data(AudioData<byte> data)
    {
        if (data.BufferData.Count <= 0)
        {
            throw new AudioDataException("No audio data exists.");
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
    /// Unloads the audio data from the card.
    /// </summary>
    private void UnloadSound()
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
    /// Puts the sound into an ignore mode to prevent OpenAL calls from occurring
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
