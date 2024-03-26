// <copyright file="AudioDeviceManagerTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Devices;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CASL.Devices;
using CASL.Devices.Exceptions;
using CASL.Exceptions;
using CASL.OpenAL;
using Moq;
using Xunit;
using FluentAssertions;
using Helpers;

/// <summary>
/// Tests the <see cref="AudioDeviceManager"/> class.
/// </summary>
public class AudioDeviceManagerTests
{
    private const string IsDisposedExceptionMessage =
        $"The '{nameof(AudioDeviceManager)}' has not been initialized.\nInvoked the '{nameof(AudioDeviceManager.InitDevice)}()' to initialize the device manager.";
    private const uint SrcId = 4321;
    private const uint BufferId = 9876;
    private readonly Mock<IOpenALInvoker> mockALInvoker;
    private readonly ALContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDeviceManagerTests"/> class.
    /// </summary>
    public AudioDeviceManagerTests()
    {
        var device = new ALDevice(1234);
        this.context = new ALContext(5678);

        this.mockALInvoker = new Mock<IOpenALInvoker>();

        MockSoundLength(60);

        this.mockALInvoker.Setup(m => m.GenSource()).Returns(SrcId);
        this.mockALInvoker.Setup(m => m.GenBuffer()).Returns(BufferId);
        this.mockALInvoker.Setup(m => m.GetDeviceList()).Returns(new[] { "Device-1", "Device-2" });
        this.mockALInvoker.Setup(m => m.OpenDevice(It.IsAny<string>())).Returns(device);
        this.mockALInvoker.Setup(m => m.CreateContext(device, It.IsAny<ALContextAttributes>()))
            .Returns(this.context);
        this.mockALInvoker.Setup(m => m.MakeContextCurrent(this.context)).Returns(true);
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullALInvokerParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new AudioDeviceManager(null);
        };

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("alInvoker");
    }

    [Fact]
    public void Ctor_WhenInvoked_SubscribesToErrorCallback()
    {
        // Arrange & Act
        _ = CreateSystemUnderTest();

        // Assert
        this.mockALInvoker.VerifyAdd(e => e.ErrorCallback += It.IsAny<Action<string>>(), Times.Once());
    }
    #endregion

    #region Prop Tests
    [Fact]
    public void IsInitialized_WhenGettingValueAfterInitialization_ReturnsTrue()
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        sut.InitDevice();

        // Act
        var actual = sut.IsInitialized;

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void GetDeviceNames_WhenGettingValueAfterBeingDisposed_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        sut.Dispose();

        // Act
        var action = sut.GetDeviceNames;

        // Assert
        action.Should().Throw<AudioDeviceManagerNotInitializedException>().WithMessage(IsDisposedExceptionMessage);
    }

    [Fact]
    public void GetDeviceNames_WhenGettingValueBeforeBeingDisposed_ReturnsCorrectResult()
    {
        // Arrange
        var expected = new[] { "Device-1", "Device-2" };
        var sut = CreateSystemUnderTest();
        sut.InitDevice();

        // Act
        var actual = sut.GetDeviceNames().ToArray();

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
        var sut = CreateSystemUnderTest();

        // Act
        sut.InitDevice("test-device");

        // Assert
        this.mockALInvoker.Verify(m => m.OpenDevice("OpenAL Soft on test-device"), Times.Once());
        this.mockALInvoker.Verify(m => m.MakeContextCurrent(this.context), Times.Once());
        this.mockALInvoker.Verify(m => m.GetDefaultDevice(), Times.Once());
        sut.DeviceInUse.Should().Be("OpenAL Soft on test-device");
    }

    [Fact]
    public void InitDevice_WithIssueMakingContextCurrent_ThrowsException()
    {
        // Arrange
        // The MakeContextCurrent call does not take nullable bool.  This fixes that issue
        this.mockALInvoker.Setup(m => m.MakeContextCurrent(this.context)).Returns(false);
        var sut = CreateSystemUnderTest();

        // Act
        var action = () => sut.InitDevice("test-device");

        // Assert
        action.Should().Throw<InitializeDeviceException>().WithMessage("There was an issue initializing the audio device.");
    }

    [Fact]
    public void ChangeDevice_WhenNotInitialized_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act
        var action = () => sut.ChangeDevice("test-device");

        // Assert
        action.Should().Throw<AudioDeviceManagerNotInitializedException>().WithMessage(IsDisposedExceptionMessage);
    }

    [Fact]
    public void ChangeDevice_WhenUsingInvalidDeviceName_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        sut.InitDevice();

        // Act
        var action = () => sut.ChangeDevice("non-existing-device");

        // Assert
        var expectedExceptionMessage = "Device Name: non-existing-device\nThe audio device does not exist.";
        action.Should().Throw<AudioDeviceDoesNotExistException>().WithMessage(expectedExceptionMessage);
    }

    [Fact]
    public void ChangeDevice_WhenNotSubscribedToDeviceChangingEvent_DoesNotThrowException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        sut.InitDevice();

        // Act
        var action = () => sut.ChangeDevice("Device-1");

        // Assert
        action.Should().NotThrow<NullReferenceException>();
    }

    [Fact]
    public void ChangeDevice_WhenNotSubscribedToDeviceChangedEvent_DoesNotThrowException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        sut.InitDevice();

        // Act
        var action = () => sut.ChangeDevice("Device-2");

        // Assert
        action.Should().NotThrow<NullReferenceException>();
    }

    [Fact]
    public void ChangeDevice_WhenRequestedDeviceIsAlreadyInUse_DoesNotChangeDevices()
    {
        // Arrange
        this.mockALInvoker.Setup(m => m.GetDeviceList()).Returns(["test-device"]);
        this.mockALInvoker.Setup(m => m.GetDefaultDevice()).Returns("test-device");
        var sut = CreateSystemUnderTest();
        sut.InitDevice();

        // Act
        sut.ChangeDevice("test-device");

        // Assert
        this.mockALInvoker.Verify(m => m.GetDeviceList(), Times.Never);
        this.mockALInvoker.Verify(m => m.MakeContextCurrent(It.IsAny<ALContext>()), Times.Exactly(1));
        this.mockALInvoker.Verify(m => m.DestroyContext(It.IsAny<ALContext>()), Times.Never);
        this.mockALInvoker.Verify(m => m.CloseDevice(It.IsAny<ALDevice>()), Times.Never);
    }

    [Theory]
    [InlineData((int)ALSourceState.Playing)]
    [InlineData((int)ALSourceState.Paused)]
    public void ChangeDevice_WithCacheable_RunsChangeDevicesProcess(int playState)
    {
        // Arrange
        MockSoundLength(5);

        this.mockALInvoker.Setup(m => m.GetSourceState(SrcId))
            .Returns((ALSourceState)playState);
        this.mockALInvoker.Setup(m => m.GetSource(SrcId, ALSourcef.SecOffset)).Returns(SrcId);
        this.mockALInvoker.Setup(m => m.GetSource(SrcId, ALSourcef.Pitch)).Returns(1f);

        var device1 = new ALDevice(1111);
        var device2 = new ALDevice(2222);
        var context1 = new ALContext(3333);
        var context2 = new ALContext(4444);

        this.mockALInvoker.Setup(m => m.MakeContextCurrent(It.IsAny<ALContext>())).Returns(true);
        this.mockALInvoker.Setup(m => m.OpenDevice("OpenAL Soft on Device-1")).Returns(device1);
        this.mockALInvoker.Setup(m => m.OpenDevice("OpenAL Soft on Device-2")).Returns(device2);
        this.mockALInvoker.Setup(m => m.CreateContext(device1, It.IsAny<ALContextAttributes>())).Returns(context1);
        this.mockALInvoker.Setup(m => m.CreateContext(device2, It.IsAny<ALContextAttributes>())).Returns(context2);

        var deviceChangingEventRaised = false;
        var deviceChangedEventRaised = false;

        var sut = CreateSystemUnderTest();
        sut.DeviceChanging += (_, _) => deviceChangingEventRaised = true;
        sut.DeviceChanged += (_, _) => deviceChangedEventRaised = true;
        sut.InitDevice("Device-1");

        // Act
        sut.ChangeDevice("Device-2");

        // Assert
        deviceChangingEventRaised.Should().BeTrue();

        // Verify that the device was destroyed
        this.mockALInvoker.Verify(m => m.MakeContextCurrent(ALContext.Null()));
        this.mockALInvoker.Verify(m => m.DestroyContext(context1));
        this.mockALInvoker.Verify(m => m.CloseDevice(device1));

        // Verify that the new device was initialized
        this.mockALInvoker.Verify(m => m.OpenDevice("OpenAL Soft on Device-2"), Times.Once);
        this.mockALInvoker.Verify(m => m.CreateContext(device2, It.IsAny<ALContextAttributes>()), Times.Once);
        this.mockALInvoker.Verify(m => m.MakeContextCurrent(context2), Times.Once);
        this.mockALInvoker.Verify(m => m.GetDefaultDevice());

        deviceChangedEventRaised.Should().BeTrue();
    }

    [Fact]
    [SuppressMessage("csharpsquid", "S3966", Justification = "Need to execute dispose twice for testing.")]
    public void Dispose_WhenInvoked_DisposesOfManager()
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        sut.InitDevice();

        // Act
        sut.Dispose();
        sut.Dispose();

        // Assert
        this.mockALInvoker.Verify(m => m.MakeContextCurrent(ALContext.Null()), Times.Once());
    }
    #endregion

    #region Indirect Tests
    [Fact]
    public void ALInvoker_WhenOpenALErrorOccurs_ThrowsException()
    {
        // Arrange
        _ = CreateSystemUnderTest();

        // Act
        var act = () => this.mockALInvoker.Raise(x => x.ErrorCallback += null, "test-error");

        // Assert
        act.Should().Throw<AudioException>().WithMessage("test-error");
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="AudioDeviceManager"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private AudioDeviceManager CreateSystemUnderTest() => new (this.mockALInvoker.Object);

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

        this.mockALInvoker.Setup(m => m.GetBuffer(BufferId, ALGetBufferi.Size)).Returns(size);
        this.mockALInvoker.Setup(m => m.GetBuffer(BufferId, ALGetBufferi.Channels)).Returns(channels);
        this.mockALInvoker.Setup(m => m.GetBuffer(BufferId, ALGetBufferi.Bits)).Returns(bitDepth);
        this.mockALInvoker.Setup(m => m.GetBuffer(BufferId, ALGetBufferi.Frequency)).Returns(freq);
    }
}
