// <copyright file="SoundTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

#pragma warning disable IDE0002 // Name can be simplified
namespace CASLTests
{
    using System;
    using System.Collections.ObjectModel;
    using CASL;
    using CASL.Data;
    using CASL.Data.Exceptions;
    using CASL.Devices;
    using CASL.OpenAL;
    using Moq;
    using Xunit;
    using Assert = CASLTests.Helpers.AssertExtensions;

    /// <summary>
    /// Tests the <see cref="Sound"/> class.
    /// </summary>
    public class SoundTests
    {
        private readonly Mock<IAudioDeviceManager> mockAudioManager;
        private readonly Mock<ISoundDecoder<float>> mockOggDecoder;
        private readonly Mock<ISoundDecoder<byte>> mockMp3Decoder;
        private readonly Mock<IOpenALInvoker> mockALInvoker;
        private readonly string soundFileNameWithoutExtension = "sound";
        private readonly string oggContentFilePath;
        private readonly string mp3ContentFilePath;
        private readonly float[] oggBufferData = new float[] { 11f, 22f, 33f, 44f };
        private readonly uint srcId = 1234;
        private readonly uint bufferId = 5678;
        private Sound? sound;

        /// <summary>
        /// Initializes a new instance of the <see cref="SoundTests"/> class.
        /// </summary>
        public SoundTests()
        {
            this.oggContentFilePath = @$"C:\temp\Content\Sounds\{this.soundFileNameWithoutExtension}.ogg";
            this.mp3ContentFilePath = @$"C:\temp\Content\Sounds\{this.soundFileNameWithoutExtension}.mp3";

            this.mockALInvoker = new Mock<IOpenALInvoker>();
            this.mockALInvoker.Setup(m => m.GenSource()).Returns(this.srcId);
            this.mockALInvoker.Setup(m => m.GenBuffer()).Returns(this.bufferId);

            MockSoundLength(266);

            this.mockAudioManager = new Mock<IAudioDeviceManager>();
            this.mockAudioManager.Setup(m => m.InitSound()).Returns((this.srcId, this.bufferId));

            this.mockOggDecoder = new Mock<ISoundDecoder<float>>();
            this.mockOggDecoder.Setup(m => m.LoadData(this.oggContentFilePath)).Returns(() =>
            {
                var result = new SoundData<float>()
                {
                    BufferData = new ReadOnlyCollection<float>(this.oggBufferData),
                    Channels = 2,
                    Format = AudioFormat.Stereo16,
                    SampleRate = 44100,
                };

                return result;
            });

            this.mockMp3Decoder = new Mock<ISoundDecoder<byte>>();
        }

        #region Constructor Tests
        [Fact]
        public void Ctor_WhenInvoking_SubscribesToDeviceChangedEvent()
        {
            // Act
            this.sound = CreateSound(this.oggContentFilePath);

            // Assert
            this.mockAudioManager.VerifyAdd(m => m.DeviceChanged += It.IsAny<EventHandler<EventArgs>>(),
                Times.Once(),
                $"Subscription to the event '{nameof(IAudioDeviceManager.DeviceChanged)}' event did not occur.");
        }

        [Fact]
        public void Ctor_WithNoBufferData_ThrowsException()
        {
            // Act
            this.mockOggDecoder.Setup(m => m.LoadData(this.oggContentFilePath))
                .Returns(() =>
                {
                    var result = default(SoundData<float>);
                    result.BufferData = new ReadOnlyCollection<float>(Array.Empty<float>());

                    return result;
                });

            // Act & Assert
            Assert.ThrowsWithMessage<SoundDataException>(() =>
            {
                this.sound = CreateSound(this.oggContentFilePath);
            }, "No audio data exists.");
        }

