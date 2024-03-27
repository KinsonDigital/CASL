// <copyright file="StreamBufferTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

#pragma warning disable SA1202

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
using CASL.DotnetWrappers;
using CASL.Factories;
using CASL.OpenAL;
using CASL.ReactableData;
using FluentAssertions;
using Helpers;
using NSubstitute;
using Xunit;

/// <summary>
/// Tests the <see cref="StreamBuffer"/> class.
/// </summary>
public class StreamBufferTests
{
    private const uint SourceId = 123;
    private readonly IOpenALInvoker mockAlInvoker;
    private readonly IAudioDeviceManager mockDeviceManager;
    private readonly IAudioDecoder mockAudioDecoder;
    private readonly IStreamBufferManager mockStreamBufferManager;
    private readonly ITaskService mockTaskService;
    private readonly IThreadService mockThreadService;
    private readonly IFile mockFile;
    private readonly IPath mockPath;
    private readonly IReactableFactory mockReactableFactory;
    private readonly IDisposable mockAudioCmdUnsubscriber;
    private readonly IDisposable mockPosCmdUnsubscriber;
    private readonly IDisposable mockLoopingUnsubscriber;
    private readonly uint[] bufferIds =
    [
        100u,
        200u,
        300u,
        400u,
    ];
    private IReceiveSubscription<AudioCommandData> audioCmdSubscription;
    private IReceiveSubscription<PosCommandData> posCmdSubscription;
    private IRespondSubscription<bool> loopSubscription;
    private Action? streamDataDelegate;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamBufferTests"/> class.
    /// </summary>
    public StreamBufferTests()
    {
        this.mockAlInvoker = Substitute.For<IOpenALInvoker>();
        this.mockAlInvoker.GenSource().Returns(SourceId);
        this.mockAlInvoker.GenBuffers(Arg.Any<int>()).Returns(this.bufferIds);

        this.mockDeviceManager = Substitute.For<IAudioDeviceManager>();

        this.mockAudioDecoder = Substitute.For<IAudioDecoder>();
        this.mockStreamBufferManager = Substitute.For<IStreamBufferManager>();

        this.audioCmdSubscription = Substitute.For<IReceiveSubscription<AudioCommandData>>();
        this.mockAudioCmdUnsubscriber = Substitute.For<IDisposable>();
        var mockAudioCmdReactable = Substitute.For<IPushReactable<AudioCommandData>>();
        mockAudioCmdReactable.Subscribe(Arg.Any<IReceiveSubscription<AudioCommandData>>())
            .Returns(this.mockAudioCmdUnsubscriber)
            .AndDoes(callInfo => this.audioCmdSubscription = callInfo.Arg<IReceiveSubscription<AudioCommandData>>());

        this.posCmdSubscription = Substitute.For<IReceiveSubscription<PosCommandData>>();
        this.mockPosCmdUnsubscriber = Substitute.For<IDisposable>();
        var mockPosCmdReactable = Substitute.For<IPushReactable<PosCommandData>>();
        mockPosCmdReactable.Subscribe(Arg.Any<IReceiveSubscription<PosCommandData>>())
            .Returns(this.mockPosCmdUnsubscriber)
            .AndDoes(callInfo => this.posCmdSubscription = callInfo.Arg<IReceiveSubscription<PosCommandData>>());

        this.loopSubscription = Substitute.For<IRespondSubscription<bool>>();
        this.mockLoopingUnsubscriber = Substitute.For<IDisposable>();
        var mockLoopingReactable = Substitute.For<IPullReactable<bool>>();
        mockLoopingReactable.Subscribe(Arg.Any<IRespondSubscription<bool>>())
            .Returns(this.mockLoopingUnsubscriber)
            .AndDoes(callInfo => this.loopSubscription = callInfo.Arg<IRespondSubscription<bool>>());

        this.mockReactableFactory = Substitute.For<IReactableFactory>();
        this.mockReactableFactory.CreateAudioCmndReactable().Returns(mockAudioCmdReactable);
        this.mockReactableFactory.CreatePositionCmndReactable().Returns(mockPosCmdReactable);
        this.mockReactableFactory.CreateIsLoopingReactable().Returns(mockLoopingReactable);

        this.mockTaskService = Substitute.For<ITaskService>();
        this.mockTaskService.SetAction(Arg.Any<Action>());
        this.mockTaskService
            .When(service => service.SetAction(Arg.Any<Action>()))
            .Do(callInfo =>
            {
                this.streamDataDelegate = callInfo.Arg<Action>();
                this.streamDataDelegate.Should().NotBeNull();
            });

        this.mockThreadService = Substitute.For<IThreadService>();

        this.mockPath = Substitute.For<IPath>();
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");

        this.mockFile = Substitute.For<IFile>();
        this.mockFile.Exists(Arg.Any<string>()).Returns(true);
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullAlInvokerParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new StreamBuffer(
                null,
                this.mockDeviceManager,
                this.mockAudioDecoder,
                this.mockStreamBufferManager,
                this.mockReactableFactory,
                this.mockTaskService,
                this.mockThreadService,
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
            _ = new StreamBuffer(
                this.mockAlInvoker,
                null,
                this.mockAudioDecoder,
                this.mockStreamBufferManager,
                this.mockReactableFactory,
                this.mockTaskService,
                this.mockThreadService,
                this.mockPath,
                this.mockFile);
        };

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("audioDeviceManager");
    }
    #endregion

