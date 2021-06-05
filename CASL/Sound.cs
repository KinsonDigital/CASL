// <copyright file="Sound.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using CASL.Data;
    using CASL.Data.Exceptions;
    using CASL.Devices;
    using CASL.Devices.Factories;
    using CASL.OpenAL;
    using IOPath = System.IO.Path;

    /// <summary>
    /// A single sound that can be played, paused etc.
    /// </summary>
    public class Sound : ISound
    {
        private const string IsDisposedExceptionMessage = "The sound is disposed.  You must create another sound instance.";

        // NOTE: This warning is ignored due to the implementation of the IAudioManager being a singleton.
        // This AudioManager implementation as a singleton is being managed by the IoC container class.
        // Disposing of the audio manager when any sound is disposed would cause issues with how the
        // audio manager implementation is suppose to behave.
        private readonly IAudioDeviceManager audioManager;
        private readonly ISoundDecoder<float> oggDecoder;
        private readonly ISoundDecoder<byte> mp3Decoder;
        private readonly IOpenALInvoker alInvoker;
        private uint srcId;
        private uint bufferId;
        private bool ignoreOpenALCalls;
        private float totalSeconds;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sound"/> class.
        /// </summary>
        /// <param name="filePath">The path to the sound file..</param>
        [ExcludeFromCodeCoverage]
        public Sound(string filePath)
        {
            Path = filePath;

            this.alInvoker = IoC.Container.GetInstance<IOpenALInvoker>();
            this.alInvoker.ErrorCallback += ErrorCallback;

            this.oggDecoder = IoC.Container.GetInstance<ISoundDecoder<float>>();

            this.mp3Decoder = IoC.Container.GetInstance<ISoundDecoder<byte>>();
            this.audioManager = AudioDeviceManagerFactory.CreateDeviceManager();
            this.audioManager.DeviceChanging += AudioManager_DeviceChanging;
            this.audioManager.DeviceChanged += AudioManager_DeviceChanged;
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
        /// <param name="soundPathResolver">Resolves paths to sound content.</param>
        internal Sound(string filePath, IOpenALInvoker alInvoker, IAudioDeviceManager audioManager, ISoundDecoder<float> oggDecoder, ISoundDecoder<byte> mp3Decoder)
        {
            Path = filePath;

            this.alInvoker = alInvoker;
            this.alInvoker.ErrorCallback += ErrorCallback;

            this.oggDecoder = oggDecoder;

            this.mp3Decoder = mp3Decoder;
            this.audioManager = audioManager;
            this.audioManager.DeviceChanged += AudioManager_DeviceChanged;

            Init();
        }

        /// <inheritdoc/>
        public string Name => IOPath.GetFileNameWithoutExtension(Path);

        /// <inheritdoc/>
        public string Path { get; private set; }

        /// <inheritdoc/>
        public float Volume
        {
            get
            {
                if (Unloaded)
                {
                    throw new Exception(IsDisposedExceptionMessage);
                }

                // Get the current volume between 0.0 and 1.0
                return this.ignoreOpenALCalls
                    ? 0
                    : this.alInvoker.GetSource(this.srcId, ALSourcef.Gain) * 100f;
            }
            set
            {
                if (Unloaded)
                {
                    throw new Exception(IsDisposedExceptionMessage);
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
                if (Unloaded)
                {
                    throw new Exception(IsDisposedExceptionMessage);
                }

                var seconds = this.ignoreOpenALCalls ? 0f : this.alInvoker.GetSource(this.srcId, ALSourcef.SecOffset);

                return new SoundTime(seconds);
            }
        }

        /// <inheritdoc/>
        public SoundTime Length => new SoundTime(this.totalSeconds);

        /// <inheritdoc/>
        public bool IsLooping
        {
            get
            {
                if (Unloaded)
                {
                    throw new Exception(IsDisposedExceptionMessage);
                }

                return this.ignoreOpenALCalls
                    ? false
                    : this.alInvoker.GetSource(this.srcId, ALSourceb.Looping);
            }
            set
            {
                if (Unloaded)
                {
                    throw new Exception(IsDisposedExceptionMessage);
                }

                if (this.ignoreOpenALCalls)
                {
                    return;
                }

                this.alInvoker.Source(this.srcId, ALSourceb.Looping, value);
            }
        }

        /// <inheritdoc/>
        public bool Unloaded { get; private set; }

        /// <inheritdoc/>
        public SoundState State
        {
            get
            {
                if (Unloaded)
                {
                    throw new Exception(IsDisposedExceptionMessage);
                }

                var currentState = this.alInvoker.GetSourceState(this.srcId);

                return this.ignoreOpenALCalls
                    ? this.alInvoker.GetSourceState(this.srcId) switch
                    {
                        ALSourceState.Playing => SoundState.Playing,
                        ALSourceState.Paused => SoundState.Paused,
                        ALSourceState.Stopped => SoundState.Stopped,
                        ALSourceState.Initial => SoundState.Stopped,
                        _ => throw new Exception($"The OpenAL sound state of '{nameof(ALSourceState)}: {(int)currentState}' not valid."),
                    }
                    : SoundState.Stopped;
            }
        }

        /// <inheritdoc/>
        public float PlaySpeed
        {
            get
            {
                if (Unloaded)
                {
                    throw new Exception(IsDisposedExceptionMessage);
                }

                return this.ignoreOpenALCalls
                    ? 0f
                    : this.alInvoker.GetSource(this.srcId, ALSourcef.Gain);
            }
            set
            {
                value = value < 0f ? 0.25f : value;
                value = value > 2.0f ? 2.0f : value;

                this.alInvoker.Source(this.srcId, ALSourcef.Pitch, value);
            }
        }

        /// <inheritdoc/>
        public void Play()
        {
            if (Unloaded)
            {
                throw new Exception(IsDisposedExceptionMessage);
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
            if (Unloaded)
            {
                throw new Exception(IsDisposedExceptionMessage);
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
            if (Unloaded)
            {
                throw new Exception(IsDisposedExceptionMessage);
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
            if (Unloaded)
            {
                throw new Exception(IsDisposedExceptionMessage);
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
            if (Unloaded)
            {
                throw new Exception(IsDisposedExceptionMessage);
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
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to dispose of managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!Unloaded)
            {
                if (disposing)
                {
                    this.oggDecoder.Dispose();
                    this.mp3Decoder.Dispose();
                    this.audioManager.DeviceChanging -= AudioManager_DeviceChanging;
                    this.audioManager.DeviceChanged -= AudioManager_DeviceChanged;
                }

                UnloadSoundData();

                this.alInvoker.ErrorCallback -= ErrorCallback;

                Unloaded = true;
            }
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
            _ => throw new Exception("Invalid or unknown audio format."),
        };

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

            var extension = IOPath.GetExtension(Path);

            SoundSource soundSrc;

            switch (extension)
            {
                case ".ogg":
                    var oggData = this.oggDecoder.LoadData(Path);

                    UploadOggData(oggData);

                    break;
                case ".mp3":
                    var mp3Data = this.mp3Decoder.LoadData(Path);

                    UploadMp3Data(mp3Data);

                    break;
                default:
                    throw new Exception($"The file extension '{extension}' is not supported file type.");
            }

            var sizeInBytes = this.alInvoker.GetBuffer(this.bufferId, ALGetBufferi.Size);
            var totalChannels = this.alInvoker.GetBuffer(this.bufferId, ALGetBufferi.Channels);
            var bitDepth = this.alInvoker.GetBuffer(this.bufferId, ALGetBufferi.Bits);
            var frequency = this.alInvoker.GetBuffer(this.bufferId, ALGetBufferi.Frequency);

            var sampleLen = sizeInBytes * 8 / (totalChannels * bitDepth);

            this.totalSeconds = sampleLen / frequency;

            // TODO: Look into removing SoundData.SampleRate.  This is freq and is now done using OpenAL
            soundSrc.SourceId = this.srcId;
            soundSrc.TotalSeconds = this.totalSeconds;
            soundSrc.SampleRate = frequency;

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

        /// <summary>
        /// The callback invoked when an OpenAL error occurs.
        /// </summary>
        /// <param name="errorMsg">The OpenAL message.</param>
        [ExcludeFromCodeCoverage]
        private void ErrorCallback(string errorMsg) => throw new Exception(errorMsg);
    }
}