        [Theory]
        [InlineData(AudioFormat.Mono8, (int)ALFormat.Mono8)]
        [InlineData(AudioFormat.Mono16, (int)ALFormat.Mono16)]
        [InlineData(AudioFormat.MonoFloat32, (int)ALFormat.MonoFloat32Ext)]
        [InlineData(AudioFormat.Stereo8, (int)ALFormat.Stereo8)]
        [InlineData(AudioFormat.Stereo16, (int)ALFormat.Stereo16)]
        [InlineData(AudioFormat.StereoFloat32, (int)ALFormat.StereoFloat32Ext)]
        public void Ctor_WhenUsingOggSound_UploadsBufferData(AudioFormat format, int expected)
        {
            // NOTE: The ALFormat enum values are casted to ints because
            // ALFormat is internal and cannot be used as a param of the unit test method

            // Act
            this.mockOggDecoder.Setup(m => m.LoadData(this.oggContentFilePath))
                .Returns(() =>
                {
                    var result = default(SoundData<float>);
                    result.Format = format;
                    result.Channels = 2;
                    result.SampleRate = 44100;
                    result.BufferData = new ReadOnlyCollection<float>(new[] { 1f, 2f });

                    return result;
                });
            this.sound = CreateSound(this.oggContentFilePath);

            // Assert
            this.mockOggDecoder.Verify(m => m.LoadData(this.oggContentFilePath), Times.Once());
            this.mockALInvoker.Verify(m => m.BufferData(this.bufferId, (ALFormat)expected, new[] { 1f, 2f }, 44100), Times.Once());
        }

        [Fact]
        public void Ctor_WhenUsingUnknownFormat_ThrowsException()
        {
            // Act
            this.mockOggDecoder.Setup(m => m.LoadData(this.oggContentFilePath))
                .Returns(() =>
                {
                    var result = default(SoundData<float>);
                    result.Format = default;
                    result.Channels = 2;
                    result.SampleRate = 44100;
                    result.BufferData = new ReadOnlyCollection<float>(new[] { 1f, 2f });

                    return result;
                });

            // Act & Assert
            Assert.ThrowsWithMessage<Exception>(() =>
            {
                this.sound = CreateSound(this.oggContentFilePath);
            }, "Invalid or unknown audio format.");
        }

        [Fact]
        public void Ctor_WhenUsingMp3Sound_UploadsBufferData()
        {
            // Act
            this.mockMp3Decoder.Setup(m => m.LoadData(this.mp3ContentFilePath))
                .Returns(() =>
                {
                    var result = default(SoundData<byte>);
                    result.BufferData = new ReadOnlyCollection<byte>(new byte[] { 1, 2 });
                    result.Format = AudioFormat.Stereo16;
                    result.Channels = 2;
                    result.SampleRate = 44100;

                    return result;
                });
            this.sound = CreateSound(this.mp3ContentFilePath);

            // Assert
            this.mockMp3Decoder.Verify(m => m.LoadData(this.mp3ContentFilePath), Times.Once());
            this.mockALInvoker.Verify(m => m.BufferData(this.bufferId, ALFormat.Stereo16, new byte[] { 1, 2 }, 44100), Times.Once());
        }

        [Fact]
        public void Ctor_WhenUsingUnsupportedFileType_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsWithMessage<Exception>(() =>
            {
                this.sound = new Sound(@"C:\temp\Content\Sounds\sound.wav", this.mockALInvoker.Object, this.mockAudioManager.Object, this.mockOggDecoder.Object, this.mockMp3Decoder.Object);
            }, "The file extension '.wav' is not supported file type.");
        }
        #endregion

        #region Prop Tests
        [Fact]
        public void ContentName_WhenGettingValue_ReturnsCorrectResult()
        {
            // Act
            this.sound = CreateSound(this.oggContentFilePath);

            // Assert
            Assert.Equal("sound", this.sound.Name);
        }

