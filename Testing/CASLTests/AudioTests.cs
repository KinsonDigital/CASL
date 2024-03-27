// <copyright file="AudioTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

#pragma warning disable SA1202

namespace CASLTests;

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Carbonate.OneWay;
using CASL;
using CASL.Data;
using CASL.Devices;
using CASL.Exceptions;
using CASL.Factories;
using CASL.OpenAL;
using CASL.ReactableData;
using FluentAssertions;
using Helpers;
using NSubstitute;
using Xunit;

/// <summary>
/// Tests the <see cref="Audio"/> class.
/// </summary>
public class AudioTests
{
    private const string OggFileExtension = ".ogg";
    private const string MP3FileExtension = ".mp3";
    private const uint SrcId = 1234;
    private const uint BufferId = 5678;
    private const string SoundFileNameWithoutExtension = "sound";
    private const string BaseDirPath = "C:/temp/Content/Sounds";
    private const string Mp3ContentFilePath = $"{BaseDirPath}/{SoundFileNameWithoutExtension}{MP3FileExtension}";
    private const string OggContentFilePath = $"{BaseDirPath}/{SoundFileNameWithoutExtension}{OggFileExtension}";
    private readonly IAudioDeviceManager mockAudioManager;
    private readonly IOpenALInvoker mockALInvoker;
    private readonly IAudioBufferFactory mockAudioBufferFactory;
    private readonly IReactableFactory mockReactableFactory;
    private readonly IPushReactable<AudioCommandData> mockAudioCmdReactable;
    private readonly IPushReactable<PosCommandData> mockPosCmnReactable;
    private readonly IPullReactable<bool> mockLoopingReactable;
    private readonly IAudioBuffer mockAudioBuffer;
    private readonly IPath mockPath;
    private readonly IFile mockFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioTests"/> class.
    /// </summary>
    [SuppressMessage("csharpsquid", "S1075", Justification = "Only used for testing")]
    public AudioTests()
    {
        this.mockALInvoker = Substitute.For<IOpenALInvoker>();
        this.mockALInvoker.GenSource().Returns(SrcId);
        this.mockALInvoker.GenBuffer().Returns(BufferId);
        this.mockALInvoker.GetSourceState(Arg.Any<uint>()).Returns(ALSourceState.Stopped);

        this.mockAudioManager = Substitute.For<IAudioDeviceManager>();

        this.mockAudioBuffer = Substitute.For<IAudioBuffer>();
        this.mockAudioBuffer.Init(Arg.Any<string>()).Returns(SrcId);

        this.mockAudioBufferFactory = Substitute.For<IAudioBufferFactory>();
        this.mockAudioBufferFactory.CreateFullBuffer(Arg.Any<string>()).Returns(this.mockAudioBuffer);
        this.mockAudioBufferFactory.CreateStreamBuffer(Arg.Any<string>()).Returns(this.mockAudioBuffer);

        this.mockAudioCmdReactable = Substitute.For<IPushReactable<AudioCommandData>>();
        this.mockPosCmnReactable = Substitute.For<IPushReactable<PosCommandData>>();
        this.mockLoopingReactable = Substitute.For<IPullReactable<bool>>();

        this.mockReactableFactory = Substitute.For<IReactableFactory>();
        this.mockReactableFactory.CreateAudioCmndReactable().Returns(this.mockAudioCmdReactable);
        this.mockReactableFactory.CreatePositionCmndReactable().Returns(this.mockPosCmnReactable);
        this.mockReactableFactory.CreateIsLoopingReactable().Returns(this.mockLoopingReactable);

        this.mockPath = Substitute.For<IPath>();
        this.mockPath.GetExtension(OggContentFilePath).Returns(OggFileExtension);
        this.mockPath.GetExtension(Mp3ContentFilePath).Returns(MP3FileExtension);
        this.mockPath.GetFileNameWithoutExtension(Arg.Any<string?>()).Returns(SoundFileNameWithoutExtension);

        this.mockFile = Substitute.For<IFile>();
        this.mockFile.Exists(Arg.Any<string>()).Returns(true);
    }

