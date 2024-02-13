// <copyright file="AudioDeviceManagerTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

#pragma warning disable IDE0002 // Name can be simplified
namespace CASLTests.Devices;

#pragma warning disable IDE0001 // Name can be simplified
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Linq;
using CASL;
using CASL.Data;
using CASL.Data.Exceptions;
using CASL.Devices;
using CASL.Devices.Exceptions;
using CASL.OpenAL;
using Moq;
using Xunit;
using FluentAssertions;
using Silk.NET.OpenAL;

#pragma warning restore IDE0001 // Name can be simplified

/// <summary>
/// Tests the <see cref="AudioDeviceManager"/> class.
/// </summary>
public class AudioDeviceManagerTests
{
    private const string OggFileExtension = ".ogg";
    private const string IsDisposedExceptionMessage = $"The '{nameof(AudioDeviceManager)}' has not been initialized.\nInvoked the '{nameof(AudioDeviceManager.InitDevice)}()' to initialize the device manager.";
    private readonly string oggFilePath;
    private readonly Mock<IOpenALInvoker> mockALInvoker;
    private readonly Mock<IPath> mockPath;
    private readonly Context context;
    private readonly uint srcId = 4321;
    private readonly uint bufferId = 9876;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDeviceManagerTests"/> class.
    /// </summary>
    public AudioDeviceManagerTests()
    {
        this.oggFilePath = @"C:/temp/Content/Sounds/sound.ogg";

        var device = new Device();
        this.context = new Context();

        this.mockALInvoker = new Mock<IOpenALInvoker>();

        MockSoundLength(60);

        this.mockALInvoker.Setup(m => m.GenSource()).Returns(this.srcId);
        this.mockALInvoker.Setup(m => m.GenBuffer()).Returns(this.bufferId);
        this.mockALInvoker.Setup(m => m.OpenDevice(It.IsAny<string>())).Returns(device);
        this.mockALInvoker.Setup(m => m.CreateContext(device, It.IsAny<ALContextAttributes>()))
            .Returns(this.context);
        this.mockALInvoker.Setup(m => m.MakeContextCurrent(this.context)).Returns(true);

        this.mockPath = new Mock<IPath>();
        this.mockPath.Setup(m => m.GetExtension(It.IsAny<string?>())).Returns(OggFileExtension);
    }

    #region Prop Tests
    [Fact]
    public void IsInitialized_WhenGettingValueAfterInitialization_ReturnsTrue()
    {
        // Arrange
        var manager = CreateManager();
        manager.InitDevice();

        // Act
        var actual = manager.IsInitialized;

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void GetDeviceNames_WhenGettingValueAfterBeingDisposed_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        manager.Dispose();

        // Act
        var action = manager.GetDeviceNames;

        // Assert
        action.Should().Throw<AudioDeviceManagerNotInitializedException>().WithMessage(IsDisposedExceptionMessage);
    }

