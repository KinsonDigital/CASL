// <copyright file="StreamBuffer.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data;

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using Carbonate;
using Decoders;
using Devices;
using DotnetWrappers;
using Factories;
using OpenAL;
using ReactableData;

/// <inheritdoc/>
internal sealed class StreamBuffer : IAudioBuffer
{
    private const int TotalDataBuffers = 4;
    private readonly IOpenALInvoker alInvoker;
    private readonly IAudioDeviceManager audioDeviceManager;
    private readonly IAudioDecoder audioDecoder;
    private readonly IStreamBufferManager streamBufferManager;
    private readonly ITaskService taskService;
    private readonly IThreadService threadService;
    private readonly IFile file;
    private readonly IPath path;
    private readonly IDisposable audioCmdUnsubscriber;
    private readonly IDisposable posCmdUnsubscriber;
    private readonly IDisposable loopingUnsubscriber;
    private readonly uint[] bufferIds = new uint[TotalDataBuffers];
    private AudioFormatType audioFormatType;
    private uint srcId;
    private bool isInitialized;
    private bool isDisposed;
    private bool audioDeviceChanging;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamBuffer"/> class.
    /// </summary>
    /// <param name="alInvoker">Provides access to OpenAL.</param>
    /// <param name="audioDeviceManager">Manages audio device related operations.</param>
    /// <param name="audioDecoder">Decodes audio data from an audio file.</param>
    /// <param name="streamBufferManager">Manages stream buffers.</param>
    /// <param name="reactableFactory">Creates reactables.</param>
    /// <param name="taskService">Manages a running task.</param>
    /// <param name="threadService">Provides thread functionality.</param>
    /// <param name="path">Manages file paths.</param>
    /// <param name="file">Performs operations with files.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if any of the following parameters are null:
    ///     <list type="bullet">
    ///         <item><paramref name="alInvoker"/></item>
    ///         <item><paramref name="audioDeviceManager"/></item>
    ///         <item><paramref name="audioDecoder"/></item>
    ///         <item><paramref name="reactableFactory"/></item>
    ///         <item><paramref name="taskService"/></item>
    ///         <item><paramref name="threadService"/></item>
    ///         <item><paramref name="path"/></item>
    ///         <item><paramref name="file"/></item>
    ///     </list>
    /// </exception>
    [SuppressMessage("csharpsquid", "S107", Justification = "Not part of the public API.")]
    public StreamBuffer(
        IOpenALInvoker alInvoker,
        IAudioDeviceManager audioDeviceManager,
        IAudioDecoder audioDecoder,
        IStreamBufferManager streamBufferManager,
        IReactableFactory reactableFactory,
        ITaskService taskService,
        IThreadService threadService,
        IPath path,
        IFile file)
    {
        ArgumentNullException.ThrowIfNull(alInvoker);
        ArgumentNullException.ThrowIfNull(audioDeviceManager);
        ArgumentNullException.ThrowIfNull(audioDecoder);
        ArgumentNullException.ThrowIfNull(streamBufferManager);
        ArgumentNullException.ThrowIfNull(reactableFactory);
        ArgumentNullException.ThrowIfNull(taskService);
        ArgumentNullException.ThrowIfNull(threadService);
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(file);

        this.alInvoker = alInvoker;
        this.audioDeviceManager = audioDeviceManager;
        this.audioDecoder = audioDecoder;
        this.streamBufferManager = streamBufferManager;
        this.taskService = taskService;
        this.threadService = threadService;
        this.path = path;
        this.file = file;

        var audioCmdReactable = reactableFactory.CreateAudioCmndReactable();
        var posCmdReactable = reactableFactory.CreatePositionCmndReactable();
        var loopingReactable = reactableFactory.CreateIsLoopingReactable();

        this.audioCmdUnsubscriber = audioCmdReactable.CreateOneWayReceive(
            PushNotifications.SendAudioCmd,
            name: nameof(StreamBuffer),
            ProcessAudioCmd,
            () => this.audioCmdUnsubscriber?.Dispose());

        this.posCmdUnsubscriber = posCmdReactable.CreateOneWayReceive(
            PushNotifications.UpdateSoundPos,
            name: nameof(StreamBuffer),
            ProcessPosCmd,
            () => this.posCmdUnsubscriber?.Dispose());

        this.loopingUnsubscriber = loopingReactable.CreateOneWayRespond(
            PullNotifications.GetLoopState,
            name: nameof(FullBuffer),
            () => IsLooping,
            () => this.loopingUnsubscriber?.Dispose());

        this.audioDeviceManager.DeviceChanging += DeviceChanging;
        this.audioDeviceManager.DeviceChanged += DeviceChanged;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="StreamBuffer"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Finalizers cannot be tested")]
    ~StreamBuffer() => Dispose(false);

    /// <inheritdoc/>
    public float TotalSeconds => this.audioDecoder.TotalSeconds;

    /// <inheritdoc/>
    public AudioTime Position
    {
        get
        {
            var totalSeconds = this.streamBufferManager.ToPositionSeconds(
                this.audioDecoder.TotalSampleFrames,
                this.audioDecoder.TotalSeconds);

            return new (totalSeconds);
        }
    }

    /// <inheritdoc/>
    public bool IsLooping { get; private set; }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    ///     Thrown for the follow reasons:
    ///     <list type="bullet">
    ///         <item>If the <paramref name="filePath"/> is null or empty.</item>
    ///         <item>If the file type is not an <b>.ogg</b> or <b>.mp3</b> file type.</item>
    ///     </list>
    /// </exception>
    /// <exception cref="FileNotFoundException">Thrown if the file is not found.</exception>
    public uint Init(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        if (!this.file.Exists(filePath))
        {
            throw new FileNotFoundException($"The audio file could not be found.", filePath);
        }

        if (!this.audioDeviceManager.IsInitialized)
        {
            this.audioDeviceManager.InitDevice();
        }

        var extension = this.path.GetExtension(filePath).ToLower();

        var exMsg = $"The file extension '{extension}' is not supported.";
        exMsg += " Supported extensions are '.ogg' and '.mp3'.";

        this.audioFormatType = extension switch
        {
            ".ogg" => AudioFormatType.Ogg,
            ".mp3" => AudioFormatType.Mp3,
            _ => throw new ArgumentException(exMsg, nameof(filePath)),
        };

        this.srcId = this.alInvoker.GenSource();
        var newBufferIds = this.alInvoker.GenBuffers(TotalDataBuffers);

        for (var i = 0; i < newBufferIds.Length; i++)
        {
            this.bufferIds[i] = newBufferIds[i];
        }

        this.isInitialized = true;

        return this.srcId;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown if the buffer has not been initialized.</exception>
    /// <exception cref="InvalidEnumArgumentException">
    ///     Thrown if the <see cref="AudioFormat"/> is not a valid enum value.
    /// </exception>
    public void Upload()
    {
        if (!this.isInitialized)
        {
            throw new InvalidOperationException("The buffer has not been initialized.");
        }

        FillBuffersFromStart();

        if (this.taskService.IsRunning)
        {
            return;
        }

        this.taskService.SetAction(StreamData);
        this.taskService.Start();
    }

    /// <inheritdoc/>
    public void RemoveBuffer()
    {
        this.alInvoker.Source(this.srcId, ALSourcei.Buffer, 0);

        foreach (var bufferId in this.bufferIds)
        {
            this.alInvoker.DeleteBuffer(bufferId);
        }
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

        if (disposing)
        {
            this.audioDeviceManager.DeviceChanging -= DeviceChanging;
            this.audioDeviceManager.DeviceChanged -= DeviceChanged;

            this.taskService.Cancel();

            while (true)
            {
                if (this.taskService.IsCompletedSuccessfully)
                {
                    break;
                }
            }

            this.taskService.Dispose();

            this.audioCmdUnsubscriber.Dispose();
            this.posCmdUnsubscriber.Dispose();
            this.loopingUnsubscriber.Dispose();
            this.audioDecoder.Dispose();
        }

        this.alInvoker.SourceStop(this.srcId);
        RemoveBuffer();

        this.isDisposed = true;
    }

    /// <summary>
    /// Puts the buffer into a state of changing audio devices.
    /// </summary>
    private void DeviceChanging(object? sender, EventArgs e) => this.audioDeviceChanging = true;

    /// <summary>
    /// Puts the buffer into a state of not changing audio devices.
    /// </summary>
    private void DeviceChanged(object? sender, EventArgs e) => this.audioDeviceChanging = false;

    /// <summary>
    /// Processes audio commands.
    /// </summary>
    /// <param name="data">The audio command data.</param>
    private void ProcessAudioCmd(AudioCommandData data)
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
                Reset();
                break;
            case AudioCommands.EnableLooping:
                IsLooping = true;
                break;
            case AudioCommands.DisableLooping:
                IsLooping = false;
                break;
        }
    }

