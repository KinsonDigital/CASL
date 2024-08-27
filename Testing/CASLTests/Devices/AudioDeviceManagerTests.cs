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
using Xunit;
using FluentAssertions;
using Helpers;
using NSubstitute;

/// <summary>
/// Tests the <see cref="AudioDeviceManager"/> class.
/// </summary>
public class AudioDeviceManagerTests
{
    private const uint SrcId = 4321;
    private const uint BufferId = 9876;
    private readonly IOpenALInvoker mockALInvoker;
    private readonly ALContext context;
    private readonly ALDevice device;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDeviceManagerTests"/> class.
    /// </summary>
    public AudioDeviceManagerTests()
    {
        this.device = new ALDevice(1234);
        this.context = new ALContext(5678);

        this.mockALInvoker = Substitute.For<IOpenALInvoker>();

        MockAudioLength(60);

        this.mockALInvoker.GenSource().Returns(SrcId);
        this.mockALInvoker.GenBuffer().Returns(BufferId);
        this.mockALInvoker.GetDeviceList().Returns(new[] { "Device-1", "Device-2" });
        this.mockALInvoker.OpenDevice(Arg.Any<string>()).Returns(this.device);
        this.mockALInvoker.CreateContext(this.device, Arg.Any<ALContextAttributes>()).Returns(this.context);
        this.mockALInvoker.MakeContextCurrent(this.context).Returns(true);
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
        this.mockALInvoker.Received(1).ErrorCallback += Arg.Any<Action<string>>();
    }

    [Fact]
    public void Ctor_WithIssueMakingContextCurrent_ThrowsException()
    {
        // Arrange
        // The MakeContextCurrent call does not take nullable bool.  This fixes that issue
        this.mockALInvoker.MakeContextCurrent(this.context).Returns(false);

        // Act
        // ReSharper disable once ConvertClosureToMethodGroup
        var act = () => CreateSystemUnderTest();

        // Assert
        act.Should().Throw<InitializeDeviceException>().WithMessage("There was an issue initializing the audio device.");
    }
    #endregion

    #region Prop Tests
    [Fact]
    public void IsInitialized_WhenGettingValueAfterInitialization_ReturnsTrue()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.IsInitialized;

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void GetDeviceNames_WhenGettingValueBeforeBeingDisposed_ReturnsCorrectResult()
    {
        // Arrange
        var expected = new[] { "Device-1", "Device-2" };
        var sut = CreateSystemUnderTest();

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
    public void ChangeDevice_WhenUsingInvalidDeviceName_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

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

        // Act
        var action = () => sut.ChangeDevice("Device-2");

        // Assert
        action.Should().NotThrow<NullReferenceException>();
    }

    [Fact]
    public void ChangeDevice_WhenRequestedDeviceIsAlreadyInUse_DoesNotChangeDevices()
    {
        // Arrange
        this.mockALInvoker.GetDeviceList().Returns(["test-device"]);
        this.mockALInvoker.GetDefaultDevice().Returns("test-device");
        var sut = CreateSystemUnderTest();

        // Act
        sut.ChangeDevice("test-device");

        // Assert
        this.mockALInvoker.DidNotReceive().GetDeviceList();
        this.mockALInvoker.Received(1).MakeContextCurrent(Arg.Any<ALContext>());
        this.mockALInvoker.DidNotReceive().DestroyContext(Arg.Any<ALContext>());
        this.mockALInvoker.DidNotReceive().CloseDevice(Arg.Any<ALDevice>());
    }