    [Fact]
    public void GetDeviceNames_WhenGettingValueBeforeBeingDisposed_ReturnsCorrectResult()
    {
        // Arrange
        var expected = new[] { "Device-1", "Device-2" };
        var manager = CreateManager();
        manager.InitDevice();
        this.mockALInvoker.Setup(m => m.GetDeviceList())
            .Returns(() => new[] { "Device-1", "Device-2" });

        // Act
        var actual = manager.GetDeviceNames().ToArray();

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void AdditionalAttributes_WithNullValue_ReturnsCorrectResult()
    {
        // Arrange
        var attributes = new ALContextAttributes();

        // Act
        attributes.AdditionalAttributes = null;
        var actual = attributes.AdditionalAttributes;

        // Assert
        actual.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void AdditionalAttributes_WithNonNullValue_ReturnsCorrectResult()
    {
        // Arrange
        var attributes = new ALContextAttributes();

        // Act
        attributes.AdditionalAttributes = new[] { 111, 222 };
        var actual = attributes.AdditionalAttributes;

        // Assert
        actual.Should().NotBeNull().And.HaveCount(2).And.ContainInOrder(111, 222);
    }
    #endregion

    #region Method Tests
    [Fact]
    public void InitDevice_WhenInvoked_InitializesDevice()
    {
        // Arrange
        this.mockALInvoker.Setup(m => m.GetDefaultDevice()).Returns("OpenAL Soft on test-device");
        var manager = CreateManager();

        // Act
        manager.InitDevice("test-device");

        // Assert
        this.mockALInvoker.Verify(m => m.OpenDevice("OpenAL Soft on test-device"), Times.Once());
        this.mockALInvoker.Verify(m => m.MakeContextCurrent(this.context), Times.Once());
        this.mockALInvoker.Verify(m => m.GetDefaultDevice(), Times.Once());
        manager.DeviceInUse.Should().Be("OpenAL Soft on test-device");
    }

    [Fact]
    public void InitDevice_WithIssueMakingContextCurrent_ThrowsException()
    {
        // Arrange
        // The MakeContextCurrent call does not take nullable bool.  This fixes that issue
        this.mockALInvoker.Setup(m => m.MakeContextCurrent(this.context)).Returns(false);
        var manager = CreateManager();

        // Act
        var action = () => manager.InitDevice("test-device");

        // Assert
        action.Should().Throw<InitializeDeviceException>().WithMessage("There was an issue initializing the audio device.");
    }

    [Fact]
    public void InitSound_WithSingleParamAndWhileDisposed_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        manager.Dispose();

        // Act
        var action = manager.InitSound;

        // Assert
        action.Should().Throw<AudioDeviceManagerNotInitializedException>().WithMessage(IsDisposedExceptionMessage);
    }

    [Fact]
    public void InitSound_WhenInvoked_SetsUpSoundAndReturnsCorrectResult()
    {
        // Arrange
        var manager = CreateManager();
        manager.InitDevice();

        // Act
        var (actualSourceId, actualBufferId) = manager.InitSound();

        // Assert
        actualSourceId.Should().Be(this.srcId);
        actualBufferId.Should().Be(this.bufferId);
        this.mockALInvoker.Verify(m => m.GenSource(), Times.Once());
        this.mockALInvoker.Verify(m => m.GenBuffer(), Times.Once());
    }

    [Fact]
    public void UpdateSoundSource_WhenNotInitialized_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();

        // Act
        var action = () => manager.UpdateSoundSource(It.IsAny<SoundSource>());

        // Assert
        action.Should().Throw<AudioDeviceManagerNotInitializedException>().WithMessage(IsDisposedExceptionMessage);
    }

    [Fact]
    public void UpdateSoundSource_WhenSoundSourceDoesNotExist_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        manager.InitDevice();

        // Act
        var action = () =>
        {
            var soundSrc = new SoundSource
            {
                SourceId = 1234,
            };
            manager.UpdateSoundSource(soundSrc);
        };