        [Fact]
        public void IsLooping_WhenGettingValueWhileDisposed_ThrowsException()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);

            // Act & Assert
            this.sound.Dispose();

            Assert.ThrowsWithMessage<Exception>(() =>
            {
                _ = this.sound.IsLooping;
            }, "The sound is disposed.  You must create another sound instance.");
        }

        [Fact]
        public void IsLooping_WhenGettingValue_GetsSoundLoopingValue()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);

            // Act
            _ = this.sound.IsLooping;

            // Assert
            this.mockALInvoker.Verify(m => m.GetSource(this.srcId, ALSourceb.Looping), Times.Once());
        }

        [Fact]
        public void IsLooping_WhenSettingValueWhileDisposed_ThrowsException()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);

            // Act & Assert
            this.sound.Dispose();

            Assert.ThrowsWithMessage<Exception>(() =>
            {
                this.sound.IsLooping = true;
            }, "The sound is disposed.  You must create another sound instance.");
        }

        [Fact]
        public void IsLooping_WhenSettingValue_SetsSoundLoopingSetting()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);

            // Act
            this.sound.IsLooping = true;

            // Assert
            this.mockALInvoker.Verify(m => m.Source(this.srcId, ALSourceb.Looping, true), Times.Once());
        }

        [Fact]
        public void Volume_WhenGettingValueWhileDisposed_ThrowsException()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);

            // Act & Assert
            this.sound.Dispose();

            Assert.ThrowsWithMessage<Exception>(() =>
            {
                _ = this.sound.Volume;
            }, "The sound is disposed.  You must create another sound instance.");
        }

        [Fact]
        public void Volume_WhenGettingValue_GetsSoundVolume()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);

            // Act
            _ = this.sound.Volume;

            // Assert
            this.mockALInvoker.Verify(m => m.GetSource(this.srcId, ALSourcef.Gain), Times.Once());
        }

        [Fact]
        public void Volume_WhenSettingValueWhileDisposed_ThrowsException()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);

            // Act & Assert
            this.sound.Dispose();

            Assert.ThrowsWithMessage<Exception>(() =>
            {
                this.sound.Volume = 0.5f;
            }, "The sound is disposed.  You must create another sound instance.");
        }

        [Theory]
        [InlineData(0.5f, 0.00499999989f)]
        [InlineData(50f, 0.5f)]
        [InlineData(-5f, 0f)]
        [InlineData(142f, 1f)]
        public void Volume_WhenSettingValue_SetsSoundVolume(float volume, float expected)
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);

            // Act
            this.sound.Volume = volume;

            // Assert
            this.mockALInvoker.Verify(m => m.Source(this.srcId, ALSourcef.Gain, expected), Times.Once());
        }

        [Fact]
        public void Position_WhenGettingValue_ReturnsCorrectResult()
        {
            // Arrange
            var expected = new SoundTime(90);
            this.mockALInvoker.Setup(m => m.GetSource(this.srcId, ALSourcef.SecOffset)).Returns(90f);
            this.sound = CreateSound(this.oggContentFilePath);

            // Act
            var actual = this.sound.Position;

            // Assert
            this.mockALInvoker.Verify(m => m.GetSource(this.srcId, ALSourcef.SecOffset), Times.Once());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void State_WhenGettingValueWhileUnloaded_ThrowsException()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);
            this.sound.Dispose();

            // Act & Assert
            Assert.ThrowsWithMessage<InvalidOperationException>(() =>
            {
                _ = this.sound.State;
            }, "The sound is disposed.  You must create another sound instance.");
        }
        #endregion

        #region Method Tests
        [Fact]
        public void Play_WhenDisposed_ThrowsException()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);

            // Act & Assert
            this.sound.Dispose();

            Assert.ThrowsWithMessage<Exception>(() =>
            {
                this.sound.Play();
            }, "The sound is disposed.  You must create another sound instance.");
        }

        [Fact]
        public void Play_WhenInvoked_PlaysSound()
        {
            // Arrange
            this.mockALInvoker.Setup(m => m.GetSourceState(this.srcId)).Returns(ALSourceState.Stopped);
            this.sound = CreateSound(this.oggContentFilePath);

            // Act
            this.sound.Play();

            // Assert
            this.mockALInvoker.Verify(m => m.SourcePlay(this.srcId), Times.Once());
        }

        [Fact]
        public void Pause_WhenDisposed_ThrowsException()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);
            this.sound.Dispose();

            // Act & Assert
            Assert.ThrowsWithMessage<Exception>(() =>
            {
                this.sound.Pause();
            }, "The sound is disposed.  You must create another sound instance.");
        }

        [Fact]
        public void Pause_WhenInvoked_PausesSound()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);

            // Act
            this.sound.Pause();

            // Assert
            this.mockALInvoker.Verify(m => m.SourcePause(this.srcId), Times.Once());
        }

        [Fact]
        public void Stop_WhenDisposed_ThrowsException()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);

            // Act & Assert
            this.sound.Dispose();

            Assert.ThrowsWithMessage<Exception>(() =>
            {
                this.sound.Stop();
            }, "The sound is disposed.  You must create another sound instance.");
        }

        [Fact]
        public void Stop_WhenInvoked_StopsSound()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);

            // Act
            this.sound.Stop();

            // Assert
            this.mockALInvoker.Verify(m => m.SourceStop(this.srcId), Times.Once());
        }

        [Fact]
        public void Reset_WhenDisposed_ThrowsException()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);

            // Act & Assert
            this.sound.Dispose();

            Assert.ThrowsWithMessage<Exception>(() =>
            {
                this.sound.Reset();
            }, "The sound is disposed.  You must create another sound instance.");
        }

        [Fact]
        public void Reset_WhenInvoked_ResetsSound()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);

            // Act
            this.sound.Reset();

            // Assert
            this.mockALInvoker.Verify(m => m.SourceRewind(this.srcId), Times.Once());
        }

        [Fact]
        public void SetTimePosition_WhenDisposed_ThrowsException()
        {
            // Arrange
            this.sound = CreateSound(this.oggContentFilePath);

            // Act & Assert
            this.sound.Dispose();

            Assert.ThrowsWithMessage<Exception>(() =>
            {
                this.sound.SetTimePosition(5);
            }, "The sound is disposed.  You must create another sound instance.");
        }

        [Theory]
        [InlineData(10f, 10f)]
        [InlineData(-2f, 0f)]
        [InlineData(300f, 50f)]
        public void SetTimePosition_WithInvoked_SetsTimePosition(float seconds, float expected)
        {
            // Arrange
            MockSoundLength(50);
            this.mockOggDecoder.Setup(m => m.LoadData(this.oggContentFilePath))
                .Returns(() =>
                {
                    var result = default(SoundData<float>);
                    result.Format = AudioFormat.Stereo16;
                    result.Channels = 2;
                    result.SampleRate = 441000;
                    result.BufferData = new ReadOnlyCollection<float>(new[] { 1f, 2f });

                    return result;
                });
            this.sound = CreateSound(this.oggContentFilePath);

            // Act
            this.sound.SetTimePosition(seconds);

            // Assert
            this.mockALInvoker.Verify(m => m.Source(this.srcId, ALSourcef.SecOffset, expected), Times.Once());
        }

        [Fact]
        public void Rewind_WhenTimeIsPastBeginingOfSound_ResetsAndPlaysSound()
        {
            // Arrange
            var sound = CreateSound(this.oggContentFilePath);
            this.mockALInvoker.Setup(m => m.GetSourceState(this.srcId)).Returns(ALSourceState.Stopped);
            this.mockALInvoker.Setup(m => m.GetSource(this.srcId, ALSourcef.SecOffset)).Returns(10f);

            // Act
            sound.Rewind(20f);

            // Assert
            this.mockALInvoker.Verify(m => m.SourceRewind(this.srcId), Times.Once());
            this.mockALInvoker.Verify(m => m.SourcePlay(this.srcId), Times.Once());
            this.mockALInvoker.Verify(m => m.Source(It.IsAny<uint>(), It.IsAny<ALSourcef>(), It.IsAny<float>()), Times.Never());
        }

        [Fact]
        public void Rewind_WhenRewinding10Seconds_Rewinds10Seconds()
        {
            // Arrange
            MockSoundLength(25);
            this.mockALInvoker.Setup(m => m.GetSource(this.srcId, ALSourcef.SecOffset))
                .Returns(15f);
            var sound = CreateSound(this.oggContentFilePath);

            // Act
            sound.Rewind(10f);

            // Assert
            this.mockALInvoker.Verify(m => m.Source(this.srcId, ALSourcef.SecOffset, 5f), Times.Once());
        }

        [Fact]
        public void FastForward_WhenTimeIsPastEndOfSound_ResetsSound()
        {
            // Arrange
            MockSoundLength(10);
            this.mockALInvoker.Setup(m => m.GetSource(this.srcId, ALSourcef.SecOffset)).Returns(10f);

            var sound = CreateSound(this.oggContentFilePath);

            // Act
            sound.FastForward(20f);

            // Assert
            this.mockALInvoker.Verify(m => m.SourceRewind(this.srcId), Times.Once());
            this.mockALInvoker.Verify(m => m.Source(It.IsAny<uint>(), It.IsAny<ALSourcef>(), It.IsAny<float>()), Times.Never());
        }

        [Fact]
        public void FastForward_WhenFastForwarding10Seconds_Forwards10Seconds()
        {
            // Arrange
            MockSoundLength(60);
            this.mockOggDecoder.Setup(m => m.LoadData(this.oggContentFilePath)).Returns(() =>
            {
                var result = new SoundData<float>()
                {
                    BufferData = new ReadOnlyCollection<float>(this.oggBufferData),
                    Channels = 2,
                    Format = AudioFormat.Stereo16,
                    SampleRate = 44100,
                };

                return result;
            });

            this.mockALInvoker.Setup(m => m.GetSource(this.srcId, ALSourcef.SecOffset)).Returns(30f);
            var sound = CreateSound(this.oggContentFilePath);

            // Act
            sound.FastForward(10f);

            // Assert
            this.mockALInvoker.Verify(m => m.Source(this.srcId, ALSourcef.SecOffset, 40f), Times.Once());
        }

        [Fact]
        public void Sound_WhenChangingAudoDevice_ReinitializesSound()
        {
            // Arrange
            // Simulate an audio device change so the event is invoked inside of the sound class
            this.mockAudioManager.Setup(m => m.ChangeDevice(It.IsAny<string>()))
                .Callback<string>((name) =>
                {
                    this.mockAudioManager.Raise(manager => manager.DeviceChanged += (sender, e) => { }, EventArgs.Empty);
                });

            this.mockOggDecoder.Setup(m => m.LoadData(this.oggContentFilePath))
                .Returns(() =>
                {
                    var result = default(SoundData<float>);
                    result.Format = AudioFormat.Stereo16;
                    result.Channels = 2;
                    result.SampleRate = 44100;
                    result.BufferData = new ReadOnlyCollection<float>(new[] { 1f, 2f });

                    return result;
                });
            this.sound = CreateSound(this.oggContentFilePath);

            // Act
            this.mockAudioManager.Object.ChangeDevice(It.IsAny<string>());

            // Assert
            // NOTE: The first invoke is during Sound construction, the second is when changing audio devices
            this.mockOggDecoder.Verify(m => m.LoadData(this.oggContentFilePath), Times.Exactly(2));
            this.mockALInvoker.Verify(m => m.BufferData(this.bufferId, ALFormat.Stereo16, new[] { 1f, 2f }, 44100), Times.Exactly(2));
        }

        [Fact]
        public void Dispose_WhenInvoked_DisposesOfSound()
        {
            // Arrange
            var sound = CreateSound(this.oggContentFilePath);

            // Act
            sound.Dispose();
            sound.Dispose();

            // Assert
            this.mockOggDecoder.Verify(m => m.Dispose(), Times.Once());
            this.mockMp3Decoder.Verify(m => m.Dispose(), Times.Once());
            this.mockAudioManager.VerifyRemove(m => m.DeviceChanged -= It.IsAny<EventHandler<EventArgs>>(),
                Times.Once(),
                $"Unsubscription to the event '{nameof(IAudioDeviceManager.DeviceChanged)}' event did not occur.");
            this.mockALInvoker.VerifyRemove(m => m.ErrorCallback -= It.IsAny<Action<string>>(),
                Times.Once(),
                $"Unsubscription to the event '{nameof(IOpenALInvoker.ErrorCallback)}' event did not occur.");
        }

        [Fact]
        public void Dispose_WithInvalidSourceID_DoesNotAttemptSourceAndBufferDeletion()
        {
            // Arrange
            this.mockAudioManager.Setup(m => m.InitSound()).Returns((0u, this.bufferId));
            var sound = CreateSound(this.oggContentFilePath);

            // Act
            sound.Dispose();
            sound.Dispose();

            // Assert
            this.mockALInvoker.Verify(m => m.DeleteSource(this.srcId), Times.Never());
        }
        #endregion

        /// <summary>
        /// Creates an instance of <see cref="Sound"/> for testing.
        /// </summary>
        /// <param name="filePath">The path to the sound file.</param>
        /// <returns>The instance for testing.</returns>
        private Sound CreateSound(string filePath)
            => new (filePath, this.mockALInvoker.Object, this.mockAudioManager.Object, this.mockOggDecoder.Object, this.mockMp3Decoder.Object);

        /// <summary>
        /// Mocks the buffer data stats to influence the total seconds that the sound has.
        /// </summary>
        /// <param name="channels">The total number of seconds to simulate.</param>
        private void MockSoundLength(float totalSeconds)
        {
            /* This is the total seconds for every byte of data
             * based on 2 Channels, 32 bit depth and a frequency of 44100
             */
            const int bytesPerSec = 352801; // Conversion factor
            const int channels = 2;
            const int bitDepth = 32;
            const int freq = 44100;

            var size = (int)(totalSeconds * bytesPerSec);

            this.mockALInvoker.Setup(m => m.GetBuffer(this.bufferId, ALGetBufferi.Size)).Returns(size);
            this.mockALInvoker.Setup(m => m.GetBuffer(this.bufferId, ALGetBufferi.Channels)).Returns(channels);
            this.mockALInvoker.Setup(m => m.GetBuffer(this.bufferId, ALGetBufferi.Bits)).Returns(bitDepth);
            this.mockALInvoker.Setup(m => m.GetBuffer(this.bufferId, ALGetBufferi.Frequency)).Returns(freq);
        }
    }
}