    /// <summary>
    /// Processes audio position commands.
    /// </summary>
    /// <param name="data">The position command data.</param>
    /// <exception cref="InvalidEnumArgumentException">
    ///     Occurs if the <see cref="AudioFormatType"/> is not a valid enum value.
    /// </exception>
    private void ProcessPosCmd(PosCommandData data)
    {
        if (data.SourceId != this.srcId || !this.isInitialized)
        {
            return;
        }

        if (Math.Floor(data.PositionSeconds) >= Math.Floor(this.audioDecoder.TotalSeconds))
        {
            FillBuffersFromStart();

            if (IsLooping)
            {
                this.alInvoker.SourcePlay(this.srcId);
            }

            return;
        }

        var samplePos = this.streamBufferManager.ToPositionSamples(
            data.PositionSeconds,
            this.audioDecoder.TotalSeconds,
            this.audioDecoder.TotalSampleFrames);

        var wasPlaying = this.alInvoker.GetSourceState(this.srcId) == ALSourceState.Playing;

        this.alInvoker.SourceStop(this.srcId);

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (this.audioFormatType)
        {
            case AudioFormatType.Mp3:
                this.audioDecoder.ReadUpTo((uint)samplePos * 4);
                break;
            case AudioFormatType.Ogg:
                this.audioDecoder.ReadUpTo((uint)samplePos * 2);
                break;
        }

        this.streamBufferManager.SetSamplePos(samplePos);

        if (wasPlaying)
        {
            this.alInvoker.SourcePlay(this.srcId);
        }
    }