        // Assert
        var expectedExceptionMessage = $"The sound source with the source id '1234' does not exist.";
        action.Should().Throw<SoundDataException>().WithMessage(expectedExceptionMessage);
    }

    [Fact]
    public void UpdateSoundSource_WhenInvoked_UpdatesSoundSource()
    {
        // Arrange
        var manager = CreateManager();
        manager.InitDevice();
        manager.InitSound();

        // Act
        var action = () =>
        {
            var otherSoundSrc = new SoundSource
            {
                SourceId = 4321,
            };
            manager.UpdateSoundSource(otherSoundSrc);
        };

        // Assert
        action.Should().NotThrow<Exception>();
    }

    [Fact]
    public void RemoveSoundSource_WhenNotInitialized_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();

        // Act
        var action = () => manager.RemoveSoundSource(It.IsAny<uint>());

        // Assert
        var expectedExceptionMessage = "The 'AudioDeviceManager' has not been initialized.\nInvoked the 'InitDevice()' to initialize the device manager.";
        action.Should().Throw<AudioDeviceManagerNotInitializedException>().WithMessage(expectedExceptionMessage);
    }

    [Fact]
    public void RemoveSoundSource_WhenSoundSourceDoesNotExist_DoesNotAttemptToRemoveSoundSource()
    {
        // Arrange
        var manager = CreateManager();
        var myItems = new Queue<uint>();
        myItems.Enqueue(1122u);
        myItems.Enqueue(3344u);

        this.mockALInvoker.Setup(m => m.GenSource()).Returns(() => myItems.Dequeue());

        manager.InitDevice();
        manager.InitSound();
        manager.InitSound();

        // Act
        manager.RemoveSoundSource(3344u);

        // Assert
        manager.GetSoundSources().Select(s => s.SourceId).Should().ContainSingle().Which.Should().Be(1122u);
    }

    [Fact]
    public void ChangeDevice_WhenNotInitialized_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();

        // Act
        var action = () => manager.ChangeDevice("test-device");

        // Assert
        action.Should().Throw<AudioDeviceManagerNotInitializedException>().WithMessage(IsDisposedExceptionMessage);
    }

    [Fact]
    public void ChangeDevice_WhenUsingInvalidDeviceName_ThrowsException()
    {
        // Arrange
        this.mockALInvoker.Setup(m => m.GetDeviceList())
            .Returns(new[] { "test-device" }.ToImmutableArray());

        var manager = CreateManager();
        manager.InitDevice();

        // Act
        var action = () => manager.ChangeDevice("test-device-1");

        // Assert
        var expectedExceptionMessage = "Device Name: test-device-1\nThe audio device does not exist.";
        action.Should().Throw<AudioDeviceDoesNotExistException>().WithMessage(expectedExceptionMessage);
    }

    [Theory]
    [InlineData(10, 5, 5)]
    public void ChangeDevice_WhenCurrentTimePositionIsGreaterThanMaxTime_ChangesDevices(
        float timePosition,
        float totalSeconds,
        float expected)
    {
        // Arrange
        MockSoundLength(totalSeconds);

        this.mockALInvoker.Setup(m => m.GetDeviceList())
            .Returns(new[] { "test-device" }.ToImmutableArray);
        this.mockALInvoker.Setup(m => m.GetSourceState(this.srcId))
            .Returns(SourceState.Playing);
        this.mockALInvoker.Setup(m => m.GetSourceProperty(this.srcId, SourceFloat.SecOffset)).Returns(timePosition);

        var mockOggDecoder = new Mock<ISoundDecoder<float>>();
        mockOggDecoder.Setup(m => m.LoadData(It.IsAny<string>()))
            .Returns(() =>
            {
                var oggData = new SoundData<float>
                {
                    BufferData = new ReadOnlyCollection<float>(new[] { 1f }),
                    Format = AudioFormat.Stereo16,
                    Channels = 2,
                    SampleRate = 44100,
                };

                return oggData;
            });

        var manager = CreateManager();

        _ = new Sound(
            this.oggFilePath,
            this.mockALInvoker.Object,
            manager,
            mockOggDecoder.Object,
            new Mock<ISoundDecoder<byte>>().Object,
            this.mockPath.Object);

        // Act
        manager.ChangeDevice("test-device");

        // Assert
        this.mockALInvoker.Verify(m => m.GetSourceState(this.srcId), Times.Once());
        this.mockALInvoker.Verify(m => m.GetSourceProperty(this.srcId, SourceFloat.SecOffset), Times.Once());
        this.mockALInvoker.Verify(m => m.SetSourceProperty(this.srcId, SourceFloat.SecOffset, expected), Times.Once());
    }

    [Theory]
    [InlineData((int)ALSourceState.Stopped, 1, 0, 0)]
    [InlineData((int)ALSourceState.Playing, 1, 1, 220_500)] // 5 seconds of sound at a sample rate of 44100
    [InlineData((int)ALSourceState.Playing, 1, 1, 485_100)] // 11 seconds of sound at a sample rate of 44100
    [InlineData((int)ALSourceState.Playing, 1, 1, -100)] // negative second value result
    [InlineData((int)ALSourceState.Paused, 1, 1, 220_500)]
    public void ChangeDevice_WithOnlySingleSoundSource_MakesProperALCallsForCachingSources(
        int srcState,
        int srcStateInvokeCount,
        int currentTimePositionInvokeCount,
        int sampleOffset)
    {
        /*NOTE:
         * When changing a device, the state and time position should be invoked once
         * for each sound source that exists.
         */
        // Arrange
        this.mockALInvoker.Setup(m => m.GetDeviceList())
            .Returns(new[] { "test-device" }.ToImmutableArray);
        this.mockALInvoker.Setup(m => m.GetSourceState(this.srcId))
            .Returns((SourceState)srcState);
        this.mockALInvoker.Setup(m => m.GetSourceProperty(this.srcId, GetSourceInteger.SampleOffset))
            .Returns(sampleOffset); // End result will be calculated to the time position that the sound is currently at

        var mockOggDecoder = new Mock<ISoundDecoder<float>>();
        mockOggDecoder.Setup(m => m.LoadData(It.IsAny<string>()))
            .Returns(() =>
            {
                var oggData = new SoundData<float>
                {
                    BufferData = new ReadOnlyCollection<float>(new[] { 1f }),
                    Format = AudioFormat.Stereo16,
                    Channels = 2,
                    SampleRate = 44100,
                };

                return oggData;
            });

        var manager = CreateManager();
        manager.InitDevice();

        _ = new Sound(
            this.oggFilePath,
            this.mockALInvoker.Object,
            manager,
            mockOggDecoder.Object,
            new Mock<ISoundDecoder<byte>>().Object,
            this.mockPath.Object);

        // Act
        manager.ChangeDevice("test-device");

        // Assert
        this.mockALInvoker.Verify(m => m.GetSourceState(this.srcId), Times.Exactly(srcStateInvokeCount));
        this.mockALInvoker.Verify(m => m.GetSourceProperty(this.srcId, SourceFloat.SecOffset), Times.Exactly(currentTimePositionInvokeCount));
    }

    [Fact]
    public void ChangeDevice_WhenInvokedWithEventSubscription_InvokesDeviceChangedEvent()
    {
        // Arrange
        this.mockALInvoker.Setup(m => m.GetDeviceList())
            .Returns(new[] { "test-device" }.ToImmutableArray);
        var manager = CreateManager();
        manager.InitDevice();

        // Act
        var eventRaised = false;
        manager.DeviceChanged += (sender, args) => eventRaised = true;
        manager.ChangeDevice("test-device");

        // Assert
        eventRaised.Should().BeTrue();
    }

    [Fact]
    public void ChangeDevice_WithNoNoDeviceChangedEventSubscription_DoesNotThrowException()
    {
        // Arrange
        this.mockALInvoker.Setup(m => m.GetDeviceList()).Returns(new[] { "test-device" }.ToImmutableArray);

        var manager = CreateManager();
        manager.InitDevice();

        // Act
        var action = () => manager.ChangeDevice("test-device");

        // Assert
        action.Should().NotThrow<NullReferenceException>();
    }

    [Fact]
    public void Dispose_WhenInvoked_DisposesOfManager()
    {
        // Arrange
        var manager = CreateManager();
        manager.InitDevice();

        // Act
        manager.Dispose();
        manager.Dispose();

        // Assert
        this.mockALInvoker.Verify(m => m.MakeContextCurrent(default), Times.Once());
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="AudioDeviceManager"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private AudioDeviceManager CreateManager() => new (this.mockALInvoker.Object);

    /// <summary>
    /// Mocks the buffer data stats to influence the total seconds that the sound has.
    /// </summary>
    /// <param name="totalSeconds">The total number of seconds to simulate.</param>
    private void MockSoundLength(float totalSeconds)
    {
        /* This is the total seconds for every byte of data
         * based on 2 Channels, 32 bit depth and a frequency of 44100.
         *
         * Changing the channels, bit depth, or frequency changes the conversion factor.
         */
        const int bytesPerSec = 352801; // Conversion factor
        const int channels = 2;
        const int bitDepth = 32;
        const int freq = 44100;

        var size = (int)(totalSeconds * bytesPerSec);

        this.mockALInvoker.Setup(m => m.GetBuffer(this.bufferId, GetBufferInteger.Size)).Returns(size);
        this.mockALInvoker.Setup(m => m.GetBuffer(this.bufferId, GetBufferInteger.Channels)).Returns(channels);
        this.mockALInvoker.Setup(m => m.GetBuffer(this.bufferId, GetBufferInteger.Bits)).Returns(bitDepth);
        this.mockALInvoker.Setup(m => m.GetBuffer(this.bufferId, GetBufferInteger.Frequency)).Returns(freq);
    }
}
