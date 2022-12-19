// <copyright file="AudioDeviceManager.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using CASL.Data.Exceptions;
    using CASL.Devices.Exceptions;
    using CASL.Exceptions;
    using CASL.OpenAL;

    /// <summary>
    /// Manages audio devices on the system using OpenAL.
    /// </summary>
    internal class AudioDeviceManager : IAudioDeviceManager
    {
        private const string DeviceNamePrefix = "OpenAL Soft on "; // All device names returned are prefixed with this
        private readonly IOpenALInvoker alInvoker;
        private readonly string isDisposedExceptionMessage = $"The '{nameof(AudioDeviceManager)}' has not been initialized.\nInvoked the '{nameof(AudioDeviceManager.InitDevice)}()' to initialize the device manager.";
        private readonly Dictionary<uint, SoundSource> soundSources = new ();
        private readonly List<SoundStats> continuePlaybackCache = new ();
        private nint device;
        private ALContext context;
        private ALContextAttributes? attributes;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDeviceManager"/> class.
        /// </summary>
        /// <param name="alInvoker">Makes calls to OpenAL.</param>
        public AudioDeviceManager(IOpenALInvoker alInvoker)
        {
            this.alInvoker = alInvoker;
            this.alInvoker.ErrorCallback += ErrorCallback;
        }

        /// <inheritdoc/>
        public event EventHandler<EventArgs>? DeviceChanging;

        /// <inheritdoc/>
        public event EventHandler<EventArgs>? DeviceChanged;

        /// <inheritdoc/>
        public bool IsInitialized => !AudioIsNull() && !(this.alInvoker is null);

        /// <inheritdoc/>
        public string[] DeviceNames
        {
            get
            {
                if (!IsInitialized)
                {
                    throw new AudioDeviceManagerNotInitializedException(this.isDisposedExceptionMessage);
                }

                var result = Array.Empty<string>();

                if (this.alInvoker is not null)
                {
                    result = this.alInvoker.GetDeviceList()
                        .Select(n => n.Replace(DeviceNamePrefix, string.Empty, StringComparison.Ordinal)).ToArray();
                }

                return result;
            }
        }

        /// <inheritdoc/>
        public string DeviceInUse { get; private set; } = string.Empty;

        /// <inheritdoc/>
        public SoundSource[] SoundSources => this.soundSources.Values.ToArray();

        /// <inheritdoc/>
        public void InitDevice(string? name = null)
        {
            var nameResult = name != null ? $"{DeviceNamePrefix}{name}" : name;
            var setCurrentResult = false;

            if (this.alInvoker is not null)
            {
                if (this.device == 0)
                {
                    this.device = this.alInvoker.OpenDevice(nameResult);
                }

                if (this.attributes is null)
                {
                    this.attributes = new ALContextAttributes();
                }

                if (this.context.Handle == 0)
                {
                    this.context = new ALContext(this.alInvoker.CreateContext(new ALDevice(this.device), this.attributes));
                }

                setCurrentResult = this.alInvoker.MakeContextCurrent(this.context);

                DeviceInUse = this.alInvoker.GetDefaultDevice();
            }

            if (!setCurrentResult)
            {
                throw new InitializeDeviceException();
            }
        }

        /// <inheritdoc/>
        public (uint srcId, uint bufferId) InitSound()
        {
            if (!IsInitialized)
            {
                throw new AudioDeviceManagerNotInitializedException(this.isDisposedExceptionMessage);
            }

            SoundSource soundSrc;
            soundSrc.TotalSeconds = -1f;
            soundSrc.SourceId = 0;

            var bufferId = 0u;

            if (this.alInvoker is not null)
            {
                soundSrc.SourceId = this.alInvoker.GenSource();
                bufferId = this.alInvoker.GenBuffer();
            }

            this.soundSources.Add(soundSrc.SourceId, soundSrc);

            return (soundSrc.SourceId, bufferId);
        }

        /// <inheritdoc/>
        public void ChangeDevice(string name)
        {
            if (!IsInitialized)
            {
                throw new AudioDeviceManagerNotInitializedException(this.isDisposedExceptionMessage);
            }

            if (!DeviceNames.Contains(name))
            {
                throw new AudioDeviceDoesNotExistException("The audio device does not exist.", name);
            }

            this.DeviceChanging?.Invoke(this, EventArgs.Empty);

            var availableDevices = DeviceNames;

            CacheSoundSources();

            DestroyDevice();
            InitDevice(name);

            this.soundSources.Clear();

            this.DeviceChanged?.Invoke(this, EventArgs.Empty);

            // Reset all of the states such as if playing or paused and the current time position
            foreach (var cachedState in this.continuePlaybackCache)
            {
                // Set the current position of the sound
                SetTimePosition(cachedState.SourceId, cachedState.TimePosition, cachedState.TotalSeconds);

                // Set the play speed
                this.alInvoker.Source(cachedState.SourceId, ALSourcef.Pitch, cachedState.PlaySpeed);

                // Set the state of the sound
                if (cachedState.PlaybackState == SoundState.Playing)
                {
                    this.alInvoker.SourcePlay(cachedState.SourceId);
                }
                else if (cachedState.PlaybackState == SoundState.Paused)
                {
                    this.alInvoker.SourceStop(cachedState.SourceId);
                }
            }
        }

        /// <inheritdoc/>
        public void UpdateSoundSource(SoundSource soundSrc)
        {
            if (!IsInitialized)
            {
                throw new AudioDeviceManagerNotInitializedException(this.isDisposedExceptionMessage);
            }

            if (!this.soundSources.Keys.Contains(soundSrc.SourceId))
            {
                throw new SoundDataException($"The sound source with the source id '{soundSrc.SourceId}' does not exist.");
            }

            this.soundSources[soundSrc.SourceId] = soundSrc;
        }

        /// <inheritdoc/>
        public void RemoveSoundSource(uint sourceId)
        {
            if (!IsInitialized)
            {
                throw new AudioDeviceManagerNotInitializedException(this.isDisposedExceptionMessage);
            }

            this.soundSources.Remove(sourceId);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to disose of managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            DestroyDevice();

            if (disposing)
            {
                this.alInvoker.ErrorCallback -= ErrorCallback;
                this.soundSources.Clear();
                this.continuePlaybackCache.Clear();
            }

            this.isDisposed = true;
        }

        /// <summary>
        /// Invoked when there is an OpenAL specific error.
        /// </summary>
        /// <param name="errorMsg">The error message from OpenAL.</param>
        [ExcludeFromCodeCoverage]
        private static void ErrorCallback(string errorMsg) => throw new AudioException(errorMsg);

        /// <summary>
        /// Destroys the current audio device.
        /// </summary>
        /// <remarks>
        ///     To use another audio device, the <see cref="InitDevice(string?)"/>
        ///     will have to be invoked again.
        /// </remarks>
        private void DestroyDevice()
        {
            this.alInvoker.MakeContextCurrent(ALContext.Null());
            this.alInvoker.DestroyContext(this.context);
            this.context = ALContext.Null();

            this.alInvoker.CloseDevice(new ALDevice(this.device));
            this.device = ALDevice.Null();

            this.attributes = null;
        }

        /// <summary>
        /// Caches all of the current sound sources that are currently playing or paused.
        /// </summary>
        /// <remarks>
        ///     These cached sounds sources are the state of the sounds and is used to bring
        ///     the state of the sounds back to where they were before changing to another audio device.
        /// </remarks>
        private void CacheSoundSources()
        {
            // Create a cache of all the songs currently playing and record the current playback position
            // Cache only if the sound was currently playing or paused

            // Guarantee that the cache is clear
            this.continuePlaybackCache.Clear();

            foreach (var soundSrcKVP in this.soundSources)
            {
                var sourceState = this.alInvoker.GetSourceState(soundSrcKVP.Value.SourceId);

                if (sourceState != ALSourceState.Playing && sourceState != ALSourceState.Paused)
                {
                    continue;
                }

                SoundStats soundStats;
                soundStats.SourceId = soundSrcKVP.Value.SourceId;
                soundStats.PlaybackState = default;
                soundStats.TimePosition = GetCurrentTimePosition(soundSrcKVP.Value.SourceId);
                soundStats.TotalSeconds = soundSrcKVP.Value.TotalSeconds;
                soundStats.PlaySpeed = this.alInvoker.GetSource(soundSrcKVP.Value.SourceId, ALSourcef.Pitch);

                if (sourceState == ALSourceState.Playing)
                {
                    soundStats.PlaybackState = SoundState.Playing;
                }
                else if (sourceState == ALSourceState.Paused)
                {
                    soundStats.PlaybackState = SoundState.Paused;
                }

                this.continuePlaybackCache.Add(soundStats);
            }
        }

        /// <summary>
        /// Returns a value indicating if the audio device and context are null.
        /// </summary>
        /// <returns><see langword="true"/> if the device and context are null.</returns>
        private bool AudioIsNull() => this.device == ALDevice.Null() && this.context == ALContext.Null() && this.attributes is null;

        /// <summary>
        /// Gets the current position of the sound in the value of seconds.
        /// </summary>
        /// <param name="srcId">The OpenAL source id.</param>
        /// <returns>The position in seconds.</returns>
        private float GetCurrentTimePosition(uint srcId) => this.alInvoker.GetSource(srcId, ALSourcef.SecOffset);

        /// <summary>
        /// Sets the time position of the sound to the given <paramref name="seconds"/> value.
        /// </summary>
        /// <param name="srcId">The OpenAL source id.</param>
        /// <param name="seconds">The position in seconds.</param>
        /// <param name="totalSeconds">The total seconds of the sound.</param>
        /// <remarks>
        ///     If the <paramref name="seconds"/> value is negative,
        ///     it will be treated as positive.
        /// </remarks>
        private void SetTimePosition(uint srcId, float seconds, float totalSeconds)
        {
            // Prevent negative number
            seconds = Math.Abs(seconds);

            seconds = seconds > totalSeconds ? totalSeconds : seconds;

            this.alInvoker.Source(srcId, ALSourcef.SecOffset, seconds);
        }
    }
}
