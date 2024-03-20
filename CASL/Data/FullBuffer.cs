// <copyright file="FullBuffer.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using Carbonate;
using CASL.Exceptions;
using Decoders;
using Devices;
using Factories;
using OpenAL;
using ReactableData;

/// <inheritdoc/>
internal sealed class FullBuffer : IAudioBuffer
{
    private readonly IOpenALInvoker alInvoker;
    private readonly IAudioDeviceManager audioDeviceManager;
    private readonly IPath path;
    private readonly IFile file;
    private readonly IDisposable audioCmdUnsubscriber;
    private readonly IDisposable posCmdUnsubscriber;
    private readonly IDisposable loopingUnsubscriber;
    private readonly IAudioDecoder audioDecoder;
    private AudioFormatType audioFormatType;
    private uint bufferId;
    private uint srcId;
    private bool isInitialized;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FullBuffer"/> class.
    /// </summary>
    /// <param name="alInvoker">Provides access to OpenAL.</param>
    /// <param name="audioDeviceManager">Manages audio device related operations.</param>
    /// <param name="audioDecoder">Decodes audio data from an audio file.</param>
    /// <param name="reactableFactory">Creates reactables.</param>
    /// <param name="path">Manages file paths.</param>
    /// <param name="file">Performs operations with files.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the following constructor parameters are null:
    ///     <list type="bullet">
    ///         <item><paramref name="alInvoker"/></item>
    ///         <item><paramref name="audioDeviceManager"/></item>
    ///         <item><paramref name="audioDecoder"/></item>
    ///         <item><paramref name="reactableFactory"/></item>
    ///         <item><paramref name="path"/></item>
    ///         <item><paramref name="file"/></item>
    ///     </list>
    /// </exception>
    public FullBuffer(
        IOpenALInvoker alInvoker,
        IAudioDeviceManager audioDeviceManager,
        IAudioDecoder audioDecoder,
        IReactableFactory reactableFactory,
        IPath path,
        IFile file)
    {
        ArgumentNullException.ThrowIfNull(alInvoker);
        ArgumentNullException.ThrowIfNull(audioDeviceManager);
        ArgumentNullException.ThrowIfNull(audioDecoder);
        ArgumentNullException.ThrowIfNull(reactableFactory);
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(file);

        this.alInvoker = alInvoker;
        this.audioDeviceManager = audioDeviceManager;
        this.audioDecoder = audioDecoder;
        this.path = path;
        this.file = file;

        var audioCmdReactable = reactableFactory.CreateAudioCmndReactable();
        var posCmdReactable = reactableFactory.CreatePositionCmndReactable();
        var loopingReactable = reactableFactory.CreateIsLoopingReactable();

        this.audioCmdUnsubscriber = audioCmdReactable.CreateOneWayReceive(
            PushNotifications.SendAudioCmd,
            name: nameof(FullBuffer),
            (data) =>
            {
                if (data.SourceId != this.srcId)
                {
                    return;
                }

                switch (data.Command)
                {
                    case AudioCommands.Play:
                        this.alInvoker.SourcePlay(this.srcId);
                        break;
                    case AudioCommands.Pause:
                        this.alInvoker.SourcePause(this.srcId);
                        break;
                    case AudioCommands.Reset:
                        this.alInvoker.SourceRewind(this.srcId);
                        break;
                    case AudioCommands.EnableLooping:
                        this.alInvoker.Source(this.srcId, ALSourceb.Looping, true);
                        break;
                    case AudioCommands.DisableLooping:
                        this.alInvoker.Source(this.srcId, ALSourceb.Looping, false);
                        break;
                }
            },
            () => this.audioCmdUnsubscriber?.Dispose());

        this.posCmdUnsubscriber = posCmdReactable.CreateOneWayReceive(
            PushNotifications.UpdateSoundPos,
            name: nameof(FullBuffer),
            (data) =>
            {
                if (data.SourceId != this.srcId)
                {
                    return;
                }

                this.alInvoker.Source(this.srcId, ALSourcef.SecOffset, data.PositionSeconds);
            },
            () => this.posCmdUnsubscriber?.Dispose());

        this.loopingUnsubscriber = loopingReactable.CreateOneWayRespond(
            PullNotifications.GetLoopState,
            name: nameof(FullBuffer),
            () => this.alInvoker.GetSource(this.srcId, ALSourceb.Looping),
            () => this.loopingUnsubscriber?.Dispose());
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="FullBuffer"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Finalizers cannot be tested")]
    ~FullBuffer() => Dispose(false);

    /// <inheritdoc/>
    public float TotalSeconds => this.audioDecoder.TotalSeconds;

    /// <inheritdoc/>
    public SoundTime Position
    {
        get
        {
            if (!this.isInitialized)
            {
                return default;
            }

            var seconds = this.alInvoker.GetSource(this.srcId, ALSourcef.SecOffset);

            return new SoundTime(seconds);
        }
    }

    /// <inheritdoc/>
    public bool IsLooping => this.alInvoker.GetSource(this.srcId, ALSourceb.Looping);

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Thrown if the given <paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the given <paramref name="filePath"/> does not exist.</exception>
    public uint Init(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        if (this.isInitialized)
        {
            return this.srcId;
        }

        if (!this.file.Exists(filePath))
        {
            throw new FileNotFoundException("The sound file could not be found.", filePath);
        }

        var extension = this.path.GetExtension(filePath).ToLower();

        this.audioFormatType = extension switch
        {
            ".mp3" => AudioFormatType.Mp3,
            ".ogg" => AudioFormatType.Ogg,
            _ => this.audioFormatType
        };

        if (!this.audioDeviceManager.IsInitialized)
        {
            this.audioDeviceManager.InitDevice();
        }

        (this.srcId, var bufferIds) = this.audioDeviceManager.InitSound(1);
        this.bufferId = bufferIds[0];

        SoundSource soundSrc;
        soundSrc.SourceId = this.srcId;
        soundSrc.TotalSeconds = TotalSeconds;

        this.audioDeviceManager.UpdateSoundSource(soundSrc);

        this.isInitialized = true;

        return this.srcId;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown if the buffer has not been initialized.</exception>
    /// <exception cref="AudioException">Thrown if the audio format is not supported or unknown.</exception>
    public void Upload()
    {
        if (!this.isInitialized)
        {
            throw new InvalidOperationException("The buffer has not been initialized.");
        }

        // Buffer all of the data
        this.audioDecoder.ReadAllSamples();

        switch (this.audioFormatType)
        {
            case AudioFormatType.Ogg:
                var oggBufferData = this.audioDecoder.GetSampleData<float>();

                this.alInvoker.BufferData(
                    this.bufferId,
                    this.audioDecoder.Format,
                    oggBufferData,
                    this.audioDecoder.SampleRate);
                break;
            case AudioFormatType.Mp3:
                var mp3BufferData = this.audioDecoder.GetSampleData<byte>();
                this.alInvoker.BufferData(
                    this.bufferId,
                    this.audioDecoder.Format,
                    mp3BufferData,
                    this.audioDecoder.SampleRate);
                break;
            default:
                var expectedMsg = "The audio format type is not supported.";
                expectedMsg += "\nSupported audio format types: .mp3, .ogg";
                throw new AudioException(expectedMsg);
        }

        // Bind the buffer to the source
        this.alInvoker.Source(this.srcId, ALSourcei.Buffer, (int)this.bufferId);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// <param name="disposing">True to dispose of managed resources.</param>
    private void Dispose(bool disposing)
    {
        if (this.isDisposed)
        {
            return;
        }

        // NOTE: Do not dispose of the device manager.  This needs to stay alive for other sounds that could be loaded in the future.
        if (disposing)
        {
            this.audioCmdUnsubscriber.Dispose();
            this.posCmdUnsubscriber.Dispose();
            this.loopingUnsubscriber.Dispose();
            this.audioDecoder.Dispose();
        }

        this.alInvoker.SourceStop(this.srcId);
        this.alInvoker.Source(this.srcId, ALSourcei.Buffer, 0);

        this.alInvoker.DeleteBuffer(this.bufferId);

        this.audioDeviceManager.RemoveSoundSource(this.srcId);

        this.isDisposed = true;
    }
}
