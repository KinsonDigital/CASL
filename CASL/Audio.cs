// <copyright file="Audio.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using Carbonate.OneWay;
using Data;
using Devices;
using Exceptions;
using Factories;
using OpenAL;
using ReactableData;

/// <summary>
/// A single sound that can be played, paused etc.
/// </summary>
[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global", Justification = "Users need to inherit.")]
public class Audio : IAudio
{
    private const char CrossPlatDirSeparatorChar = '/';
    private const string IsDisposedExceptionMessage = "The sound is disposed.  You must create another sound instance.";
    private readonly IAudioDeviceManager audioManager;
    private readonly IPushReactable<AudioCommandData> audioCommandReactable;
    private readonly IPushReactable<PosCommandData> posCommandReactable;
    private readonly IPullReactable<bool> loopingReactable;
    private readonly IOpenALInvoker alInvoker;
    private readonly IAudioBuffer audioBuffer;
    private readonly IPath path;
    private uint srcId;
    private bool isDisposed;
    private bool audioDeviceChanging;
    private AudioTime posBeforeDeviceChange;
    private ALSourceState stateBeforeDeviceChange;
    private float volumeBeforeDeviceChange = -1;
    private float playSpeedBeforeDeviceChange;

    /// <summary>
    /// Initializes a new instance of the <see cref="Audio"/> class.
    /// </summary>
    /// <param name="filePath">The path to the sound file.</param>
    /// <param name="bufferType">The type of audio buffer used.</param>
    /// <remarks>
    ///     Using <see cref="CASL.BufferType"/>.<see cref="CASL.BufferType.Full"/> means all of the audio data will be loaded into memory.
    ///     This is fine for small audio files like sound effects.  Large audio files will consume more memory and take longer to load.
    ///     <para/>
    ///     Using <see cref="CASL.BufferType"/>.<see cref="CASL.BufferType.Stream"/> means all of the audio data will be loaded/streamed into
    ///     This is better for larger audio files like music. It will consume less memory and much better for performances from a loading perspective.
    ///     in chunks as needed memory.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="InvalidEnumArgumentException">Thrown if the <paramref name="bufferType"/> is invalid.</exception>
    ///
    [ExcludeFromCodeCoverage(Justification = "Directly interacts with the IoC container.")]
    public Audio(string filePath, BufferType bufferType)
    {
        var file = IoC.Container.GetInstance<IFile>();

        if (!file.Exists(filePath))
        {
            throw new FileNotFoundException($"The sound file could not be found.", filePath);
        }

        FilePath = filePath.ToCrossPlatPath().TrimAllFromEnd(CrossPlatDirSeparatorChar);

        this.alInvoker = IoC.Container.GetInstance<IOpenALInvoker>();
        this.alInvoker.ErrorCallback += ErrorCallback;

        this.audioManager = IoC.Container.GetInstance<IAudioDeviceManager>();
        this.audioManager.DeviceChanging += AudioManager_DeviceChanging;
        this.audioManager.DeviceChanged += AudioManager_DeviceChanged;
        this.path = IoC.Container.GetInstance<IPath>();

        var reactableFactory = IoC.Container.GetInstance<IReactableFactory>();
        this.audioCommandReactable = reactableFactory.CreateAudioCmndReactable();
        this.posCommandReactable = reactableFactory.CreatePositionCmndReactable();
        this.loopingReactable = reactableFactory.CreateIsLoopingReactable();

        var bufferFactory = IoC.Container.GetInstance<IAudioBufferFactory>();

        this.audioBuffer = bufferType switch
        {
            BufferType.Full => bufferFactory.CreateFullBuffer(filePath),
            BufferType.Stream => bufferFactory.CreateStreamBuffer(filePath),
            _ => throw new InvalidEnumArgumentException(nameof(bufferType), (int)bufferType, typeof(BufferType))
        };

        Init();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Audio"/> class.
    /// </summary>
    /// <param name="filePath">The path to the sound file.</param>
    /// <param name="bufferType">The type of audio buffer used.</param>
    /// <param name="alInvoker">Provides access to OpenAL.</param>
    /// <param name="audioManager">Manages audio device related operations.</param>
    /// <param name="reactableFactory">Creates reactables.</param>
    /// <param name="path">Manages file paths.</param>
    /// <param name="file">Performs operations with files.</param>
    /// <param name="bufferFactory">Creates audio buffers.</param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the following parameters are null:
    /// <list type="bullet">
    ///     <item><paramref name="alInvoker"/></item>
    ///     <item><paramref name="audioManager"/></item>
    ///     <item><paramref name="bufferFactory"/></item>
    ///     <item><paramref name="reactableFactory"/></item>
    ///     <item><paramref name="path"/></item>
    ///     <item><paramref name="file"/></item>
    /// </list>
    /// </exception>
    [SuppressMessage("csharpsquid", "S107", Justification = "Not part of the public API.")]
    internal Audio(
        string filePath,
        BufferType bufferType,
        IOpenALInvoker alInvoker,
        IAudioDeviceManager audioManager,
        IAudioBufferFactory bufferFactory,
        IReactableFactory reactableFactory,
        IPath path,
        IFile file)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(alInvoker);
        ArgumentNullException.ThrowIfNull(audioManager);
        ArgumentNullException.ThrowIfNull(bufferFactory);
        ArgumentNullException.ThrowIfNull(reactableFactory);
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(file);

        if (!file.Exists(filePath))
        {
            throw new FileNotFoundException($"The sound file could not be found.", filePath);
        }

        var extension = path.GetExtension(filePath).ToLower();

        var exMsg = $"The file extension '{extension}' is not supported.";
        exMsg += " Supported extensions are '.ogg' and '.mp3'.";

        if (extension != ".ogg" && extension != ".mp3")
        {
            throw new AudioException(exMsg);
        }

        FilePath = filePath.ToCrossPlatPath().TrimAllFromEnd(CrossPlatDirSeparatorChar);
        BufferType = bufferType;

        this.alInvoker = alInvoker;
        this.alInvoker.ErrorCallback += ErrorCallback;

        this.audioBuffer = bufferType switch
        {
            BufferType.Full => bufferFactory.CreateFullBuffer(filePath),
            BufferType.Stream => bufferFactory.CreateStreamBuffer(filePath),
            _ => throw new InvalidEnumArgumentException(nameof(bufferType), (int)bufferType, typeof(BufferType))
        };

        this.audioCommandReactable = reactableFactory.CreateAudioCmndReactable();
        this.posCommandReactable = reactableFactory.CreatePositionCmndReactable();
        this.loopingReactable = reactableFactory.CreateIsLoopingReactable();

        this.audioManager = audioManager;
        this.path = path;

        this.audioManager.DeviceChanging += AudioManager_DeviceChanging;
        this.audioManager.DeviceChanged += AudioManager_DeviceChanged;

        Init();
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="Audio"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Finalizers cannot be tested")]
    ~Audio() => Dispose(false);

    /// <inheritdoc/>
    public string Name => this.path.GetFileNameWithoutExtension(FilePath);

    /// <inheritdoc/>
    public string FilePath { get; }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="Audio"/> has been disposed.</exception>
    public float Volume
    {
        get
        {
            if (this.isDisposed)
            {
                return 0;
            }

            if (this.audioDeviceChanging)
            {
                return this.volumeBeforeDeviceChange;
            }

            return this.alInvoker.GetSource(this.srcId, ALSourcef.Gain) * 100f;
        }
        set
        {
            if (this.isDisposed)
            {
                throw new InvalidOperationException(IsDisposedExceptionMessage);
            }

            if (this.audioDeviceChanging)
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
    public AudioTime Position
    {
        get
        {
            if (this.isDisposed)
            {
                return default;
            }

            return this.audioDeviceChanging ? this.posBeforeDeviceChange : this.audioBuffer.Position;
        }
    }

    /// <inheritdoc/>
    public AudioTime Length
    {
        get
        {
            if (this.isDisposed)
            {
                return default;
            }

            var length = this.audioDeviceChanging ? 0f : this.audioBuffer.TotalSeconds;

            return new AudioTime(length);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="Audio"/> has been disposed.</exception>
    public bool IsLooping
    {
        get => !this.audioDeviceChanging && !this.isDisposed && this.loopingReactable.Pull(PullNotifications.GetLoopState);
        set
        {
            if (this.isDisposed)
            {
                throw new InvalidOperationException(IsDisposedExceptionMessage);
            }

            if (this.audioDeviceChanging)
            {
                return;
            }

            var state = new AudioCommandData
            {
                SourceId = this.srcId,
                Command = value ? AudioCommands.EnableLooping : AudioCommands.DisableLooping,
            };
            this.audioCommandReactable.Push(PushNotifications.SendAudioCmd, state);
        }
    }

    /// <inheritdoc/>
    public SoundState State
    {
        get
        {
            if (this.audioDeviceChanging || this.isDisposed)
            {
                return SoundState.Stopped;
            }

            var currentState = this.alInvoker.GetSourceState(this.srcId);

#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            return currentState switch
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            {
                ALSourceState.Playing => SoundState.Playing,
                ALSourceState.Paused => SoundState.Paused,
                ALSourceState.Stopped => SoundState.Stopped,
                ALSourceState.Initial => SoundState.Stopped,
            };
        }
    }

    /// <inheritdoc/>
    public BufferType BufferType { get; }

    /// <inheritdoc/>
    public float PlaySpeed
    {
        get => this.audioDeviceChanging || this.isDisposed
                ? 0f
                : this.alInvoker.GetSource(this.srcId, ALSourcef.Pitch);
        set
        {
            value = value < 0f ? 0.25f : value;
            value = value > 2.0f ? 2.0f : value;

            if (this.audioDeviceChanging)
            {
                return;
            }

            this.alInvoker.Source(this.srcId, ALSourcef.Pitch, value);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="Audio"/> has been disposed.</exception>
    public void Play()
    {
        if (this.isDisposed)
        {
            throw new InvalidOperationException(IsDisposedExceptionMessage);
        }

        if (this.audioDeviceChanging)
        {
            return;
        }

        var cmd = new AudioCommandData
        {
            SourceId = this.srcId,
            Command = AudioCommands.Play,
        };
        this.audioCommandReactable.Push(PushNotifications.SendAudioCmd, cmd);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="Audio"/> has been disposed.</exception>
    public void Pause()
    {
        if (this.isDisposed)
        {
            throw new InvalidOperationException(IsDisposedExceptionMessage);
        }

        if (this.audioDeviceChanging)
        {
            return;
        }

        var cmd = new AudioCommandData
        {
            SourceId = this.srcId,
            Command = AudioCommands.Pause,
        };
        this.audioCommandReactable.Push(PushNotifications.SendAudioCmd, cmd);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="Audio"/> has been disposed.</exception>
    public void Reset()
    {
        if (this.isDisposed)
        {
            throw new InvalidOperationException(IsDisposedExceptionMessage);
        }

        if (this.audioDeviceChanging)
        {
            return;
        }

        var cmd = new AudioCommandData
        {
            SourceId = this.srcId,
            Command = AudioCommands.Reset,
        };
        this.audioCommandReactable.Push(PushNotifications.SendAudioCmd, cmd);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="Audio"/> has been disposed.</exception>
    public void SetTimePosition(float seconds)
    {
        if (this.isDisposed)
        {
            throw new InvalidOperationException(IsDisposedExceptionMessage);
        }

        if (this.audioDeviceChanging)
        {
            return;
        }

        // Prevent negative numbers
        seconds = seconds < 0f ? 0.0f : seconds;

        var isPastEnd = seconds > this.audioBuffer.TotalSeconds;

        // Prevent a value past the end of the sound
        seconds = isPastEnd ? (float)Math.Floor(this.audioBuffer.TotalSeconds) : seconds;

        var cmd = new PosCommandData { SourceId = this.srcId, PositionSeconds = seconds };
        this.posCommandReactable.Push(PushNotifications.UpdateSoundPos, cmd);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="Audio"/> has been disposed.</exception>
    public void Rewind(float seconds) => SetTimePosition(Position.TotalSeconds - seconds);

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="Audio"/> has been disposed.</exception>
    public void FastForward(float seconds) => SetTimePosition(Position.TotalSeconds + seconds);

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

        if (State == SoundState.Playing)
        {
            this.alInvoker.SourceStop(this.srcId);
        }

        if (disposing)
        {
            this.audioBuffer.Dispose();

            this.audioManager.DeviceChanging -= AudioManager_DeviceChanging;
            this.audioManager.DeviceChanged -= AudioManager_DeviceChanged;
        }

        this.alInvoker.DeleteSource(this.srcId);
        this.alInvoker.ErrorCallback -= ErrorCallback;

        this.isDisposed = true;
    }

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
        this.srcId = this.audioBuffer.Init(FilePath);
        this.audioBuffer.Upload();
    }

    /// <summary>
    /// Saves various states of the sound to be reapplied after the device has
    /// been changed.
    /// </summary>
    private void AudioManager_DeviceChanging(object? sender, EventArgs e)
    {
        this.stateBeforeDeviceChange = this.alInvoker.GetSourceState(this.srcId);
        this.volumeBeforeDeviceChange = Volume;
        this.posBeforeDeviceChange = Position;
        this.playSpeedBeforeDeviceChange = PlaySpeed;

        this.audioDeviceChanging = true;

        if (this.stateBeforeDeviceChange != ALSourceState.Stopped)
        {
            this.alInvoker.SourceStop(this.srcId);
        }

        this.audioBuffer.RemoveBuffer();
        this.alInvoker.DeleteSource(this.srcId);
        this.srcId = 0;
    }

    /// <summary>
    /// Reapplies the state of the sound after the audio device has been changed.
    /// </summary>
    private void AudioManager_DeviceChanged(object? sender, EventArgs e)
    {
        Init();

        this.audioDeviceChanging = false;

        if (this.volumeBeforeDeviceChange >= 0)
        {
            Volume = this.volumeBeforeDeviceChange;
            this.volumeBeforeDeviceChange = -1;
        }

        if (this.posBeforeDeviceChange != default)
        {
            SetTimePosition(this.posBeforeDeviceChange.TotalSeconds);
            this.posBeforeDeviceChange = default;
        }

        if (this.playSpeedBeforeDeviceChange >= 0)
        {
            PlaySpeed = this.playSpeedBeforeDeviceChange;
            this.playSpeedBeforeDeviceChange = -1;
        }

        if (this.stateBeforeDeviceChange == ALSourceState.Playing)
        {
            Play();
        }
    }
}