    #region Prop Tests
    [Fact]
    public void Position_WhenGettingValue_ReturnsCorrectResult()
    {
        // Arrange
        this.mockStreamBufferManager.ToPositionSeconds(Arg.Any<long>(), Arg.Any<float>()).Returns(50);

        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.Position;

        // Assert
        actual.TotalSeconds.Should().Be(50);
    }
    #endregion

    #region Method Tests
    [Fact]
    public void Init_WithNullFilePathParam_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act
        var act = () => sut.Init(null);

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("filePath");
    }

    [Fact]
    public void Init_WithEmptyFilePathParam_ThrowsException()
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
        var act = () => sut.Init("test-file.ogg");

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage("The audio file could not be found.")
            .And.FileName.Should().Be("test-file.ogg");
    }

    [Fact]
    public void Init_WithUnsupportedFileExtensionAndFormat_ThrowsException()
    {
        // Arrange
        const string expected = "The file extension '.mp4' is not supported. Supported extensions are '.ogg' and '.mp3'. (Parameter 'filePath')";
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".mp4");
        var sut = CreateSystemUnderTest();

        // Act
        var act = () => sut.Init("test-file.mp4");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage(expected);
    }

    [Theory]
    [InlineData(".mp3")]
    [InlineData(".ogg")]
    public void Init_WhenInvoked_InitializesBuffer(string extension)
    {
        // Arrange
        var fileName = $"test-file{extension}";

        this.mockPath.GetExtension(Arg.Any<string>()).Returns(extension);
        this.mockAudioDecoder.TotalSeconds.Returns(298);

        var sut = CreateSystemUnderTest();

        // Act
        var actualSrcId = sut.Init(fileName);

        // Assert
        actualSrcId.Should().Be(SourceId);
        this.mockPath.Received(1).GetExtension(fileName);
    }

    [Fact]
    public void Upload_WhenNotInitialized_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act
        var act = () => sut.Upload();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The buffer has not been initialized.");
    }

    [Fact]
    public void Upload_WhenUploadingOggData_StartsStreamingOggData()
    {
        // Arrange
        var expectedBufferStats = new BufferStats
        {
            SourceId = SourceId,
            FormatType = AudioFormatType.Ogg,
            DecoderFormat = ALFormat.StereoFloat32Ext,
            SampleRate = 41_000,
            TotalChannels = 2,
        };

        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");
        this.mockAudioDecoder.Format.Returns(ALFormat.StereoFloat32Ext);
        this.mockAudioDecoder.SampleRate.Returns(41_000);
        this.mockAudioDecoder.TotalChannels.Returns(2);

        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Act
        sut.Upload();

        // Assert
        this.mockStreamBufferManager.Received(1).FillBuffersFromStart(
            expectedBufferStats,
            Arg.Do<uint[]>(bufferIdsArg => bufferIdsArg.Should().BeEquivalentTo(this.bufferIds)),
            this.mockAudioDecoder.Flush,
            Arg.Any<Func<float[]>>());
        this.mockTaskService.Received(1).SetAction(Arg.Any<Action>());
        this.mockTaskService.Received(1).Start();
    }

    [Fact]
    public void Upload_WhenUploadingMp3Data_StartsStreamingMp3Data()
    {
        // Arrange
        var expectedBufferStats = new BufferStats
        {
            SourceId = SourceId,
            FormatType = AudioFormatType.Mp3,
            DecoderFormat = ALFormat.Stereo16,
            SampleRate = 20_000,
            TotalChannels = 1,
        };

        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".mp3");
        this.mockAudioDecoder.Format.Returns(ALFormat.Stereo16);
        this.mockAudioDecoder.SampleRate.Returns(20_000);
        this.mockAudioDecoder.TotalChannels.Returns(1);

        var sut = CreateSystemUnderTest();
        sut.Init("test-file.mp3");

        // Act
        sut.Upload();

        // Assert
        this.mockStreamBufferManager.Received(1).FillBuffersFromStart(
            expectedBufferStats,
            Arg.Do<uint[]>(bufferIdsArg => bufferIdsArg.Should().BeEquivalentTo(this.bufferIds)),
            this.mockAudioDecoder.Flush,
            Arg.Any<Func<byte[]>>());
        this.mockTaskService.Received(1).SetAction(Arg.Any<Action>());
        this.mockTaskService.Received(1).Start();
    }

    [Theory]
    [InlineData(ALSourceState.Paused)]
    [InlineData(ALSourceState.Initial)]
    [InlineData(ALSourceState.Stopped)]
    internal void Upload_WhenNotPlayingAudio_DoesNotStreamBufferData(ALSourceState srcState)
    {
        // Arrange
        var isCancelRequested = false;

        // Make sure that the reset process does not occur
        this.mockStreamBufferManager.ToPositionSeconds(Arg.Any<long>(), Arg.Any<float>()).Returns(50f);
        this.mockAudioDecoder.TotalSeconds.Returns(100f);

        this.mockAlInvoker.GetSourceState(Arg.Any<uint>()).Returns(srcState);

        this.mockTaskService.When(x => x.SetAction(Arg.Any<Action>()))
            .Do(cb => this.streamDataDelegate = cb.Arg<Action>());
        this.mockTaskService.When(x => x.Start()).Do(_ => this.streamDataDelegate?.Invoke());
        this.mockTaskService.IsCancellationRequested.Returns(_ => isCancelRequested);

        // Prevent an infinite loop from occurring
        this.mockThreadService
            .When(x => x.Sleep(Arg.Any<int>()))
            .Do(_ => isCancelRequested = true);

        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Enable looping
        this.audioCmdSubscription.OnReceive(new AudioCommandData { Command = AudioCommands.EnableLooping, SourceId = SourceId });

        // Act
        sut.Upload();

        // Assert
        this.mockStreamBufferManager.DidNotReceive().ManageBuffers(Arg.Any<BufferStats>(), Arg.Any<Func<float[]>>());
        this.mockAlInvoker.Received(1).GetSourceState(SourceId);

        // Verify that the reset process has NOT been performed
        this.mockStreamBufferManager.DidNotReceive().UnqueueProcessedBuffers(SourceId);
        this.mockStreamBufferManager.FillBuffersFromStart(Arg.Any<BufferStats>(), this.bufferIds, this.mockAudioDecoder.Flush, Arg.Any<Func<float[]>>());
        this.mockAlInvoker.DidNotReceive().SourceRewind(SourceId);
        this.mockStreamBufferManager.DidNotReceive().ResetSamplePos();

        // Verify that the playback has been started again due to looping being enabled
        this.mockAlInvoker.DidNotReceive().SourcePlay(SourceId);

        this.mockThreadService.DidNotReceive().Sleep(100);
    }

    [Fact]
    public void Upload_WhenPlayingOggAudio_StreamsBufferData()
    {
        // Arrange
        var isCancelRequested = false;
        var taskServiceIsRunning = false;

        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");

        // Make sure that the reset process occurs
        this.mockStreamBufferManager.ToPositionSeconds(Arg.Any<long>(), Arg.Any<float>()).Returns(100f);
        this.mockAudioDecoder.TotalSeconds.Returns(50f);

        this.mockAlInvoker.GetSource(Arg.Any<uint>(), ALSourcef.Pitch).Returns(1f);
        this.mockAlInvoker.GetSourceState(Arg.Any<uint>()).Returns(ALSourceState.Playing);

        this.mockTaskService.When(x => x.SetAction(Arg.Any<Action>()))
            .Do(cb => this.streamDataDelegate = cb.Arg<Action>());
        this.mockTaskService.When(x => x.Start()).Do(cb =>
        {
            cb.Should().NotBeNull("it is required for mocking the task service.");
            this.streamDataDelegate?.Invoke();
            taskServiceIsRunning = true;
        });
        this.mockTaskService.IsCancellationRequested.Returns(_ => isCancelRequested);
        this.mockTaskService.IsRunning.Returns((_) => taskServiceIsRunning);

        // Execute the internal read sample data delegate
        this.mockStreamBufferManager.ManageBuffers(
            Arg.Any<BufferStats>(),
            Arg.Do<Func<float[]>>(arg => arg()));

        // Prevent an infinite loop from occurring
        this.mockThreadService
            .When(x => x.Sleep(Arg.Any<int>()))
            .Do(_ => isCancelRequested = true);

        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Enable looping
        this.audioCmdSubscription.OnReceive(new AudioCommandData { Command = AudioCommands.EnableLooping, SourceId = SourceId });

        // Act
        sut.Upload();
        sut.Upload();

        // Assert
        this.mockAlInvoker.Received(1).GetSourceState(SourceId);
        this.mockStreamBufferManager.Received(1).ManageBuffers(Arg.Any<BufferStats>(), Arg.Any<Func<float[]>>());
        this.mockAudioDecoder.Received(1).ReadSamples();
        this.mockAudioDecoder.Received(1).GetSampleData<float>();

        // Verify that the reset process has been performed
        this.mockStreamBufferManager.Received(1).UnqueueProcessedBuffers(SourceId);
        this.mockStreamBufferManager.FillBuffersFromStart(
            Arg.Any<BufferStats>(),
            this.bufferIds,
            this.mockAudioDecoder.Flush,
            Arg.Any<Func<float[]>>());
        this.mockAlInvoker.Received(1).SourceRewind(SourceId);
        this.mockStreamBufferManager.Received(1).ResetSamplePos();

        // Verify that the playback has been started again due to looping being enabled
        this.mockAlInvoker.Received(1).SourcePlay(SourceId);

        this.mockThreadService.Received(1).Sleep(100);

        this.mockTaskService.Received(1).SetAction(Arg.Any<Action>());
        this.mockTaskService.Received(1).Start();
    }

    [Fact]
    public void Upload_WhenPlayingMp3Audio_StreamsBufferData()
    {
        // Arrange
        var isCancelRequested = false;

        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".mp3");

        // Make sure that the reset process occurs
        this.mockStreamBufferManager.ToPositionSeconds(Arg.Any<long>(), Arg.Any<float>()).Returns(100f);
        this.mockAudioDecoder.TotalSeconds.Returns(50f);

        this.mockAlInvoker.GetSource(Arg.Any<uint>(), ALSourcef.Pitch).Returns(1f);
        this.mockAlInvoker.GetSourceState(Arg.Any<uint>()).Returns(ALSourceState.Playing);

        this.mockTaskService.When(x => x.SetAction(Arg.Any<Action>()))
            .Do(cb => this.streamDataDelegate = cb.Arg<Action>());
        this.mockTaskService.When(x => x.Start()).Do(_ => this.streamDataDelegate?.Invoke());
        this.mockTaskService.IsCancellationRequested.Returns(_ => isCancelRequested);

        // Execute the internal read sample data delegate
        this.mockStreamBufferManager.ManageBuffers(
            Arg.Any<BufferStats>(),
            Arg.Do<Func<byte[]>>(arg => arg()));

        // Prevent an infinite loop from occurring
        this.mockThreadService
            .When(x => x.Sleep(Arg.Any<int>()))
            .Do(_ => isCancelRequested = true);

        var sut = CreateSystemUnderTest();
        sut.Init("test-file.mp3");

        // Enable looping
        this.audioCmdSubscription.OnReceive(new AudioCommandData { Command = AudioCommands.EnableLooping, SourceId = SourceId });

        // Act
        sut.Upload();

        // Assert
        this.mockAlInvoker.Received(1).GetSourceState(SourceId);
        this.mockStreamBufferManager.Received(1).ManageBuffers(Arg.Any<BufferStats>(), Arg.Any<Func<byte[]>>());
        this.mockAudioDecoder.Received(1).ReadSamples();
        this.mockAudioDecoder.Received(1).GetSampleData<byte>();

        // Verify that the reset process has been performed
        this.mockStreamBufferManager.Received(1).UnqueueProcessedBuffers(SourceId);
        this.mockStreamBufferManager.FillBuffersFromStart(
            Arg.Any<BufferStats>(),
            this.bufferIds,
            this.mockAudioDecoder.Flush,
            Arg.Any<Func<byte[]>>());
        this.mockAlInvoker.Received(1).SourceRewind(SourceId);
        this.mockStreamBufferManager.Received(1).ResetSamplePos();

        // Verify that the playback has been started again due to looping being enabled
        this.mockAlInvoker.Received(1).SourcePlay(SourceId);

        this.mockThreadService.Received(1).Sleep(100);
    }

    [Fact]
    [SuppressMessage("csharpsquid", "S3966", Justification = "Need to execute dispose twice for testing.")]
    public void Dispose_WhenInvoked_DisposesOfStreamBuffer()
    {
        // Arrange
        // This prevents an infinite loop from occurring
        this.mockTaskService.IsCompletedSuccessfully.Returns(true);

        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Act
        sut.Dispose();
        sut.Dispose();
        this.mockDeviceManager.DeviceChanging += Raise.EventWith(sut, EventArgs.Empty);
        var changingInvoked = sut.GetBoolFieldValue("audioDeviceChanging");

        // Set the private value to true to verify if the changed event has been invoked
        sut.SetBoolField("audioDeviceChanging", true);

        this.mockDeviceManager.DeviceChanged += Raise.EventWith(sut, EventArgs.Empty);

        var changedInvoked = sut.GetBoolFieldValue("audioDeviceChanging");

        // Assert
        changingInvoked.Should().BeFalse("the device changing event should not have been invoked.");
        changedInvoked.Should().BeTrue("the device changed event should not have been invoked.");

        this.mockTaskService.Received(1).Cancel();
        this.mockTaskService.Received(1).Dispose();
        this.mockAudioCmdUnsubscriber.Received(1).Dispose();
        this.mockPosCmdUnsubscriber.Received(1).Dispose();
        this.mockLoopingUnsubscriber.Received(1).Dispose();
        this.mockAudioDecoder.Received(1).Dispose();
        this.mockAlInvoker.Received(1).SourceStop(SourceId);
        this.mockAlInvoker.Received(1).Source(SourceId, ALSourcei.Buffer, 0);
        this.mockAlInvoker.Received(1).DeleteBuffer(this.bufferIds[0]);
        this.mockAlInvoker.Received(1).DeleteBuffer(this.bufferIds[1]);
        this.mockAlInvoker.Received(1).DeleteBuffer(this.bufferIds[2]);
        this.mockAlInvoker.Received(1).DeleteBuffer(this.bufferIds[3]);
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

    [Fact]
    public void AudioCmdReactable_WhenSendingResetCmdForMp3Data_ResetsDecoderAndBuffers()
    {
        // Arrange
        var expectedBufferStats = new BufferStats
        {
            SourceId = SourceId,
            FormatType = AudioFormatType.Mp3,
            DecoderFormat = ALFormat.StereoFloat32Ext,
            SampleRate = 41_000,
            TotalChannels = 2,
        };

        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".mp3");

        this.mockAudioDecoder.Format.Returns(ALFormat.StereoFloat32Ext);
        this.mockAudioDecoder.SampleRate.Returns(41_000);
        this.mockAudioDecoder.TotalChannels.Returns(2);

        var sut = CreateSystemUnderTest();
        sut.Init($"test-file.mp3");

        // Act
        this.audioCmdSubscription.OnReceive(new AudioCommandData { Command = AudioCommands.Reset, SourceId = SourceId });

        // Assert
        this.mockStreamBufferManager.Received(1).UnqueueProcessedBuffers(SourceId);

        this.mockStreamBufferManager.Received(1).FillBuffersFromStart(
            expectedBufferStats,
            Arg.Do<uint[]>(bufferIdsArg => bufferIdsArg.Should().BeEquivalentTo(this.bufferIds)),
            this.mockAudioDecoder.Flush,
            Arg.Any<Func<byte[]>>());
        this.mockAlInvoker.Received(1).SourceRewind(SourceId);
        this.mockStreamBufferManager.Received(1).ResetSamplePos();
    }

    [Fact]
    public void AudioCmdReactable_WhenSendingResetCmdForOggData_ResetsDecoderAndBuffers()
    {
        // Arrange
        var expectedBufferStats = new BufferStats
        {
            SourceId = SourceId,
            FormatType = AudioFormatType.Ogg,
            DecoderFormat = ALFormat.StereoFloat32Ext,
            SampleRate = 41_000,
            TotalChannels = 2,
        };

        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");

        this.mockAudioDecoder.Format.Returns(ALFormat.StereoFloat32Ext);
        this.mockAudioDecoder.SampleRate.Returns(41_000);
        this.mockAudioDecoder.TotalChannels.Returns(2);

        var sut = CreateSystemUnderTest();
        sut.Init($"test-file.ogg");

        // Act
        this.audioCmdSubscription.OnReceive(new AudioCommandData { Command = AudioCommands.Reset, SourceId = SourceId });

        // Assert
        this.mockStreamBufferManager.Received(1).UnqueueProcessedBuffers(SourceId);
        this.mockStreamBufferManager.Received(1).FillBuffersFromStart(
            expectedBufferStats,
            Arg.Do<uint[]>(bufferIdsArg => bufferIdsArg.Should().BeEquivalentTo(this.bufferIds)),
            this.mockAudioDecoder.Flush,
            Arg.Any<Func<float[]>>());
        this.mockAlInvoker.Received(1).SourceRewind(SourceId);
        this.mockStreamBufferManager.Received(1).ResetSamplePos();
    }

    [Fact]
    public void AudioCmdReactable_WhenUnsubscribing_UnsubscribesFromReactable()
    {
        // Arrange
        _ = CreateSystemUnderTest();

        // Act
        this.audioCmdSubscription.OnUnsubscribe();

        // Assert
        this.mockAudioCmdUnsubscriber.Received(1).Dispose();
    }

    [Theory]
    [InlineData(AudioCommands.EnableLooping, true)]
    [InlineData(AudioCommands.DisableLooping, false)]
    internal void AudioCmdReactable_WhenSendingLoopCmd_EnablesLooping(AudioCommands audioCmd, bool expected)
    {
        // Arrange
        this.mockAudioDecoder.TotalSamples.Returns(1);
        this.mockAudioDecoder.TotalChannels.Returns(1);
        this.mockAudioDecoder.TotalSeconds.Returns(1);

        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Force the setting to be the opposite of the expected value to make sure that the default value of
        // the bool IsLooping does not result in a false positive result
        var oppositeAudioCmd = audioCmd == AudioCommands.EnableLooping ? AudioCommands.DisableLooping : AudioCommands.EnableLooping;
        this.audioCmdSubscription.OnReceive(new AudioCommandData { Command = oppositeAudioCmd, SourceId = SourceId });

        // Act
        this.audioCmdSubscription.OnReceive(new AudioCommandData { Command = audioCmd, SourceId = SourceId });

        // Assert
        sut.IsLooping.Should().Be(expected);
    }

    [Fact]
    public void PosCmdReactable_WithIncorrectSourceId_DoesNotSetPosition()
    {
        // Arrange
        this.mockAudioDecoder.TotalSamples.Returns(100);
        this.mockAudioDecoder.TotalChannels.Returns(2);

        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Act
        this.posCmdSubscription.OnReceive(new PosCommandData { SourceId = 710u, PositionSeconds = 379f });

        // Assert
        this.mockAlInvoker.DidNotReceive().SourcePlay(Arg.Any<uint>());
        this.mockStreamBufferManager.DidNotReceive()
            .ToPositionSamples(Arg.Any<float>(), Arg.Any<float>(), Arg.Any<long>());
        this.mockAlInvoker.DidNotReceive().GetSourceState(Arg.Any<uint>());
        this.mockAlInvoker.DidNotReceive().SourceStop(Arg.Any<uint>());
        this.mockAudioDecoder.DidNotReceive().ReadUpTo(Arg.Any<uint>());
        this.mockStreamBufferManager.DidNotReceive().SetSamplePos(Arg.Any<long>());
    }

    [Fact]
    public void PosCmdReactable_WhenNotInitialized_DoesNotSetPosition()
    {
        // Arrange
        this.mockAudioDecoder.TotalSamples.Returns(100);
        this.mockAudioDecoder.TotalChannels.Returns(2);

        _ = CreateSystemUnderTest();

        // Act
        this.posCmdSubscription.OnReceive(new PosCommandData { SourceId = 710u, PositionSeconds = 379f });

        // Assert
        this.mockAlInvoker.DidNotReceive().SourcePlay(Arg.Any<uint>());
        this.mockStreamBufferManager.DidNotReceive()
            .ToPositionSamples(Arg.Any<float>(), Arg.Any<float>(), Arg.Any<long>());
        this.mockAlInvoker.DidNotReceive().GetSourceState(Arg.Any<uint>());
        this.mockAlInvoker.DidNotReceive().SourceStop(Arg.Any<uint>());
        this.mockAudioDecoder.DidNotReceive().ReadUpTo(Arg.Any<uint>());
        this.mockStreamBufferManager.DidNotReceive().SetSamplePos(Arg.Any<long>());
    }

    [Theory]
    [InlineData(200f, 100f, AudioCommands.EnableLooping)]
    [InlineData(200f, 100f, AudioCommands.DisableLooping)]
    internal void PosCmdReactable_WhenEndOfAudioIsReached_RefillsBuffersAndPossiblyRestartsPlayback(
        float posSeconds,
        float totalSeconds,
        AudioCommands loopState)
    {
        // Arrange
        this.mockAudioDecoder.TotalSeconds.Returns(totalSeconds);

        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");

        // Act
        this.audioCmdSubscription.OnReceive(new AudioCommandData { SourceId = SourceId, Command = loopState });
        this.posCmdSubscription.OnReceive(new PosCommandData { SourceId = SourceId, PositionSeconds = posSeconds });

        // Assert
        this.mockStreamBufferManager.Received(1).FillBuffersFromStart(
            Arg.Any<BufferStats>(),
            Arg.Any<uint[]>(),
            Arg.Any<Action>(),
            Arg.Any<Func<float[]>>());

        this.mockAlInvoker.Received(loopState == AudioCommands.EnableLooping ? 1 : 0).SourcePlay(SourceId);
    }

    [Theory]
    [InlineData(".mp3", 481L, 1924u, ALSourceState.Paused)]
    [InlineData(".mp3", 481L, 1924u, ALSourceState.Playing)]
    [InlineData(".ogg", 481L, 962u, ALSourceState.Paused)]
    [InlineData(".ogg", 481L, 962u, ALSourceState.Playing)]
    internal void PosCmdReactable_WhenSendingSetPositionNotification_SetsAudioPosition(
        string extension,
        long samplePos,
        uint readUpToPos,
        ALSourceState sourceState)
    {
        // Arrange
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(extension);
        this.mockAudioDecoder.TotalSeconds.Returns(100f);
        this.mockAudioDecoder.TotalSampleFrames.Returns(200);
        this.mockAlInvoker.GetSourceState(Arg.Any<uint>()).Returns(sourceState);
        this.mockStreamBufferManager
            .ToPositionSamples(Arg.Any<float>(), Arg.Any<float>(), Arg.Any<long>())
            .Returns(samplePos);

        var sut = CreateSystemUnderTest();
        sut.Init($"test-file{extension}");

        // Act
        this.audioCmdSubscription.OnReceive(new AudioCommandData { SourceId = SourceId, Command = AudioCommands.DisableLooping });
        this.posCmdSubscription.OnReceive(new PosCommandData { SourceId = SourceId, PositionSeconds = 10f });

        // Assert
        this.mockStreamBufferManager.Received(1).ToPositionSamples(10f, 100f, 200);
        this.mockAlInvoker.Received(1).GetSourceState(SourceId);
        this.mockAlInvoker.Received(1).SourceStop(SourceId);
        this.mockAudioDecoder.Received(1).ReadUpTo(readUpToPos);
        this.mockStreamBufferManager.Received(1).SetSamplePos(samplePos);
        this.mockAlInvoker.Received(sourceState == ALSourceState.Playing ? 1 : 0).SourcePlay(SourceId);
    }

    [Fact]
    public void PosCmdReactable_WhenUnsubscribing_UnsubscribesFromReactable()
    {
        // Arrange
        _ = CreateSystemUnderTest();

        // Act
        this.posCmdSubscription.OnUnsubscribe();

        // Assert
        this.mockPosCmdUnsubscriber.Received(1).Dispose();
    }

    [Fact]
    public void LoopingReactable_WhenUnsubscribing_UnsubscribesFromReactable()
    {
        // Arrange
        _ = CreateSystemUnderTest();

        // Act
        this.loopSubscription.OnUnsubscribe();

        // Assert
        this.mockLoopingUnsubscriber.Received(1).Dispose();
    }

    [Theory]
    [InlineData(AudioCommands.EnableLooping, true)]
    [InlineData(AudioCommands.DisableLooping, false)]
    internal void LoopingReactable_WhenRequestingLoopState_ReturnsCorrectResult(AudioCommands cmd, bool expected)
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        sut.Init("test-file.ogg");
        this.audioCmdSubscription.OnReceive(new AudioCommandData { Command = cmd, SourceId = SourceId });

        // Act
        var actual = this.loopSubscription.OnRespond();

        // Assert
        actual.Should().Be(expected);
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="StreamBuffer"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private StreamBuffer CreateSystemUnderTest()
        => new (
            this.mockAlInvoker,
            this.mockDeviceManager,
            this.mockAudioDecoder,
            this.mockStreamBufferManager,
            this.mockReactableFactory,
            this.mockTaskService,
            this.mockThreadService,
            this.mockPath,
            this.mockFile);
}
