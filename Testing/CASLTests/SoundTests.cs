// <copyright file="SoundTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

#pragma warning disable IDE0002 // Name can be simplified

namespace CASLTests;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using CASL;
using CASL.Data;
using CASL.Data.Exceptions;
using CASL.Devices;
using CASL.Exceptions;
using CASL.OpenAL;
using FluentAssertions;
using Moq;
using Xunit;
using Assert = Helpers.AssertExtensions;

/// <summary>
/// Tests the <see cref="Sound"/> class.
/// </summary>
public class SoundTests
{
    private const string OggFileExtension = ".ogg";
    private const string MP3FileExtension = ".mp3";
    private const uint SrcId = 1234;
    private const uint BufferId = 5678;
    private const string SoundFileNameWithoutExtension = "sound";
    private readonly Mock<IAudioDeviceManager> mockAudioManager;
    private readonly Mock<ISoundDecoder<float>> mockOggDecoder;
    private readonly Mock<ISoundDecoder<byte>> mockMp3Decoder;
    private readonly Mock<IOpenALInvoker> mockALInvoker;
    private readonly Mock<IPath> mockPath;
    private readonly string oggContentFilePath;
    private readonly string mp3ContentFilePath;
    private readonly float[] oggBufferData = { 11f, 22f, 33f, 44f };

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundTests"/> class.
    /// </summary>
    [SuppressMessage("csharpsquid", "S1075", Justification = "Only used for testing")]
    public SoundTests()
    {
        const string soundsDirPath = "C:/temp/Content/Sounds";
        this.oggContentFilePath = $"{soundsDirPath}/{SoundFileNameWithoutExtension}{OggFileExtension}";
        this.mp3ContentFilePath = $"{soundsDirPath}/{SoundFileNameWithoutExtension}{MP3FileExtension}";

        this.mockALInvoker = new Mock<IOpenALInvoker>();
        this.mockALInvoker.Setup(m => m.GenSource()).Returns(SrcId);
        this.mockALInvoker.Setup(m => m.GenBuffer()).Returns(BufferId);

        MockSoundLength(266);

        this.mockAudioManager = new Mock<IAudioDeviceManager>();
        this.mockAudioManager.Setup(m => m.InitSound()).Returns((SrcId, BufferId));

        this.mockOggDecoder = new Mock<ISoundDecoder<float>>();
        this.mockOggDecoder.Setup(m => m.LoadData(this.oggContentFilePath)).Returns(() =>
        {
            var result = new SoundData<float>
            {
                BufferData = new ReadOnlyCollection<float>(this.oggBufferData),
                Channels = 2,
                Format = AudioFormat.Stereo16,
                SampleRate = 44100,
            };

            return result;
        });

        this.mockMp3Decoder = new Mock<ISoundDecoder<byte>>();

        this.mockPath = new Mock<IPath>();
        this.mockPath.Setup(m => m.GetExtension(this.oggContentFilePath)).Returns(OggFileExtension);
        this.mockPath.Setup(m => m.GetExtension(this.mp3ContentFilePath)).Returns(MP3FileExtension);
        this.mockPath.Setup(m => m.GetFileNameWithoutExtension(It.IsAny<string?>()))
            .Returns(SoundFileNameWithoutExtension);
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WhenInvoking_SubscribesToDeviceChangedEvent()
    {
        // Act
        _ = CreateSound(this.oggContentFilePath);

        // Assert
        this.mockAudioManager.VerifyAdd(m => m.DeviceChanged += It.IsAny<EventHandler<EventArgs>>(),
            Times.Once(),
            $"Subscription to the event '{nameof(IAudioDeviceManager.DeviceChanged)}' event did not occur.");
    }

    [Fact]
    public void Ctor_WithNoOggBufferData_ThrowsException()
    {
        // Act
        this.mockOggDecoder.Setup(m => m.LoadData(this.oggContentFilePath))
            .Returns(() =>
            {
                var result = default(SoundData<float>);
                result.BufferData = new ReadOnlyCollection<float>(Array.Empty<float>());

                return result;
            });

        // Act
        var act = () => _ = CreateSound(this.oggContentFilePath);

        // Assert
        act.Should().Throw<SoundDataException>()
            .WithMessage("No audio data exists.");
    }

    [Fact]
    public void Ctor_WithNoMP3BufferData_ThrowsException()
    {
        // Act
        this.mockMp3Decoder.Setup(m => m.LoadData(this.mp3ContentFilePath))
            .Returns(() =>
            {
                var result = default(SoundData<byte>);
                result.BufferData = new ReadOnlyCollection<byte>(Array.Empty<byte>());

                return result;
            });

        // Act
        var act = () => _ = CreateSound(this.mp3ContentFilePath);

        // Assert
        act.Should().Throw<SoundDataException>()
            .WithMessage("No audio data exists.");
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

        // Arrange
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

        // Act
        _ = CreateSound(this.oggContentFilePath);

        // Assert
        this.mockOggDecoder.Verify(m => m.LoadData(this.oggContentFilePath), Times.Once());
        this.mockALInvoker.Verify(m => m.BufferData(BufferId, (ALFormat)expected, new[] { 1f, 2f }, 44100), Times.Once());
    }

    [Fact]
    public void Ctor_WhenUsingUnknownFormat_ThrowsException()
    {
        // Arrange
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

        // Act
        var act = () => _ = CreateSound(this.oggContentFilePath);

        act.Should().Throw<AudioException>()
            .WithMessage("Invalid or unknown audio format.");
    }

    [Fact]
    public void Ctor_WhenUsingMp3Sound_UploadsBufferData()
    {
        // Arrange
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

        // Act
        _ = CreateSound(this.mp3ContentFilePath);

        // Assert
        this.mockMp3Decoder.Verify(m => m.LoadData(this.mp3ContentFilePath), Times.Once());
        this.mockALInvoker.Verify(m => m.BufferData(BufferId, ALFormat.Stereo16, new byte[] { 1, 2 }, 44100), Times.Once());
    }

    [Fact]
    public void Ctor_WhenUsingUnsupportedFileType_ThrowsException()
    {
        // Arrange
        this.mockPath.Setup(m => m.GetExtension(It.IsAny<string?>())).Returns(".wav");

        // Act
        var act = () => _ = new Sound(
                @"C:\temp\Content\Sounds\sound.wav",
                this.mockALInvoker.Object,
                this.mockAudioManager.Object,
                this.mockOggDecoder.Object,
                this.mockMp3Decoder.Object,
                this.mockPath.Object);

        // Assert
        act.Should().Throw<AudioException>()
            .WithMessage("The file extension '.wav' is not supported file type.");
    }
    #endregion

    #region Prop Tests
    [Fact]
    public void Name_WhenGettingValue_ReturnsCorrectResult()
    {
        // Act
        var sut = CreateSound(this.oggContentFilePath);

        // Assert
        sut.Name.Should().Be("sound");
    }

    [Fact]
    public void IsLooping_WhenGettingValueWhileDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        sut.Dispose();

        // Act
        var act = () => _ = sut.IsLooping;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The sound is disposed.  You must create another sound instance.");
    }