    /// <summary>
    /// Streams chunks of audio data to the audio device over time.
    /// </summary>
    /// <exception cref="InvalidEnumArgumentException">
    ///     Thrown if the <see cref="AudioFormat"/> is not a valid enum value.
    /// </exception>
    private void StreamData()
    {
        while (true)
        {
            if (this.taskService.IsCancellationRequested)
            {
                break;
            }

            if (this.audioDeviceChanging)
            {
                this.threadService.Sleep(100);
                continue;
            }

            // If the current position has reached the end of the audio, reset and start playing
            // again if the audio is set to loop.
            if (Math.Floor(Position.TotalSeconds) >= Math.Floor(this.audioDecoder.TotalSeconds))
            {
                Reset();

                if (IsLooping)
                {
                    this.alInvoker.SourcePlay(this.srcId);
                }
            }

            var srcState = this.alInvoker.GetSourceState(this.srcId);
            if (srcState is ALSourceState.Paused or ALSourceState.Initial or ALSourceState.Stopped)
            {
                this.threadService.Sleep(125);
                continue;
            }

            var bufferStats = new BufferStats
            {
                SourceId = this.srcId,
                FormatType = this.audioFormatType,
                DecoderFormat = this.audioDecoder.Format,
                SampleRate = this.audioDecoder.SampleRate,
                TotalChannels = this.audioDecoder.TotalChannels,
            };

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (this.audioFormatType)
            {
                case AudioFormatType.Mp3:
                    this.streamBufferManager.ManageBuffers(bufferStats, ReadSampleData<byte>);
                    break;
                case AudioFormatType.Ogg:
                    this.streamBufferManager.ManageBuffers(bufferStats, ReadSampleData<float>);
                    break;
            }

            var playSpeed = this.alInvoker.GetSource(this.srcId, ALSourcef.Pitch);

            // Adjust the time to sleep based on the play speed.
            // The faster the play speed, the more often buffer attempts need to be made,
            // which means we need to sleep less to make sure we keep up.
            var sleepTime = playSpeed.MapValue(1f, 2f, 100, 0);

            // Sleep for a bit to avoid unnecessary CPU usage
            // NOTE: This made a difference of 0.3% to over 25% CPU when using
            // Thread.Sleep() vs Task.Delay().  Task.Delay() is not efficient for this use case.
            this.threadService.Sleep(sleepTime);
        }
    }

    /// <summary>
    /// Resets the decoder and the audio buffers back to the beginning of the audio.
    /// </summary>
    private void Reset()
    {
        this.streamBufferManager.UnqueueProcessedBuffers(this.srcId);
        FillBuffersFromStart();
        this.alInvoker.SourceRewind(this.srcId);
        this.streamBufferManager.ResetSamplePos();
    }

    /// <summary>
    /// Reads and returns the sample data from the audio decoder.
    /// </summary>
    /// <typeparam name="T">The type of dat to return.</typeparam>
    /// <returns>The sample data.</returns>
    private T[] ReadSampleData<T>()
        where T : unmanaged
    {
        this.audioDecoder.ReadSamples();

        return this.audioDecoder.GetSampleData<T>();
    }

    /// <summary>
    /// Fills the buffers from the beginning of the audio data.
    /// </summary>
    /// <exception cref="InvalidEnumArgumentException">
    ///     Thrown if the <see cref="AudioFormat"/> is not a valid enum value.
    /// </exception>
    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach", Justification = "Need as for loop for reference to buffer id.")]
    private void FillBuffersFromStart()
    {
        var bufferStats = new BufferStats
        {
            SourceId = this.srcId,
            FormatType = this.audioFormatType,
            DecoderFormat = this.audioDecoder.Format,
            SampleRate = this.audioDecoder.SampleRate,
            TotalChannels = this.audioDecoder.TotalChannels,
        };

        switch (this.audioFormatType)
        {
            case AudioFormatType.Mp3:
                this.streamBufferManager.FillBuffersFromStart(
                    bufferStats,
                    this.bufferIds,
                    this.audioDecoder.Flush,
                    ReadSampleData<byte>);
                break;
            case AudioFormatType.Ogg:
                this.streamBufferManager.FillBuffersFromStart(
                    bufferStats,
                    this.bufferIds,
                    this.audioDecoder.Flush,
                    ReadSampleData<float>);
                break;
        }
    }
}