    [Theory]
    [InlineData((int)ALSourceState.Playing)]
    [InlineData((int)ALSourceState.Paused)]
    public void ChangeDevice_WithCacheable_RunsChangeDevicesProcess(int playState)
    {
        // Arrange
        MockAudioLength(5);

        this.mockALInvoker.GetSourceState(SrcId).Returns((ALSourceState)playState);
        this.mockALInvoker.GetSource(SrcId, ALSourcef.SecOffset).Returns(SrcId);
        this.mockALInvoker.GetSource(SrcId, ALSourcef.Pitch).Returns(1f);

        var newDevice = new ALDevice(2222);
        var newContext = new ALContext(4444);

        this.mockALInvoker.MakeContextCurrent(Arg.Any<ALContext>()).Returns(true);
        this.mockALInvoker.OpenDevice("OpenAL Soft on Device-2").Returns(newDevice);
        this.mockALInvoker.CreateContext(newDevice, Arg.Any<ALContextAttributes>()).Returns(newContext);

        var deviceChangingEventRaised = false;
        var deviceChangedEventRaised = false;

        var sut = CreateSystemUnderTest();
        sut.DeviceChanging += (_, _) => deviceChangingEventRaised = true;
        sut.DeviceChanged += (_, _) => deviceChangedEventRaised = true;

        // Act
        sut.ChangeDevice("Device-2");

        // Assert
        deviceChangingEventRaised.Should().BeTrue();

        // Verify that the device was destroyed
        // GetDeviceList
        this.mockALInvoker.Received(1).GetDeviceList();
        this.mockALInvoker.Received(1).MakeContextCurrent(ALContext.Null());
        this.mockALInvoker.Received(1).DestroyContext(this.context);
        this.mockALInvoker.Received(1).CloseDevice(this.device);

        sut.GetStructFieldValue<ALDevice>("device").Should().Be(newDevice);
        sut.GetStructFieldValue<ALContext>("context").Should().Be(newContext);

        // Verify that the new device was initialized
        this.mockALInvoker.Received(1).OpenDevice("OpenAL Soft on Device-2");
        this.mockALInvoker.Received(1).CreateContext(newDevice, Arg.Any<ALContextAttributes>());
        this.mockALInvoker.Received(1).MakeContextCurrent(newContext);
        this.mockALInvoker.Received(2).GetDefaultDevice();

        deviceChangedEventRaised.Should().BeTrue();
    }

    [Fact]
    [SuppressMessage("csharpsquid", "S3966", Justification = "Need to execute dispose twice for testing.")]
    public void Dispose_WhenInvoked_DisposesOfManager()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act
        sut.Dispose();
        sut.Dispose();

        // Assert
        this.mockALInvoker.Received(1).MakeContextCurrent(ALContext.Null());
    }
    #endregion

    #region Indirect Tests
    [Fact]
    public void ALInvoker_WhenOpenALErrorOccurs_ThrowsException()
    {
        // Arrange
        _ = CreateSystemUnderTest();

        // Act
        var act = () => this.mockALInvoker.ErrorCallback += Raise.Event<Action<string>>("test-error");

        // Assert
        act.Should().Throw<AudioException>().WithMessage("test-error");
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="AudioDeviceManager"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private AudioDeviceManager CreateSystemUnderTest() => new (this.mockALInvoker);

    /// <summary>
    /// Mocks the buffer data stats to influence the total seconds that the  audio has.
    /// </summary>
    /// <param name="totalSeconds">The total number of seconds to simulate.</param>
    private void MockAudioLength(float totalSeconds)
    {
        /* This is the total seconds for every byte of data
         * based on 2 Channels, 32 bit depth and a frequency of 44100.
         *
         * Changing the channels, the bit depth, or frequency changes the conversion factor.
         */
        const int bytesPerSec = 352801; // Conversion factor
        const int channels = 2;
        const int bitDepth = 32;
        const int freq = 44100;

        var size = (int)(totalSeconds * bytesPerSec);

        this.mockALInvoker.GetBuffer(BufferId, ALGetBufferi.Size).Returns(size);
        this.mockALInvoker.GetBuffer(BufferId, ALGetBufferi.Channels).Returns(channels);
        this.mockALInvoker.GetBuffer(BufferId, ALGetBufferi.Bits).Returns(bitDepth);
        this.mockALInvoker.GetBuffer(BufferId, ALGetBufferi.Frequency).Returns(freq);
    }
}