    [Fact]
    public void IsLooping_WhenGettingValue_GetsSoundLoopingValue()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        _ = sut.IsLooping;

        // Assert
        this.mockALInvoker.Verify(m => m.GetSource(SrcId, ALSourceb.Looping), Times.Once());
    }

    [Fact]
    public void IsLooping_WhenSettingValueWhileSIgnoringOpenALCalls_ReturnsFalse()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);

        // Act
        var actual = sut.IsLooping;

        // Assert
        actual.Should().BeFalse();
        this.mockALInvoker.Verify(m => m.GetSource(It.IsAny<uint>(), It.IsAny<ALSourceb>()), Times.Never());
    }

    [Fact]
    public void IsLooping_WhenSettingValueWhileDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        sut.Dispose();

        // Act
        var act = () => sut.IsLooping = true;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The sound is disposed.  You must create another sound instance.");
    }

    [Fact]
    public void IsLooping_WhenSettingValue_SetsSoundLoopingSetting()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        sut.IsLooping = true;

        // Assert
        this.mockALInvoker.Verify(m => m.Source(SrcId, ALSourceb.Looping, true), Times.Once());
    }

    [Fact]
    public void IsLooping_WhenSettingValueWhileIgnoringOpenALCalls_DoesNotAttemptToLoopSound()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);

        // Act
        sut.IsLooping = true;

        // Assert
        this.mockALInvoker.Verify(m => m.Source(It.IsAny<uint>(), It.IsAny<ALSourceb>(), true), Times.Never());
    }

    [Fact]
    public void Volume_WhenGettingValueWhileDisposed_ThrowsException()
    {
        // Arrange
        var sut  = CreateSound(this.oggContentFilePath);
        sut.Dispose();

        // Act
        var act = () => _ = sut.Volume;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The sound is disposed.  You must create another sound instance.");
    }

    [Fact]
    public void Volume_WhenGettingValue_GetsSoundVolume()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        _ = sut.Volume;

        // Assert
        this.mockALInvoker.Verify(m => m.GetSource(SrcId, ALSourcef.Gain), Times.Once());
    }

    [Fact]
    public void Volume_WhenSettingValueWhileDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        sut.Dispose();

        // Act
        var act = () => sut.Volume = 0.5f;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The sound is disposed.  You must create another sound instance.");
    }

    [Theory]
    [InlineData(0.5f, 0.00499999989f)]
    [InlineData(50f, 0.5f)]
    [InlineData(-5f, 0f)]
    [InlineData(142f, 1f)]
    public void Volume_WhenSettingValue_SetsSoundVolume(float volume, float expected)
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        sut.Volume = volume;

        // Assert
        this.mockALInvoker.Verify(m => m.Source(SrcId, ALSourcef.Gain, expected), Times.Once());
    }

    [Fact]
    public void Volume_WhenGettingValueWhileIgnoringOpenALCalls_ReturnsZero()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);

        // Act
        _ = sut.Volume;

        // Assert
        this.mockALInvoker.Verify(m => m.GetSource(It.IsAny<uint>(), It.IsAny<ALSourcef>()), Times.Never());
    }

    [Fact]
    public void Volume_WhenSettingValueWhileIgnoringOpenALCalls_DoesNotAttemptToSetSoundVolume()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);

        // Act
        sut.Volume = 50f;

        // Assert
        this.mockALInvoker.Verify(m => m.Source(It.IsAny<uint>(), It.IsAny<ALSourcef>(), It.IsAny<float>()), Times.Never());
    }

    [Fact]
    public void Position_WhenGettingValue_ReturnsCorrectResult()
    {
        // Arrange
        var expected = new SoundTime(90);
        this.mockALInvoker.Setup(m => m.GetSource(SrcId, ALSourcef.SecOffset)).Returns(90f);
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        var actual = sut.Position;

        // Assert
        this.mockALInvoker.Verify(m => m.GetSource(SrcId, ALSourcef.SecOffset), Times.Once());
        actual.Should().Be(expected);
    }

    [Fact]
    public void Position_WhenGettingValueWhileUnloaded_ThrowsException()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        sut.Dispose();

        // Act
        var act = () => _ = sut.Position;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The sound is disposed.  You must create another sound instance.");
    }

    [Fact]
    public void Position_WhenGettingValueWhileIgnoringOpenALCalls_ReturnsZero()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);
        var expected = new SoundTime(0);

        // Act
        var actual = sut.Position;

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void Length_WhenGettingValue_ReturnsCorrectResult()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        var expected = new SoundTime(266.00076f);

        // Act
        var actual = sut.Length;

        // Assert
        actual.TotalSeconds.Should().Be(expected.TotalSeconds);
    }

    [Fact]
    public void State_WhenGettingValueWhileUnloaded_ThrowsException()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        sut.Dispose();

        // Act
        var act = () => _ = sut.State;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The sound is disposed.  You must create another sound instance.");
    }

    [Fact]
    public void State_WhenGettingValueWhileIgnoringOpenALCalls_ReturnsAsStopped()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);

        // Act
        var actual = sut.State;

        // Assert
        actual.Should().Be(SoundState.Stopped);
    }

    [Theory]
    [InlineData(0x1011, SoundState.Stopped)]
    [InlineData(0x1012, SoundState.Playing)]
    [InlineData(0x1013, SoundState.Paused)]
    [InlineData(0x1014, SoundState.Stopped)]
    public void State_WhenGettingValue_ReturnsCorrectResult(int openALState, SoundState expected)
    {
        /*NOTE:
         * The first InlineData value must be a raw integer value of the ALSourceState enum.
         * This is because ALSourceState is scoped as internal and we cannot use it as a parameter
         * for the unit test method.  This is illegal. So the value is casted to get around this limitation
         * for testing purposes.
         */

        // Arrange
        this.mockALInvoker.Setup(m => m.GetSourceState(SrcId)).Returns((ALSourceState)openALState);
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        var actual = sut.State;

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void State_WithInvalidOpenALSourceState_ThrowsException()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockALInvoker.Setup(m => m.GetSourceState(SrcId)).Returns(0f);

        // Act
        var act = () => _ = sut.State;

        // Assert
        act.Should().Throw<AudioException>()
            .WithMessage("The OpenAL sound state of 'ALSourceState: 0' is not valid.");
    }

    [Fact]
    public void PlaySpeed_WhenGettingValueWhileUnloaded_ThrowsException()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        sut.Dispose();

        // Act
        var act = () => _ = sut.PlaySpeed;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The sound is disposed.  You must create another sound instance.");
    }

    [Fact]
    public void PlaySpeed_WhenIgnoringOpenALCalls_ReturnsZero()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);

        // Act
        var actual = sut.PlaySpeed;

        // Assert
        actual.Should().Be(0f);
    }

    [Theory]
    [InlineData(1f, 1f)]
    [InlineData(-1f, 0.25f)]
    [InlineData(3f, 2.0f)]
    public void PlaySpeed_WhenSettingValue_ReturnsCorrectResult(float speedValue, float expectedResult)
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        sut.PlaySpeed = speedValue;
        _ = sut.PlaySpeed;

        // Assert
        this.mockALInvoker.Verify(m => m.GetSource(SrcId, ALSourcef.Pitch), Times.Once());
        this.mockALInvoker.Verify(m => m.Source(SrcId, ALSourcef.Pitch, expectedResult), Times.Once());
    }

    [Fact]
    public void PlaySpeed_WhenIgnoringOpenALCalls_DoesNotMakeOpenALCalls()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);

        // Act
        sut.PlaySpeed = 1f;

        // Assert
        this.mockALInvoker.Verify(m => m.Source(It.IsAny<uint>(), It.IsAny<ALSourcef>(), It.IsAny<float>()), Times.Never());
    }
    #endregion

    #region Method Tests
    [Fact]
    public void Play_WhenDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        sut.Dispose();

        // Act
        var act = () => sut.Play();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The sound is disposed.  You must create another sound instance.");
    }

    [Fact]
    public void Play_WhenInvoked_PlaysSound()
    {
        // Arrange
        this.mockALInvoker.Setup(m => m.GetSourceState(SrcId)).Returns(ALSourceState.Stopped);
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        sut.Play();

        // Assert
        this.mockALInvoker.Verify(m => m.SourcePlay(SrcId), Times.Once());
    }

    [Fact]
    public void Play_WhenAlreadyPlaying_DoesNotAttemptToPlaySoundAgain()
    {
        // Arrange
        this.mockALInvoker.Setup(m => m.GetSourceState(SrcId)).Returns(ALSourceState.Playing);
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        sut.Play();

        // Assert
        this.mockALInvoker.Verify(m => m.SourcePlay(It.IsAny<uint>()), Times.Never());
    }

    [Fact]
    public void Play_WhenIgnoringOpenALCalls_DoesNotAttemptToPlaySound()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);

        this.mockALInvoker.Setup(m => m.GetSourceState(SrcId)).Returns(ALSourceState.Playing);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);

        // Act
        sut.Play();

        // Assert
        this.mockALInvoker.Verify(m => m.SourcePlay(It.IsAny<uint>()), Times.Never());
    }

    [Fact]
    public void Pause_WhenDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        sut.Dispose();

        // Act
        var act = () => sut.Pause();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The sound is disposed.  You must create another sound instance.");
    }

    [Fact]
    public void Pause_WhenInvoked_PausesSound()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        sut.Pause();

        // Assert
        this.mockALInvoker.Verify(m => m.SourcePause(SrcId), Times.Once());
    }

    [Fact]
    public void Pause_WhenIgnoringOpenALCalls_DoesNotAttemptToPauseSound()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);

        // Act
        sut.Pause();

        // Assert
        this.mockALInvoker.Verify(m => m.SourcePause(It.IsAny<uint>()), Times.Never());
    }

    [Fact]
    public void Stop_WhenDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        sut.Dispose();

        // Act
        var act = () => sut.Stop();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The sound is disposed.  You must create another sound instance.");
    }

    [Fact]
    public void Stop_WhenInvoked_StopsSound()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        sut.Stop();

        // Assert
        this.mockALInvoker.Verify(m => m.SourceStop(SrcId), Times.Once());
    }

    [Fact]
    public void Stop_WhenIgnoringOpenALCalls_DoesNotAttemptToStopSound()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);

        // Act
        sut.Stop();

        // Assert
        this.mockALInvoker.Verify(m => m.SourceStop(It.IsAny<uint>()), Times.Never());
    }

    [Fact]
    public void Reset_WhenDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        sut.Dispose();

        // Act
        var act = () => sut.Reset();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The sound is disposed.  You must create another sound instance.");
    }

    [Fact]
    public void Reset_WhenInvoked_ResetsSound()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        sut.Reset();

        // Assert
        this.mockALInvoker.Verify(m => m.SourceRewind(SrcId), Times.Once());
    }

    [Fact]
    public void Reset_WhenIgnoringOpenALCalls_DoesNotAttemptToResetSound()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);

        // Act
        sut.Reset();

        // Assert
        this.mockALInvoker.Verify(m => m.SourceRewind(It.IsAny<uint>()), Times.Never());
    }

    [Fact]
    public void SetTimePosition_WhenDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        sut.Dispose();

        // Act
        var act = () => sut.SetTimePosition(5);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The sound is disposed.  You must create another sound instance.");
    }

    [Theory]
    [InlineData(10f, 10f)]
    [InlineData(-2f, 0f)]
    [InlineData(300f, 50.0001373f)]
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
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        sut.SetTimePosition(seconds);

        // Assert
        this.mockALInvoker.Verify(m => m.Source(SrcId, ALSourcef.SecOffset, expected), Times.Once());
    }

    [Fact]
    public void SetTimePosition_WithIgnoringOpenALCalls_DoesNotAttemptToSetTimePosition()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);

        // Act
        sut.SetTimePosition(1f);

        // Assert
        this.mockALInvoker.Verify(m => m.Source(It.IsAny<uint>(), It.IsAny<ALSourcef>(), It.IsAny<float>()), Times.Never());
    }

    [Fact]
    public void Rewind_WhenTimeIsPastBeginningOfSound_ResetsAndPlaysSound()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockALInvoker.Setup(m => m.GetSourceState(SrcId)).Returns(ALSourceState.Stopped);
        this.mockALInvoker.Setup(m => m.GetSource(SrcId, ALSourcef.SecOffset)).Returns(10f);

        // Act
        sut.Rewind(20f);

        // Assert
        this.mockALInvoker.Verify(m => m.SourceRewind(SrcId), Times.Once());
        this.mockALInvoker.Verify(m => m.SourcePlay(SrcId), Times.Once());
        this.mockALInvoker.Verify(m => m.Source(It.IsAny<uint>(), It.IsAny<ALSourcef>(), It.IsAny<float>()), Times.Never());
    }

    [Fact]
    public void Rewind_WhenRewinding10Seconds_Rewinds10Seconds()
    {
        // Arrange
        MockSoundLength(25);
        this.mockALInvoker.Setup(m => m.GetSource(SrcId, ALSourcef.SecOffset))
            .Returns(15f);
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        sut.Rewind(10f);

        // Assert
        this.mockALInvoker.Verify(m => m.Source(SrcId, ALSourcef.SecOffset, 5f), Times.Once());
    }

    [Fact]
    public void Rewind_WhenIgnoringOpenALCalls_DoesNotAttemptToRewindSound()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);

        // Act
        sut.Rewind(10f);

        // Assert
        this.mockALInvoker.Verify(m => m.Source(It.IsAny<uint>(), ALSourcef.SecOffset, It.IsAny<float>()), Times.Never());
    }

    [Fact]
    public void FastForward_WhenTimeIsPastEndOfSound_ResetsSound()
    {
        // Arrange
        MockSoundLength(10);
        this.mockALInvoker.Setup(m => m.GetSource(SrcId, ALSourcef.SecOffset)).Returns(10f);

        var sut = CreateSound(this.oggContentFilePath);

        // Act
        sut.FastForward(20f);

        // Assert
        this.mockALInvoker.Verify(m => m.SourceRewind(SrcId), Times.Once());
        this.mockALInvoker.Verify(m => m.Source(It.IsAny<uint>(), It.IsAny<ALSourcef>(), It.IsAny<float>()), Times.Never());
    }

    [Fact]
    public void FastForward_WhenIgnoringOpenALCalls_DoesNotAttemptToResetSound()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);

        // Act
        sut.FastForward(20f);

        // Assert
        this.mockALInvoker.Verify(m => m.SourceRewind(It.IsAny<uint>()), Times.Never());
        this.mockALInvoker.Verify(m => m.Source(It.IsAny<uint>(), It.IsAny<ALSourcef>(), It.IsAny<float>()), Times.Never());
    }

    [Fact]
    public void FastForward_WhenFastForwarding10Seconds_Forwards10Seconds()
    {
        // Arrange
        MockSoundLength(60);
        this.mockOggDecoder.Setup(m => m.LoadData(this.oggContentFilePath)).Returns(() =>
        {
            var result = new SoundData<float>
            {
                BufferData = new ReadOnlyCollection<float>(this.oggBufferData),
                Channels = 2,
                Format = AudioFormat.Stereo16,
                SampleRate = 44100,
            };

            return result;
        });

        this.mockALInvoker.Setup(m => m.GetSource(SrcId, ALSourcef.SecOffset)).Returns(30f);
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        sut.FastForward(10f);

        // Assert
        this.mockALInvoker.Verify(m => m.Source(SrcId, ALSourcef.SecOffset, 40f), Times.Once());
    }

    [Fact]
    public void Sound_WhenChangingAudioDevice_ReinitializeSound()
    {
        // Arrange
        // Simulate an audio device change so the event is invoked inside of the sound class
        this.mockAudioManager.Setup(m => m.ChangeDevice(It.IsAny<string>()))
            .Callback<string>(_ =>
            {
                this.mockAudioManager.Raise(manager => manager.DeviceChanged += (_, _) => { }, EventArgs.Empty);
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
        _ = CreateSound(this.oggContentFilePath);

        // Act
        this.mockAudioManager.Object.ChangeDevice(It.IsAny<string>());

        // Assert
        // NOTE: The first invoke is during Sound construction, the second is when changing audio devices
        this.mockOggDecoder.Verify(m => m.LoadData(this.oggContentFilePath), Times.Exactly(2));
        this.mockALInvoker.Verify(m => m.BufferData(BufferId, ALFormat.Stereo16, new[] { 1f, 2f }, 44100), Times.Exactly(2));
    }

    [Fact]
    [SuppressMessage("csharpsquid", "S3966", Justification = "Need to execute dispose twice for testing.")]
    public void Dispose_WhenInvoked_DisposesOfSound()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        sut.Dispose();
        sut.Dispose();

        // Assert
        this.mockOggDecoder.Verify(m => m.Dispose(), Times.Once());
        this.mockMp3Decoder.Verify(m => m.Dispose(), Times.Once());
        this.mockAudioManager.VerifyRemove(m => m.DeviceChanged -= It.IsAny<EventHandler<EventArgs>>(),
            Times.Once(),
            $"Un-subscription to the event '{nameof(IAudioDeviceManager.DeviceChanged)}' event did not occur.");
        this.mockALInvoker.VerifyRemove(m => m.ErrorCallback -= It.IsAny<Action<string>>(),
            Times.Once(),
            $"Un-subscription to the event '{nameof(IOpenALInvoker.ErrorCallback)}' event did not occur.");
    }

    [Fact]
    public void Dispose_WhenIgnoringOpenALCalls_DoesNotAttemptToUnloadData()
    {
        // Arrange
        var sut = CreateSound(this.oggContentFilePath);
        this.mockAudioManager.Raise(manager => manager.DeviceChanging += null, EventArgs.Empty);

        // Act
        sut.Dispose();

        // Assert
        this.mockALInvoker.Verify(m => m.DeleteSource(It.IsAny<uint>()), Times.Never());
        this.mockALInvoker.Verify(m => m.DeleteBuffer(It.IsAny<uint>()), Times.Never());
        this.mockAudioManager.Verify(m => m.RemoveSoundSource(It.IsAny<uint>()), Times.Never());
    }

    [Fact]
    [SuppressMessage("csharpsquid", "S3966", Justification = "Need to execute dispose twice for testing.")]
    public void Dispose_WithInvalidSourceID_DoesNotAttemptSourceAndBufferDeletion()
    {
        // Arrange
        this.mockAudioManager.Setup(m => m.InitSound()).Returns((0u, BufferId));
        var sut = CreateSound(this.oggContentFilePath);

        // Act
        sut.Dispose();
        sut.Dispose();

        // Assert
        this.mockALInvoker.Verify(m => m.DeleteSource(SrcId), Times.Never());
    }
    #endregion

    /// <summary>
    /// Creates an instance of <see cref="Sound"/> for testing.
    /// </summary>
    /// <param name="filePath">The path to the sound file.</param>
    /// <returns>The instance for testing.</returns>
    private Sound CreateSound(string filePath)
        => new (filePath,
            this.mockALInvoker.Object,
            this.mockAudioManager.Object,
            this.mockOggDecoder.Object,
            this.mockMp3Decoder.Object,
            this.mockPath.Object);

    /// <summary>
    /// Mocks the buffer data stats to influence the total seconds that the sound has.
    /// </summary>
    /// <param name="totalSeconds">The total number of seconds to simulate.</param>
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

        this.mockALInvoker.Setup(m => m.GetBuffer(BufferId, ALGetBufferi.Size)).Returns(size);
        this.mockALInvoker.Setup(m => m.GetBuffer(BufferId, ALGetBufferi.Channels)).Returns(channels);
        this.mockALInvoker.Setup(m => m.GetBuffer(BufferId, ALGetBufferi.Bits)).Returns(bitDepth);
        this.mockALInvoker.Setup(m => m.GetBuffer(BufferId, ALGetBufferi.Frequency)).Returns(freq);
    }
}
