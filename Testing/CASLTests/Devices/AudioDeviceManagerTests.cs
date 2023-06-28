// <copyright file="AudioDeviceManagerTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

#pragma warning disable IDE0002 // Name can be simplified
namespace CASLTests.Devices;

#pragma warning disable IDE0001 // Name can be simplified
using System;
using System.Collections.Generic;
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
using Assert = Helpers.AssertExtensions;
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
    private readonly ALDevice device;
    private readonly ALContext context;
    private readonly uint srcId = 4321;
    private readonly uint bufferId = 9876;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDeviceManagerTests"/> class.
    /// </summary>
    public AudioDeviceManagerTests()
    {
        this.oggFilePath = @"C:/temp/Content/Sounds/sound.ogg";
        this.device = new ALDevice(1234);
        this.context = new ALContext(5678);

        this.mockALInvoker = new Mock<IOpenALInvoker>();

        MockSoundLength(60);

        this.mockALInvoker.Setup(m => m.GenSource()).Returns(this.srcId);
        this.mockALInvoker.Setup(m => m.GenBuffer()).Returns(this.bufferId);
        this.mockALInvoker.Setup(m => m.OpenDevice(It.IsAny<string>())).Returns(this.device);
        this.mockALInvoker.Setup(m => m.CreateContext(this.device, It.IsAny<ALContextAttributes>()))
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
        Assert.True(actual);
    }

    [Fact]
    public void GetDeviceNames_WhenGettingValueAfterBeingDisposed_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        manager.Dispose();

        // Act & Assert
        Assert.ThrowsWithMessage<AudioDeviceManagerNotInitializedException>(() =>
        {
            _ = manager.GetDeviceNames();
        }, IsDisposedExceptionMessage);
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
        Assert.Equal(expected, actual);
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
        Assert.NotNull(actual);
        Assert.Empty(actual);
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
        Assert.NotNull(actual);
        Assert.Equal(2, actual.Length);
        Assert.Equal(111, actual[0]);
        Assert.Equal(222, actual[1]);
    }
    #endregion

    #region Method Tests
    [Fact]
    public void InitDevice_WhenInvoked_InitializesDevice()
    {
        // Arrange
        var manager = CreateManager();

        // Act
        manager.InitDevice("test-device");

        // Assert
        this.mockALInvoker.Verify(m => m.OpenDevice("OpenAL Soft on test-device"), Times.Once());
        this.mockALInvoker.Verify(m => m.MakeContextCurrent(this.context), Times.Once());
        this.mockALInvoker.Verify(m => m.GetDefaultDevice(), Times.Once());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public void InitDevice_WithIssueMakingContextCurrent_ThrowsException(bool? makeContextCurrentResult)
    {
        // Arrange
        // The MakeContextCurrent call does not take nullable bool.  This fixes that issue
        this.mockALInvoker.Setup(m => m.MakeContextCurrent(this.context)).Returns(false);
        var manager = CreateManager();

        // Act & Assert
        Assert.ThrowsWithMessage<InitializeDeviceException>(() =>
        {
            manager.InitDevice("test-device");
        }, "There was an issue initializing the audio device.");
    }

    [Fact]
    public void InitSound_WithSingleParamAndWhileDisposed_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        manager.Dispose();

        // Act & Assert
        Assert.ThrowsWithMessage<AudioDeviceManagerNotInitializedException>(() =>
        {
            manager.InitSound();
        }, IsDisposedExceptionMessage);
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
        Assert.Equal(this.srcId, actualSourceId);
        Assert.Equal(this.bufferId, actualBufferId);
        this.mockALInvoker.Verify(m => m.GenSource(), Times.Once());
        this.mockALInvoker.Verify(m => m.GenBuffer(), Times.Once());
    }

    [Fact]
    public void UpdateSoundSource_WhenNotInitialized_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();

        // Act & Assert
        Assert.ThrowsWithMessage<AudioDeviceManagerNotInitializedException>(() =>
        {
            manager.UpdateSoundSource(It.IsAny<SoundSource>());
        }, IsDisposedExceptionMessage);
    }

    [Fact]
    public void UpdateSoundSource_WhenSoundSourceDoesNotExist_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();
        manager.InitDevice();

        // Act & Assert
        Assert.ThrowsWithMessage<SoundDataException>(() =>
        {
            var soundSrc = new SoundSource()
            {
                SourceId = 1234,
            };
            manager.UpdateSoundSource(soundSrc);
        }, $"The sound source with the source id '1234' does not exist.");
    }

    [Fact]
    public void UpdateSoundSource_WhenInvoked_UpdatesSoundSource()
    {
        // Arrange
        var manager = CreateManager();
        manager.InitDevice();
        manager.InitSound();

        // Act & Assert
        Assert.DoesNotThrow<Exception>(() =>
        {
            var otherSoundSrc = new SoundSource()
            {
                SourceId = 4321,
            };
            manager.UpdateSoundSource(otherSoundSrc);
        });
    }

    [Fact]
    public void RemoveSoundSource_WhenNotInitialized_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();

        // Act & Assert
        Assert.ThrowsWithMessage<AudioDeviceManagerNotInitializedException>(() =>
        {
            manager.RemoveSoundSource(It.IsAny<uint>());
        }, "The 'AudioDeviceManager' has not been initialized.\nInvoked the 'InitDevice()' to initialize the device manager.");
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
        Assert.Single(manager.GetSoundSources().ToArray());
        Assert.Equal(1122u, manager.GetSoundSources().ToArray()[0].SourceId);
    }

    [Fact]
    public void ChangeDevice_WhenNotInitialized_ThrowsException()
    {
        // Arrange
        var manager = CreateManager();

        // Act & Assert
        Assert.ThrowsWithMessage<AudioDeviceManagerNotInitializedException>(() =>
        {
            manager.ChangeDevice("test-device");
        }, IsDisposedExceptionMessage);
    }

    [Fact]
    public void ChangeDevice_WhenUsingInvalidDeviceName_ThrowsException()
    {
        // Arrange
        this.mockALInvoker.Setup(m => m.GetDeviceList())
            .Returns(new[] { "test-device" });

        var manager = CreateManager();
        manager.InitDevice();

        // Act & Assert
        Assert.ThrowsWithMessage<AudioDeviceDoesNotExistException>(() =>
        {
            manager.ChangeDevice("test-device-1");
        }, "Device Name: test-device-1\nThe audio device does not exist.");
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
            .Returns(new[] { "test-device" });
        this.mockALInvoker.Setup(m => m.GetSourceState(this.srcId))
            .Returns(ALSourceState.Playing);
        this.mockALInvoker.Setup(m => m.GetSource(this.srcId, ALSourcef.SecOffset)).Returns(timePosition);

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
        this.mockALInvoker.Verify(m => m.GetSource(this.srcId, ALSourcef.SecOffset), Times.Once());
        this.mockALInvoker.Verify(m => m.Source(this.srcId, ALSourcef.SecOffset, expected), Times.Once());
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
            .Returns(new[] { "test-device" });
        this.mockALInvoker.Setup(m => m.GetSourceState(this.srcId))
            .Returns((ALSourceState)srcState);
        this.mockALInvoker.Setup(m => m.GetSource(this.srcId, ALGetSourcei.SampleOffset))
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
        this.mockALInvoker.Verify(m => m.GetSource(this.srcId, ALSourcef.SecOffset), Times.Exactly(currentTimePositionInvokeCount));
    }

    [Fact]
    public void ChangeDevice_WhenInvokedWithEventSubscription_InvokesDeviceChangedEvent()
    {
        // Arrange
        this.mockALInvoker.Setup(m => m.GetDeviceList())
            .Returns(new[] { "test-device" });
        var manager = CreateManager();
        manager.InitDevice();

        // Act & Assert
        Assert.Raises<EventArgs>((e) =>
        {
            manager.DeviceChanged += e;
        }, (e) =>
        {
            manager.DeviceChanged -= e;
        }, () =>
        {
            manager.ChangeDevice("test-device");
        });
    }

    [Fact]
    public void ChangeDevice_WithNoNoDeviceChangedEventSubscription_DoesNotThrowException()
    {
        // Arrange
        this.mockALInvoker.Setup(m => m.GetDeviceList()).Returns(new[] { "test-device" });

        var manager = CreateManager();
        manager.InitDevice();

        // Act & Assert
        Assert.DoesNotThrowNullReference(() =>
        {
            manager.ChangeDevice("test-device");
        });
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
        this.mockALInvoker.Verify(m => m.MakeContextCurrent(ALContext.Null()), Times.Once());
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

        this.mockALInvoker.Setup(m => m.GetBuffer(this.bufferId, ALGetBufferi.Size)).Returns(size);
        this.mockALInvoker.Setup(m => m.GetBuffer(this.bufferId, ALGetBufferi.Channels)).Returns(channels);
        this.mockALInvoker.Setup(m => m.GetBuffer(this.bufferId, ALGetBufferi.Bits)).Returns(bitDepth);
        this.mockALInvoker.Setup(m => m.GetBuffer(this.bufferId, ALGetBufferi.Frequency)).Returns(freq);
    }
}