    #region Constructor Tests
    [Theory]
    [InlineData(null, "Value cannot be null. (Parameter 'filePath')")]
    [InlineData("", "The value cannot be an empty string. (Parameter 'filePath')")]
    public void Ctor_WithNullFilePathParam_ThrowsException(string? filePath, string expected)
    {
        // Arrange & Act
        var act = () => _ = new Audio(
            filePath,
            BufferType.Full,
            this.mockALInvoker,
            this.mockAudioManager,
            this.mockAudioBufferFactory,
            this.mockReactableFactory,
            this.mockPath,
            this.mockFile);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage(expected);
    }

    [Fact]
    public void Ctor_WithNullALInvokerParam_ThrowsException()
    {
        // Arrange & Act
        var act = void () => _ = new Audio(
            OggContentFilePath,
            BufferType.Full,
            null,
            this.mockAudioManager,
            this.mockAudioBufferFactory,
            this.mockReactableFactory,
            this.mockPath,
            this.mockFile);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'alInvoker')");
    }

    [Fact]
    public void Ctor_WithNullAudioManagerParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => _ = new Audio(
            OggContentFilePath,
            BufferType.Full,
            this.mockALInvoker,
            null,
            this.mockAudioBufferFactory,
            this.mockReactableFactory,
            this.mockPath,
            this.mockFile);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'audioManager')");
    }

    [Fact]
    public void Ctor_WithNullBufferFactoryParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => _ = new Audio(
            OggContentFilePath,
            BufferType.Full,
            this.mockALInvoker,
            this.mockAudioManager,
            null,
            this.mockReactableFactory,
            this.mockPath,
            this.mockFile);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'bufferFactory')");
    }

    [Fact]
    public void Ctor_WithNullReactableFactoryParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => _ = new Audio(
            OggContentFilePath,
            BufferType.Full,
            this.mockALInvoker,
            this.mockAudioManager,
            this.mockAudioBufferFactory,
            null,
            this.mockPath,
            this.mockFile);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'reactableFactory')");
    }

    [Fact]
    public void Ctor_WithNullPathParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => _ = new Audio(
            OggContentFilePath,
            BufferType.Full,
            this.mockALInvoker,
            this.mockAudioManager,
            this.mockAudioBufferFactory,
            this.mockReactableFactory,
            null,
            this.mockFile);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'path')");
    }

    [Fact]
    public void Ctor_WithNullFileParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => _ = new Audio(
            OggContentFilePath,
            BufferType.Full,
            this.mockALInvoker,
            this.mockAudioManager,
            this.mockAudioBufferFactory,
            this.mockReactableFactory,
            this.mockPath,
            null);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'file')");
    }

    [Fact]
    public void Ctor_WhenFileDoesNotExist_ThrowsException()
    {
        // Arrange
        this.mockFile.Exists(Arg.Any<string>()).Returns(false);

        // Act
        var act = () => _ = CreateSystemUnderTest("non-existing-file.data");

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage("The sound file could not be found.")
            .Subject.First().FileName.Should().Be("non-existing-file.data");
    }

    [Fact]
    public void Ctor_WhenUsingUnknownBufferType_ThrowsException()
    {
        // Arrange
        const string expected = "The value of argument 'bufferType' (1000) is invalid for Enum type 'BufferType'. (Parameter 'bufferType')";

        // Act
        var act = () => _ = CreateSystemUnderTest(OggContentFilePath, (BufferType)1000);

        act.Should().Throw<InvalidEnumArgumentException>().WithMessage(expected);
    }

    [Fact]
    public void Ctor_WhenInvoking_SubscribesToDeviceChangedEvent()
    {
        // Arrange
        var eventInvoked = false;

        this.mockAudioManager.DeviceChanged += (_, _) => eventInvoked = true;

        // Act
        var sut = CreateSystemUnderTest(OggContentFilePath);
        this.mockAudioManager.DeviceChanged += Raise.EventWith(sut, EventArgs.Empty);

        // Assert
        eventInvoked.Should().BeTrue();
    }

    [Theory]
    [InlineData(BufferType.Full)]
    [InlineData(BufferType.Stream)]
    public void Ctor_WhenInvoked_InitializesSound(BufferType bufferType)
    {
        // Arrange & Act
        var sut = CreateSystemUnderTest(OggContentFilePath, bufferType);

        // Assert
        sut.BufferType.Should().Be(bufferType);
        this.mockAudioBuffer.Received(1).Init(OggContentFilePath);
        this.mockAudioBuffer.Received(1).Upload();
    }

    [Fact]
    public void Ctor_WhenUsingUnsupportedFileType_ThrowsException()
    {
        // Arrange
        const string expected = "The file extension '.wav' is not supported. Supported extensions are '.ogg' and '.mp3'.";
        this.mockPath.GetExtension(Arg.Any<string?>()).Returns(".wav");

        // Act
        var act = () => _ = new Audio(
            @"C:\temp\Content\Sounds\sound.wav",
            BufferType.Full,
            this.mockALInvoker,
            this.mockAudioManager,
            this.mockAudioBufferFactory,
            this.mockReactableFactory,
            this.mockPath,
            this.mockFile);

        // Assert
        act.Should().Throw<AudioException>()
            .WithMessage(expected);
    }
    #endregion

    #region Prop Tests
    [Fact]
    public void Name_WhenGettingValue_ReturnsCorrectResult()
    {
        // Act
        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Assert
        sut.Name.Should().Be("sound");
    }

    [Fact]
    public void IsLooping_WhenGettingValueWhileChangingDevices_ReturnsFalse()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);

        // Act
        var actual = sut.IsLooping;

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void IsLooping_WhenGettingValueWhileDisposed_ReturnsFalse()
    {
        // Arrange
        this.mockLoopingReactable.Pull(Arg.Any<Guid>()).Returns(false);

        var sut = CreateSystemUnderTest(OggContentFilePath);
        sut.Dispose();

        // Act
        var actual = sut.IsLooping;

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void IsLooping_WhenGettingValueWhileNotDisposed_GetsSoundLoopState()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        _ = sut.IsLooping;

        // Assert
        this.mockLoopingReactable.Received(1).Pull(PullNotifications.GetLoopState);
    }

    [Fact]
    public void IsLooping_WhenSettingValueWhileDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
        sut.Dispose();

        // Act
        var act = () => sut.IsLooping = true;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The sound is disposed.  You must create another sound instance.");
    }

    [Fact]
    public void IsLooping_WhenSettingValueWhileChangingDevices_ReturnsFalse()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);

        // Act
        sut.IsLooping = true;

        // Assert
        this.mockAudioCmdReactable.DidNotReceive().Push(Arg.Any<Guid>(), Arg.Any<AudioCommandData>());
    }

    [Theory]
    [InlineData(true, AudioCommands.EnableLooping)]
    [InlineData(false, AudioCommands.DisableLooping)]
    internal void IsLooping_WhenSettingValue_SetsSoundLoopingSetting(bool value, AudioCommands cmd)
    {
        // Arrange
        var expectedCmd = new AudioCommandData { SourceId = SrcId, Command = cmd, };
        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        sut.IsLooping = value;

        // Assert
        this.mockAudioCmdReactable.Received(1).Push(PushNotifications.SendAudioCmd, expectedCmd);
    }

    [Fact]
    public void Volume_WhenGettingValueWhileChangingDevices_ReturnsVolumeBeforeStartOfDeviceChangeProcess()
    {
        // Arrange
        this.mockALInvoker.GetSource(Arg.Any<uint>(), Arg.Any<ALSourcef>()).Returns(0.5f);

        var sut = CreateSystemUnderTest(OggContentFilePath);
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);

        // Act
        var actual = sut.Volume;

        // Assert
        actual.Should().Be(50f);
    }

    [Fact]
    public void Volume_WhenGettingValueWhileDisposed_ReturnsZero()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
        sut.Dispose();

        // Act
        var actual = sut.Volume;

        // Assert
        actual.Should().Be(0);
    }

    [Fact]
    public void Volume_WhenGettingValue_GetsSoundVolume()
    {
        // Arrange
        this.mockALInvoker.GetSource(Arg.Any<uint>(), ALSourcef.Gain).Returns(0.123f);
        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        var actual = sut.Volume;

        // Assert
        actual.Should().Be(12.3f);
        this.mockALInvoker.Received(1).GetSource(SrcId, ALSourcef.Gain);
    }

    [Fact]
    public void Volume_WhenSettingValueWhileDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
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
        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        sut.Volume = volume;

        // Assert
        this.mockALInvoker.Received(1).Source(SrcId, ALSourcef.Gain, expected);
    }

    [Fact]
    public void Volume_WhenSettingVolumeWhileChangingDevices_SetsValueAfterDeviceHasChanged()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Invoke that the device is changing which should cache any incoming values
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);

        // Act
        sut.Volume = 30f;

        // Invoked the device changed event which mean the device has switch has completed.
        // The system should set the intended volume that was requested during the device change process.
        this.mockAudioManager.DeviceChanged += Raise.EventWith(sut, EventArgs.Empty);

        // Assert
        this.mockALInvoker.Received(1).GetSource(SrcId, ALSourcef.Gain);
        this.mockALInvoker.Source(SrcId, ALSourcef.Gain, 0.30f);
    }

    [Fact]
    public void Position_WhenGettingValue_ReturnsCorrectResult()
    {
        // Arrange
        var expected = new AudioTime(90);
        this.mockAudioBuffer.Position.Returns(expected);
        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        var actual = sut.Position;

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void Position_WhenGettingValueWhileDisposed_ReturnsCorrectResult()
    {
        // Arrange
        var expected = default(AudioTime);
        this.mockAudioBuffer.Position.Returns(expected);
        var sut = CreateSystemUnderTest(OggContentFilePath);
        sut.Dispose();

        // Act
        var actual = sut.Position;

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void Position_WhenGettingValueWhileChangingDevices_ReturnsPositionBeforeStartOfDeviceChangeProcess()
    {
        // Arrange
        this.mockAudioBuffer.Position.Returns(new AudioTime(123));
        var sut = CreateSystemUnderTest(OggContentFilePath);
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);
        var expected = new AudioTime(123);

        // Act
        var actual = sut.Position;

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void Length_WhileDisposed_ReturnsZero()
    {
        // Arrange
        var expected = new AudioTime(0f);

        this.mockAudioBuffer.TotalSeconds.Returns(123f);

        var sut = CreateSystemUnderTest(OggContentFilePath);
        sut.Dispose();

        // Act
        var actual = sut.Length;

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void Length_WhenGettingValue_ReturnsCorrectResult()
    {
        // Arrange
        var expected = new AudioTime(266.00076f);

        this.mockAudioBuffer.TotalSeconds.Returns(266.00076f);

        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        var actual = sut.Length;

        // Assert
        actual.TotalSeconds.Should().Be(expected.TotalSeconds);
    }

    [Fact]
    public void Length_WhenGettingValueWhileChangingDevices_ReturnsZero()
    {
        // Arrange
        var expected = new AudioTime(0f);
        this.mockAudioBuffer.TotalSeconds.Returns(123);
        var sut = CreateSystemUnderTest(OggContentFilePath);

        this.mockAudioManager.DeviceChanging += Raise.EventWith(this, EventArgs.Empty);

        // Act
        var actual = sut.Length;

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void State_WhenGettingValueWhileDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
        sut.Dispose();

        // Act
        var actual = sut.State;

        // Assert
        actual.Should().Be(AudioState.Stopped);
    }

    [Fact]
    public void State_WhenGettingValueWhileChangingDevices_ReturnsAsStopped()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);

        // Act
        var actual = sut.State;

        // Assert
        actual.Should().Be(AudioState.Stopped);
    }

    [Theory]
    [InlineData(0x1011, AudioState.Stopped)]
    [InlineData(0x1012, AudioState.Playing)]
    [InlineData(0x1013, AudioState.Paused)]
    [InlineData(0x1014, AudioState.Stopped)]
    public void State_WhenGettingValue_ReturnsCorrectResult(int openALState, AudioState expected)
    {
        /*NOTE:
         * The first InlineData value must be a raw integer value of the ALSourceState enum.
         * This is because ALSourceState is scoped as internal and we cannot use it as a parameter
         * for the unit test method.  This is illegal. So the value is casted to get around this limitation
         * for testing purposes.
         */

        // Arrange
        this.mockALInvoker.GetSourceState(SrcId).Returns((ALSourceState)openALState);
        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        var actual = sut.State;

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void PlaySpeed_WhenGettingValueWhileDisposed_ReturnsZero()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
        sut.Dispose();

        // Act
        var actual = sut.PlaySpeed;

        // Assert
        actual.Should().Be(0f);
    }

    [Fact]
    public void PlaySpeed_WhenChangingDevices_ReturnsZero()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);

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
        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        sut.PlaySpeed = speedValue;
        _ = sut.PlaySpeed;

        // Assert
        this.mockALInvoker.Received(1).GetSource(SrcId, ALSourcef.Pitch);
        this.mockALInvoker.Received(1).Source(SrcId, ALSourcef.Pitch, expectedResult);
    }

    [Fact]
    public void PlaySpeed_WhenChangingDevices_DoesNotMakeOpenALCalls()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);

        // Act
        sut.PlaySpeed = 1f;

        // Assert
        this.mockALInvoker.DidNotReceive().Source(Arg.Any<uint>(), Arg.Any<ALSourcef>(), Arg.Any<float>());
    }
    #endregion

    #region Method Tests
    [Fact]
    public void Play_WhenDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
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
        var expectedCmd = new AudioCommandData { SourceId = SrcId, Command = AudioCommands.Play, };

        this.mockALInvoker.GetSourceState(SrcId).Returns(ALSourceState.Stopped);
        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        sut.Play();

        // Assert
        this.mockAudioCmdReactable.Received(1).Push(PushNotifications.SendAudioCmd, expectedCmd);
    }

    [Fact]
    public void Play_WhileChangingDevices_DoesNotAttemptToPlaySound()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);

        this.mockALInvoker.GetSourceState(SrcId).Returns(ALSourceState.Paused);
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);

        // Act
        sut.Play();

        // Assert
        this.mockAudioCmdReactable.DidNotReceive().Push(Arg.Any<Guid>(), Arg.Any<AudioCommandData>());
    }

    [Fact]
    public void Pause_WhenDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
        sut.Dispose();

        // Act
        var act = () => sut.Pause();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The sound is disposed.  You must create another sound instance.");
    }

    [Fact]
    public void Pause_WhileChangingDevices_DoesNotAttemptToPauseSound()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);

        // Act
        sut.Pause();

        // Assert
        this.mockAudioCmdReactable.DidNotReceive().Push(Arg.Any<Guid>(), Arg.Any<AudioCommandData>());
    }

    [Fact]
    public void Pause_WhenInvoked_PausesSound()
    {
        // Arrange
        var expectedCmd = new AudioCommandData { SourceId = SrcId, Command = AudioCommands.Pause, };
        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        sut.Pause();

        // Assert
        this.mockAudioCmdReactable.Received(1).Push(PushNotifications.SendAudioCmd, expectedCmd);
    }

    [Fact]
    public void Reset_WhenDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
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
        var expectedCmd = new AudioCommandData { SourceId = SrcId, Command = AudioCommands.Reset, };
        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        sut.Reset();

        // Assert
        this.mockAudioCmdReactable.Received(1).Push(PushNotifications.SendAudioCmd, expectedCmd);
    }

    [Fact]
    public void Reset_WhileChangingDevices_DoesNotAttemptToResetSound()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);

        // Act
        sut.Reset();

        // Assert
        this.mockAudioCmdReactable.DidNotReceive().Push(Arg.Any<Guid>(), Arg.Any<AudioCommandData>());
    }

    [Fact]
    public void SetTimePosition_WhenDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
        sut.Dispose();

        // Act
        var act = () => sut.SetTimePosition(5);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The sound is disposed.  You must create another sound instance.");
    }

    [Theory]
    [InlineData(50f, 100f, 50f)]
    [InlineData(150f, 100f, 100f)]
    [InlineData(100f, 100f, 100f)]
    [InlineData(-50f, 100f, 0f)]
    public void SetTimePosition_SettingPosition_SendsCorrectSetPositionCommand(
        float requestedPos,
        float totalSeconds,
        float expectedPos)
    {
        // Arrange
        var expectedPosCmd = new PosCommandData { SourceId = SrcId, PositionSeconds = expectedPos, };

        this.mockAudioBuffer.TotalSeconds.Returns(totalSeconds);

        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        sut.SetTimePosition(requestedPos);

        // Assert
        this.mockPosCmnReactable.Received(1).Push(PushNotifications.UpdateSoundPos, expectedPosCmd);
    }

    [Fact]
    public void SetTimePosition_WithChangingDevices_SetsToRequestedPositionAfterCompletingDeviceChangeProcess()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Put the device into a changing device state
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);

        // Act
        sut.SetTimePosition(123f);

        // Assert
        this.mockPosCmnReactable.DidNotReceive().Push(Arg.Any<Guid>(), Arg.Any<PosCommandData>());
    }

    [Fact]
    public void Rewind_WhenTimeIsPastBeginningOfSound_ResetsAndPlaysSound()
    {
        // Arrange
        var expectedCmd = new PosCommandData
        {
            SourceId = SrcId,
            PositionSeconds = 0f,
        };

        var sut = CreateSystemUnderTest(OggContentFilePath);
        this.mockAudioBuffer.Position.Returns(new AudioTime(10f));
        this.mockAudioBuffer.TotalSeconds.Returns(300f);

        // Act
        sut.Rewind(20f);

        // Assert
        this.mockPosCmnReactable.Received(1).Push(PushNotifications.UpdateSoundPos, expectedCmd);
    }

    [Fact]
    public void Rewind_WhenRewinding10Seconds_Rewinds10Seconds()
    {
        // Arrange
        var expected = new PosCommandData { SourceId = SrcId, PositionSeconds = 90f, };

        this.mockAudioBuffer.TotalSeconds.Returns(123.456f);
        this.mockAudioBuffer.Position.Returns(new AudioTime(100f));
        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        sut.Rewind(10f);

        // Assert
        this.mockPosCmnReactable.Received(1).Push(PushNotifications.UpdateSoundPos, expected);
    }

    [Fact]
    public void Rewind_WhileChangingDevices_DoesNotAttemptToRewindSound()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);

        // Act
        sut.Rewind(10f);

        // Assert
        this.mockALInvoker.DidNotReceive().Source(Arg.Any<uint>(), ALSourcef.SecOffset, Arg.Any<float>());
    }

    [Theory]
    [InlineData(20f, 20f)]
    [InlineData(150f, 123f)]
    public void FastForward_WhenInvoked_SendsCorrectCommand(float seconds, float expectedSeconds)
    {
        // Arrange
        var expected = new PosCommandData { SourceId = SrcId, PositionSeconds = expectedSeconds, };

        this.mockAudioBuffer.TotalSeconds.Returns(123.456f);
        this.mockALInvoker.GetSource(SrcId, ALSourcef.SecOffset).Returns(10f);

        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        sut.FastForward(seconds);

        // Assert
        this.mockPosCmnReactable.Received(1).Push(PushNotifications.UpdateSoundPos, expected);
    }

    [Fact]
    public void FastForward_WhileChangingDevices_DoesNotAttemptToResetSound()
    {
        // Arrange
        var sut = CreateSystemUnderTest(OggContentFilePath);
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);

        // Act
        sut.FastForward(20f);

        // Assert
        this.mockALInvoker.DidNotReceive().SourceRewind(Arg.Any<uint>());
        this.mockALInvoker.DidNotReceive().Source(Arg.Any<uint>(), Arg.Any<ALSourcef>(), Arg.Any<float>());
    }

    [Fact]
    public void Sound_WhenChangingAudioDevice_ReinitializeSound()
    {
        // Arrange
        // Simulate an audio device change so the event is invoked inside of the sound class
        var sut = CreateSystemUnderTest(OggContentFilePath);

        this.mockAudioManager.DeviceChanged += Raise.EventWith(sut, EventArgs.Empty);

        // Act
        this.mockAudioManager.ChangeDevice(Arg.Any<string>());
        var actualDeviceChangingState = sut.GetBoolFieldValue("audioDeviceChanging");

        // Assert
        this.mockAudioBuffer.Received(2).Init(OggContentFilePath);
        this.mockAudioBuffer.Received(2).Upload();
        actualDeviceChangingState.Should().BeFalse();
    }

    [Theory]
    [InlineData(ALSourceState.Stopped)]
    [InlineData(ALSourceState.Playing)]
    [InlineData(ALSourceState.Paused)]
    [InlineData(ALSourceState.Initial)]
    [SuppressMessage("csharpsquid", "S3966", Justification = "Need to execute dispose twice for testing.")]
    internal void Dispose_WhenInvoked_DisposesOfSound(ALSourceState sourceState)
    {
        // Arrange
        this.mockALInvoker.GetSourceState(Arg.Any<uint>()).Returns(sourceState);

        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        sut.Dispose();
        sut.Dispose();

        var actualIsDisposed = sut.GetBoolFieldValue("isDisposed");

        actualIsDisposed.Should().BeTrue();

        var errorCallbackAct = () => this.mockALInvoker.ErrorCallback += Raise.Event<Action<string>>("test-error");

        errorCallbackAct.Should().NotThrow<AudioException>();

        // Raise the DeviceChanging event to verify that it has been unsubscribed
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);
        var changingInvoked = sut.GetBoolFieldValue("audioDeviceChanging");

        // Set the value back to true to verify that it has not been changed without the influence of its default value
        // and the correct usage of the DeviceChanging event
        sut.SetBoolField("audioDeviceChanging", true);
        this.mockAudioManager.DeviceChanged += Raise.EventWith(sut, EventArgs.Empty);
        var changedInvoked = sut.GetBoolFieldValue("audioDeviceChanging");

        // Assert
        this.mockALInvoker.Received(sourceState == ALSourceState.Playing ? 1 : 0).SourceStop(SrcId);
        this.mockAudioBuffer.Received(1).Dispose();
        changingInvoked.Should().BeFalse("the DeviceChanging event was not unsubscribed.");

        // Assert that the device changed was not invoked
        changedInvoked.Should().BeTrue("the DeviceChanged event was not unsubscribed.");
    }
    #endregion

    #region Indirect Tests
    [Theory]
    [InlineData(ALSourceState.Playing)]
    [InlineData(ALSourceState.Paused)]
    [InlineData(ALSourceState.Stopped)]
    [InlineData(ALSourceState.Initial)]
    internal void DeviceChangingProcess_WhenChangingDevices_CorrectlyRestoresSound(
        ALSourceState stateBeforeChange)
    {
        // Arrange
        var expectedTimePos = new PosCommandData
        {
            SourceId = SrcId,
            PositionSeconds = 50f,
        };

        var expectedAudioCmd = new AudioCommandData
        {
            SourceId = SrcId,
            Command = AudioCommands.Play,
        };

        this.mockAudioBuffer.TotalSeconds.Returns(100f);
        this.mockALInvoker.GetSourceState(Arg.Any<uint>()).Returns(stateBeforeChange);
        this.mockALInvoker.GetSource(Arg.Any<uint>(), ALSourcef.Gain).Returns(0.5f);
        this.mockALInvoker.GetSource(Arg.Any<uint>(), ALSourcef.Pitch).Returns(1.2f);
        this.mockAudioBuffer.Position.Returns(new AudioTime(50f));

        var sut = CreateSystemUnderTest(OggContentFilePath);

        // Act
        this.mockAudioManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);
        this.mockAudioManager.DeviceChanged += Raise.EventWith(sut, EventArgs.Empty);

        // Assert
        // Assert that the changing process has been completed
        this.mockALInvoker.Received(1).GetSourceState(SrcId);
        this.mockALInvoker.Received(stateBeforeChange == ALSourceState.Stopped ? 0 : 1).SourceStop(SrcId);
        this.mockALInvoker.Received(1).GetSource(SrcId, ALSourcef.Gain);
        this.mockALInvoker.Received(1).GetSource(SrcId, ALSourcef.Pitch);
        this.mockAudioBuffer.Received(1).RemoveBuffer();
        this.mockALInvoker.Received(1).DeleteSource(SrcId);

        // Assert that the changed process has been completed
        this.mockAudioBuffer.Received(2).Init(OggContentFilePath);
        this.mockAudioBuffer.Received(2).Upload();
        this.mockALInvoker.Received(1).Source(SrcId, ALSourcef.Gain, 0.5f);
        this.mockPosCmnReactable.Received(1).Push(PushNotifications.UpdateSoundPos, expectedTimePos);
        this.mockALInvoker.Received(1).Source(SrcId, ALSourcef.Pitch, 1.2f);
        this.mockAudioCmdReactable.Received(stateBeforeChange == ALSourceState.Playing ? 1 : 0).Push(PushNotifications.SendAudioCmd, expectedAudioCmd);
    }
    #endregion

    /// <summary>
    /// Creates an instance of <see cref="Audio"/> for testing.
    /// </summary>
    /// <param name="filePath">The path to the sound file.</param>
    /// <param name="bufferType">The type of buffer.</param>
    /// <returns>The instance for testing.</returns>
    private Audio CreateSystemUnderTest(string filePath, BufferType bufferType = BufferType.Full)
        => new (filePath,
            bufferType,
            this.mockALInvoker,
            this.mockAudioManager,
            this.mockAudioBufferFactory,
            this.mockReactableFactory,
            this.mockPath,
            this.mockFile);
}
