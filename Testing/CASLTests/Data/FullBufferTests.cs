// <copyright file="FullBufferTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Data;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using Carbonate.Core.OneWay;
using Carbonate.OneWay;
using CASL;
using CASL.Data;
using CASL.Data.Decoders;
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
/// Tests the <see cref="FullBuffer"/> class.
/// </summary>
public class FullBufferTests
{
    private const uint SourceId = 456u;
    private readonly IOpenALInvoker mockAlInvoker;
    private readonly IAudioDeviceManager mockDeviceManager;
    private readonly IAudioDecoder mockAudioDecoder;
    private readonly IReactableFactory mockReactableFactory;
    private readonly IPath mockPath;
    private readonly IFile mockFile;
    private readonly IPushReactable<AudioCommandData> mockAudioCmdReactable;
    private readonly IPushReactable<PosCommandData> mockPosCmdReactable;
    private readonly IPullReactable<bool> mockLoopingReactable;
    private readonly IDisposable mockAudioCmdUnsubscriber;
    private readonly IDisposable mockPosCmdUnsubscriber;
    private readonly IDisposable mockLoopingUnsubscriber;
    private readonly uint[] buffers =
    [
        100u
    ];
    private IReceiveSubscription<AudioCommandData> audioCmdSubscription;
    private IReceiveSubscription<PosCommandData> posCmdSubscription;
    private IRespondSubscription<bool> loopSubscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="FullBufferTests"/> class.
    /// </summary>
    public FullBufferTests()
    {
        this.mockAlInvoker = Substitute.For<IOpenALInvoker>();

        this.mockDeviceManager = Substitute.For<IAudioDeviceManager>();
        this.mockDeviceManager.InitSound(Arg.Any<int>()).Returns((SourceId, this.buffers));

        this.mockAudioDecoder = Substitute.For<IAudioDecoder>();

        this.audioCmdSubscription = Substitute.For<IReceiveSubscription<AudioCommandData>>();
        this.mockAudioCmdUnsubscriber = Substitute.For<IDisposable>();
        this.mockAudioCmdReactable = Substitute.For<IPushReactable<AudioCommandData>>();
        this.mockAudioCmdReactable.Subscribe(Arg.Any<IReceiveSubscription<AudioCommandData>>())
            .Returns(this.mockAudioCmdUnsubscriber)
            .AndDoes(callInfo => this.audioCmdSubscription = callInfo.Arg<IReceiveSubscription<AudioCommandData>>());

        this.posCmdSubscription = Substitute.For<IReceiveSubscription<PosCommandData>>();
        this.mockPosCmdUnsubscriber = Substitute.For<IDisposable>();
        this.mockPosCmdReactable = Substitute.For<IPushReactable<PosCommandData>>();
        this.mockPosCmdReactable.Subscribe(Arg.Any<IReceiveSubscription<PosCommandData>>())
            .Returns(this.mockPosCmdUnsubscriber)
            .AndDoes(callInfo => this.posCmdSubscription = callInfo.Arg<IReceiveSubscription<PosCommandData>>());

        this.loopSubscription = Substitute.For<IRespondSubscription<bool>>();
        this.mockLoopingUnsubscriber = Substitute.For<IDisposable>();
        this.mockLoopingReactable = Substitute.For<IPullReactable<bool>>();
        this.mockLoopingReactable.Subscribe(Arg.Any<IRespondSubscription<bool>>())
            .Returns(this.mockLoopingUnsubscriber)
            .AndDoes(callInfo => this.loopSubscription = callInfo.Arg<IRespondSubscription<bool>>());

        this.mockReactableFactory = Substitute.For<IReactableFactory>();
        this.mockReactableFactory.CreateAudioCmndReactable().Returns(this.mockAudioCmdReactable);
        this.mockReactableFactory.CreatePositionCmndReactable().Returns(this.mockPosCmdReactable);
        this.mockReactableFactory.CreateIsLoopingReactable().Returns(this.mockLoopingReactable);

        this.mockPath = Substitute.For<IPath>();

        this.mockFile = Substitute.For<IFile>();
        this.mockFile.Exists(Arg.Any<string>()).Returns(true);
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullALInvokerParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new FullBuffer(
                null,
                this.mockDeviceManager,
                this.mockAudioDecoder,
                this.mockReactableFactory,
                this.mockPath,
                this.mockFile);
        };

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("alInvoker");
    }

    [Fact]
    public void Ctor_WithNullAudioDeviceManagerParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new FullBuffer(
                this.mockAlInvoker,
                null,
                this.mockAudioDecoder,
                this.mockReactableFactory,
                this.mockPath,
                this.mockFile);
        };

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("audioDeviceManager");
    }

    [Fact]
    public void Ctor_WithNullAudioDecoderParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new FullBuffer(
                this.mockAlInvoker,
                this.mockDeviceManager,
                null,
                this.mockReactableFactory,
                this.mockPath,
                this.mockFile);
        };

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("audioDecoder");
    }

    [Fact]
    public void Ctor_WithNullReactableFactoryParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new FullBuffer(
                this.mockAlInvoker,
                this.mockDeviceManager,
                this.mockAudioDecoder,
                null,
                this.mockPath,
                this.mockFile);
        };

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("reactableFactory");
    }

    [Fact]
    public void Ctor_WithNullPathParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new FullBuffer(
                this.mockAlInvoker,
                this.mockDeviceManager,
                this.mockAudioDecoder,
                this.mockReactableFactory,
                null,
                this.mockFile);
        };

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("path");
    }

    [Fact]
    public void Ctor_WithNullFileParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new FullBuffer(
                this.mockAlInvoker,
                this.mockDeviceManager,
                this.mockAudioDecoder,
                this.mockReactableFactory,
                this.mockPath,
                null);
        };

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("file");
    }

    [Fact]
    public void Ctor_WhenInvoked_SetsUpAudioCmdSubscription()
    {
        // Arrange & Arrange
        _ = CreateSystemUnderTest();

        // Assert
        this.mockAudioCmdReactable.Received(1).Subscribe(Arg.Any<IReceiveSubscription<AudioCommandData>>());
    }

    [Fact]
    public void Ctor_WhenInvoked_SetsUpPositionCmdSubscription()
    {
        // Arrange & Arrange
        _ = CreateSystemUnderTest();

        // Assert
        this.mockPosCmdReactable.Received(1).Subscribe(Arg.Any<IReceiveSubscription<PosCommandData>>());
    }

    [Fact]
    public void Ctor_WhenInvoked_SetsUpIsLoopingSubscription()
    {
        // Arrange & Arrange
        _ = CreateSystemUnderTest();

        // Assert
        this.mockLoopingReactable.Received(1).Subscribe(Arg.Any<IRespondSubscription<bool>>());
    }
    #endregion

    #region Prop Tests
    [Fact]
    public void TotalSeconds_WhenGettingValue_ReturnsCorrectResult()
    {
        // Arrange
        this.mockAudioDecoder.TotalSeconds.Returns(123);

        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.TotalSeconds;

        // Assert
        actual.Should().Be(123);
    }

    [Fact]
    public void Position_WhenGettingValueBeforeInitialization_ReturnsEmptySoundTime()
    {
        // Arrange
        var expected = default(SoundTime);
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.Position;

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void Position_WhenGettingValue_ReturnsCorrectResult()
    {
        // Arrange
        var expected = new SoundTime(123);

        this.mockAlInvoker.GetSource(Arg.Any<uint>(), ALSourcef.SecOffset).Returns(123);

        var sut = CreateSystemUnderTest();
        sut.Init("test-file-path.ogg");

        // Act
        var actual = sut.Position;

        // Assert
        actual.Should().Be(expected);
        this.mockAlInvoker.Received(1).GetSource(SourceId, ALSourcef.SecOffset);
    }
    #endregion

    #region Method Tests
    [Fact]
    public void Init_WithNullFilePath_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act
        var act = () => sut.Init(null);

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("filePath");
    }

    [Fact]
    public void Init_WithEmptyFilePath_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act
        var act = () => sut.Init(string.Empty);

        // Assert
        act.Should().ThrowArgException().WithEmptyStringParamMsg("filePath");
    }

    [Fact]
    public void Init_WhenFileDoesNotExist_ThrowsException()
    {
        // Arrange
        this.mockFile.Exists(Arg.Any<string>()).Returns(false);
        var sut = CreateSystemUnderTest();

        // Act
        var act = () => sut.Init("non-existing-file.ogg");

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage("The sound file could not be found.")
            .And.FileName.Should().Be("non-existing-file.ogg");
    }

    [Theory]
    [InlineData(".ogg", true)]
    [InlineData(".mp3", false)]
    public void Init_WhenBufferIsNotInitialized_InitializesBuffer(string extension, bool isInitialized)
    {
        // Arrange
        var expectedSoundSrc = new SoundSource { SourceId = SourceId, TotalSeconds = 123 };

        var fileName = $"test-AUDIO.{extension}";
        var filePath = $"C:/dir-A/{fileName}";

        this.mockPath.GetExtension(Arg.Any<string>()).Returns(extension);
        this.mockDeviceManager.IsInitialized.Returns(isInitialized);
        this.mockAudioDecoder.TotalSeconds.Returns(123);

        var sut = CreateSystemUnderTest();

        // Act
        var actualFirstInvoke = sut.Init(filePath);
        var actualSecondInvoke = sut.Init(filePath);

        // Assert
        actualFirstInvoke.Should().Be(SourceId);
        actualSecondInvoke.Should().Be(SourceId);
        this.mockPath.Received(1).GetExtension(filePath);
        this.mockDeviceManager.Received(isInitialized ? 0 : 1).InitDevice();
        this.mockDeviceManager.Received(1).InitSound(1);
        this.mockDeviceManager.Received(1).UpdateSoundSource(expectedSoundSrc);
    }

    [Fact]
    public void Upload_WhenBufferIsNotInitialized_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act
        var act = () => sut.Upload();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("The buffer has not been initialized.");
    }

    [Fact]
    public void Upload_WithInvalidAudioFormatType_ThrowsException()
    {
        // Arrange
        var expectedMsg = "The audio format type is not supported.";
        expectedMsg += "\nSupported audio format types: .mp3, .ogg";

        this.mockAudioDecoder.Format.Returns((ALFormat)1234);

        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Act
        var act = () => sut.Upload();

        // Assert
        act.Should().Throw<AudioException>().WithMessage(expectedMsg);
    }

    [Fact]
    public void Upload_WhenUploadingOggAudioData_UploadsAudioData()
    {
        // Arrange
        var bufferData = new[] { 10f, 20f, 30f };

        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");
        this.mockDeviceManager.InitSound(Arg.Any<int>()).Returns((SourceId, this.buffers));
        this.mockAudioDecoder.GetSampleData<float>().Returns(bufferData);
        this.mockAudioDecoder.SampleRate.Returns(41000);
        this.mockAudioDecoder.Format.Returns(ALFormat.StereoFloat32Ext);

        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Act
        sut.Upload();

        // Assert
        this.mockAudioDecoder.Received(1).ReadAllSamples();
        this.mockAudioDecoder.Received(1).GetSampleData<float>();
        this.mockAudioDecoder.DidNotReceive().GetSampleData<byte>();
        this.mockAlInvoker.Received(1).BufferData(this.buffers[0], ALFormat.StereoFloat32Ext, bufferData, 41000);
        this.mockAlInvoker.Received(1).Source(SourceId, ALSourcei.Buffer, (int)this.buffers[0]);
    }

    [Fact]
    public void Upload_WhenUploadingMp3AudioData_UploadsAudioData()
    {
        // Arrange
        byte[] bufferData =
        [
            10, 20, 30
        ];

        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".mp3");
        this.mockDeviceManager.InitSound(Arg.Any<int>()).Returns((SourceId, this.buffers));
        this.mockAudioDecoder.GetSampleData<byte>().Returns(bufferData);
        this.mockAudioDecoder.SampleRate.Returns(25000);
        this.mockAudioDecoder.Format.Returns(ALFormat.Stereo16);

        var sut = CreateSystemUnderTest();
        sut.Init("test-file.mp3");

        // Act
        sut.Upload();

        // Assert
        this.mockAudioDecoder.Received(1).ReadAllSamples();
        this.mockAudioDecoder.Received(1).GetSampleData<byte>();
        this.mockAudioDecoder.DidNotReceive().GetSampleData<float>();
        this.mockAlInvoker.Received(1).BufferData(this.buffers[0], ALFormat.Stereo16, bufferData, 25000);
        this.mockAlInvoker.Received(1).Source(SourceId, ALSourcei.Buffer, (int)this.buffers[0]);
    }

    [Fact]
    [SuppressMessage("csharpsquid", "S3966", Justification = "Need to execute dispose twice for testing.")]
    public void Dispose_WhenInvoked_DisposesOfBuffer()
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg"); // Must be invoked to generate the source id

        // Act
        sut.Dispose();
        sut.Dispose();

        // Assert
        this.mockAudioCmdUnsubscriber.Received(1).Dispose();
        this.mockPosCmdUnsubscriber.Received(1).Dispose();
        this.mockLoopingUnsubscriber.Received(1).Dispose();
        this.mockAudioDecoder.Received(1).Dispose();
        this.mockAlInvoker.Received(1).SourceStop(SourceId);
        this.mockAlInvoker.Received(1).Source(SourceId, ALSourcei.Buffer, 0);
        this.mockAlInvoker.Received(1).DeleteBuffer(this.buffers[0]);
        this.mockDeviceManager.Received(1).RemoveSoundSource(SourceId);
    }
    #endregion

    #region Reactable Tests
    [Theory]
    [InlineData(SourceId)]
    [InlineData(123456789)]
    public void AudioCmdReactable_WhenSendingPlayCmd_PlaysAudio(uint sourceId)
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Act
        this.audioCmdSubscription.OnReceive(new AudioCommandData { Command = AudioCommands.Play, SourceId = sourceId });

        // Assert
        this.mockAlInvoker.Received(sourceId == SourceId ? 1 : 0).SourcePlay(SourceId);
    }

    [Theory]
    [InlineData(SourceId)]
    [InlineData(123456789)]
    public void AudioCmdReactable_WhenSendingPauseCmd_PausesAudio(uint sourceId)
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Act
        this.audioCmdSubscription.OnReceive(new AudioCommandData { Command = AudioCommands.Pause, SourceId = sourceId });

        // Assert
        this.mockAlInvoker.Received(sourceId == SourceId ? 1 : 0).SourcePause(SourceId);
    }

    [Theory]
    [InlineData(SourceId)]
    [InlineData(123456789)]
    public void AudioCmdReactable_WhenSendingResetCmd_ResetsAudio(uint sourceId)
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Act
        this.audioCmdSubscription.OnReceive(new AudioCommandData { Command = AudioCommands.Reset, SourceId = sourceId });

        // Assert
        this.mockAlInvoker.Received(sourceId == SourceId ? 1 : 0).SourceRewind(SourceId);
    }

    [Theory]
    [InlineData(SourceId)]
    [InlineData(123456789)]
    public void AudioCmdReactable_WhenSendingEnableLoopCmd_EnablesLooping(uint sourceId)
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Act
        this.audioCmdSubscription.OnReceive(new AudioCommandData { Command = AudioCommands.EnableLooping, SourceId = sourceId });

        // Assert
        this.mockAlInvoker.Received(sourceId == SourceId ? 1 : 0).Source(SourceId, ALSourceb.Looping, true);
    }

    [Theory]
    [InlineData(SourceId)]
    [InlineData(123456789)]
    public void AudioCmdReactable_WhenSendingDisableLoopCmd_DisablesLooping(uint sourceId)
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Act
        this.audioCmdSubscription.OnReceive(new AudioCommandData { Command = AudioCommands.DisableLooping, SourceId = sourceId });

        // Assert
        this.mockAlInvoker.Received(sourceId == SourceId ? 1 : 0).Source(SourceId, ALSourceb.Looping, false);
    }

    [Theory]
    [InlineData(SourceId)]
    [InlineData(123456789)]
    public void PositionCmdReactable_WhenSendingSetPositionCommand_SetsPosition(uint sourceId)
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Act
        this.posCmdSubscription.OnReceive(new PosCommandData { PositionSeconds = 123, SourceId = sourceId });

        // Assert
        this.mockAlInvoker.Received(sourceId == SourceId ? 1 : 0).Source(SourceId, ALSourcef.SecOffset, 123);
    }

    [Fact]
    public void LoopReactable_WhenRequestingLoopState_RespondsWithLoopState()
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Act
        this.loopSubscription.OnRespond();

        // Assert
        this.mockAlInvoker.Received(1).GetSource(SourceId, ALSourceb.Looping);
    }

    [Fact]
    public void AudioCmdReactable_WhenUnsubscribingReactable_UnsubscribesSubscription()
    {
        // Arrange
        _ = CreateSystemUnderTest();

        // Act
        this.audioCmdSubscription.OnUnsubscribe();

        // Assert
        this.mockAudioCmdUnsubscriber.Received(1).Dispose();
    }

    [Fact]
    public void PositionCmdReactable_WhenUnsubscribingReactable_UnsubscribesSubscription()
    {
        // Arrange
        _ = CreateSystemUnderTest();

        // Act
        this.posCmdSubscription.OnUnsubscribe();

        // Assert
        this.mockPosCmdUnsubscriber.Received(1).Dispose();
    }

    [Fact]
    public void LoopReactable_WhenUnsubscribingReactable_UnsubscribesSubscription()
    {
        // Arrange
        _ = CreateSystemUnderTest();

        // Act
        this.loopSubscription.OnUnsubscribe();

        // Assert
        this.mockLoopingUnsubscriber.Received(1).Dispose();
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="FullBuffer"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private FullBuffer CreateSystemUnderTest()
        => new (
            this.mockAlInvoker,
            this.mockDeviceManager,
            this.mockAudioDecoder,
            this.mockReactableFactory,
            this.mockPath,
            this.mockFile);
}
