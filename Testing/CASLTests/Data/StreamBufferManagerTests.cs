// <copyright file="StreamBufferManagerTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

#pragma warning disable SA1202

namespace CASLTests.Data;

using System;
using System.ComponentModel;
using CASL;
using CASL.Data;
using CASL.OpenAL;
using FluentAssertions;
using Helpers;
using NSubstitute;
using Xunit;

/// <summary>
/// Tests the <see cref="StreamBufferManager"/> class.
/// </summary>
public class StreamBufferManagerTests
{
    private readonly IOpenALInvoker mockAlInvoker;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamBufferManagerTests"/> class.
    /// </summary>
    public StreamBufferManagerTests() => this.mockAlInvoker = Substitute.For<IOpenALInvoker>();

    #region Constructors
    [Fact]
    public void Ctor_WithNullAlInvokerParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new StreamBufferManager(null);
        };

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("alInvoker");
    }
    #endregion

    #region Method Tests
    [Fact]
    public void GetCurrentSampleData_WithDefaultValue_ReturnsZero()
    {
        // Arrange
        var sut = new StreamBufferManager(this.mockAlInvoker);

        // Act
        var actual = sut.GetCurrentSamplePos();

        // Assert
        actual.Should().Be(0);
    }

    [Fact]
    public void SetSamplePos_WhenSettingValue_SetsToCorrectValue()
    {
        // Arrange
        var sut = new StreamBufferManager(this.mockAlInvoker);

        // Act
        sut.SetSamplePos(123);
        var actual = sut.GetCurrentSamplePos();

        // Assert
        actual.Should().Be(123);
    }

    [Fact]
    public void ResetSamplePos_WhenInvoked_ResetsSamplePos()
    {
        // Arrange
        var sut = new StreamBufferManager(this.mockAlInvoker);
        sut.SetSamplePos(123);

        // Act
        sut.ResetSamplePos();
        var actual = sut.GetCurrentSamplePos();

        // Assert
        actual.Should().Be(0);
    }

    [Fact]
    public void ManageBuffers_WithInvalidFormatType_ThrowsException()
    {
        // Arrange
        var expectedMsg = "The value of argument 'this.FormatType' (10000) is invalid for";
        expectedMsg += " Enum type 'AudioFormatType'. (Parameter 'this.FormatType')";

        var bufferData = new BufferStats { FormatType = (AudioFormatType)10_000 };

        this.mockAlInvoker.GetSource(Arg.Any<uint>(), ALGetSourcei.BuffersProcessed).Returns(1);

        var sut = new StreamBufferManager(this.mockAlInvoker);

        // Act
        var act = () => sut.ManageBuffers(bufferData, Array.Empty<float>);

        // Assert
        act.Should().Throw<InvalidEnumArgumentException>()
            .WithMessage(expectedMsg);
    }

    [Theory]
    [InlineData(
        ALFormat.StereoFloat32Ext,
        AudioFormatType.Ogg,
        41_000,
        2,
        ALFormat.StereoFloat32Ext,
        41_000,
        8_192)]
    [InlineData(
        ALFormat.Stereo16,
        AudioFormatType.Mp3,
        20_000,
        2,
        ALFormat.Stereo16,
        20_000,
        4_096)]
    internal void ManageBuffers_WithSampleData_RefillsAndRequeuesBuffers(
        ALFormat format,
        AudioFormatType formatType,
        int sampleRate,
        int totalChannels,
        ALFormat expectedFormat,
        int expectedSampleRate,
        long expectedSamplePos)
    {
        // Arrange
        var bufferId = 987u;
        const uint srcId = 123u;
        var expectedBufferData = new[] { 10f, 20f };
        float[] ReadSampleData()
        {
            return expectedBufferData;
        }

        this.mockAlInvoker.GetSource(Arg.Any<uint>(), ALGetSourcei.BuffersProcessed).Returns(2);

        // ReSharper disable once AccessToModifiedClosure - Need access due to ref parameter usage below
        this.mockAlInvoker
            .When(x => x.SourceUnqueueBuffer(Arg.Any<uint>(), ref Arg.Any<uint>()))
            .Do(callInfo => callInfo[1] = bufferId);

        var sut = new StreamBufferManager(this.mockAlInvoker);

        var bufferData = new BufferStats
        {
            SourceId = srcId,
            DecoderFormat = format,
            FormatType = formatType,
            SampleRate = sampleRate,
            TotalChannels = totalChannels,
        };

        // Act
        sut.ManageBuffers(bufferData, ReadSampleData);
        var samplePos = sut.GetCurrentSamplePos();

        // Assert
        this.mockAlInvoker.Received(1).GetSource(srcId, ALGetSourcei.BuffersProcessed);
        this.mockAlInvoker.Received(2).SourceUnqueueBuffer(srcId, ref Arg.Any<uint>());
        this.mockAlInvoker.Received(2).BufferData(bufferId, expectedFormat, expectedBufferData, expectedSampleRate);
        this.mockAlInvoker.Received(2).SourceQueueBuffer(srcId, ref bufferId);
        samplePos.Should().Be(expectedSamplePos);
    }

    [Fact]
    public void UnqueueProcessedBuffers_WhenInvoked_UnqueuesAllProcessedBuffers()
    {
        // Arrange
        const uint srcId = 123u;
        this.mockAlInvoker.GetSource(Arg.Any<uint>(), ALGetSourcei.BuffersProcessed)
            .Returns(2);
        var sut = new StreamBufferManager(this.mockAlInvoker);

        // Act
        sut.UnqueueProcessedBuffers(srcId);

        // Assert
        this.mockAlInvoker.Received(1).SourceStop(srcId);
        this.mockAlInvoker.Received(1).GetSource(srcId, ALGetSourcei.BuffersProcessed);
        this.mockAlInvoker.Received(2).SourceUnqueueBuffers(srcId, 1, ref Arg.Any<uint[]>());
    }

    [Fact]
    public void FillBuffersFromStart_WithInvalidFormatType_ThrowsException()
    {
        // Arrange
        var expectedMsg = "The value of argument 'this.FormatType' (10000) is invalid for Enum type 'AudioFormatType'.";
        expectedMsg += " (Parameter 'this.FormatType')";

        var bufferStats = new BufferStats
        {
            SourceId = 123u,
            FormatType = (AudioFormatType)10_000,
        };

        void FlushData()
        {
            // implementation not required
        }

        float[] ReadSamples() => new[] { 0f };

        this.mockAlInvoker.GetSource(Arg.Any<uint>(), ALGetSourcei.BuffersProcessed).Returns(1);

        var sut = new StreamBufferManager(this.mockAlInvoker);

        // Act
        var act = () => sut.FillBuffersFromStart(bufferStats, new[] { 0u }, FlushData, (Func<float[]>?)ReadSamples);

        // Assert
        act.Should().Throw<InvalidEnumArgumentException>()
            .WithMessage(expectedMsg);
    }

    [Fact]
    public void FillBuffersFromStart_WithNoSampleData_DoesNotAttemptDataBuffering()
    {
        // Arrange
        const uint srcId = 123u;
        var bufferStats = new BufferStats
        {
            SourceId = srcId,
            DecoderFormat = ALFormat.StereoFloat32Ext,
            FormatType = AudioFormatType.Ogg,
            SampleRate = 41_000,
            TotalChannels = 2,
        };
        var bufferIds = new[] { 123u, 456u, };
        var flushDataInvoked = false;

        void FlushData() => flushDataInvoked = true;

        float[] ReadSamples() => Array.Empty<float>();

        this.mockAlInvoker.GetSource(Arg.Any<uint>(), ALGetSourcei.BuffersProcessed).Returns(1);

        var sut = new StreamBufferManager(this.mockAlInvoker);

        // Act
        sut.FillBuffersFromStart(bufferStats, bufferIds, FlushData, (Func<float[]>?)ReadSamples);
        var actualSamplePos = sut.GetCurrentSamplePos();

        // Assert
        this.mockAlInvoker.Received(2).SourceStop(srcId);
        this.mockAlInvoker.Received(1).GetSource(srcId, ALGetSourcei.BuffersProcessed);
        this.mockAlInvoker.Received(1).SourceUnqueueBuffers(srcId, 1, ref Arg.Any<uint[]>());
        flushDataInvoked.Should().BeTrue();
        this.mockAlInvoker.DidNotReceive().BufferData(Arg.Any<uint>(), Arg.Any<ALFormat>(), Arg.Any<float[]>(), Arg.Any<int>());
        this.mockAlInvoker.DidNotReceive().SourceQueueBuffer(Arg.Any<uint>(), ref Arg.Any<uint>());
        actualSamplePos.Should().Be(0);
    }

    [Theory]
    [InlineData(
        ALFormat.StereoFloat32Ext,
        AudioFormatType.Ogg,
        41_000,
        2,
        ALFormat.StereoFloat32Ext,
        41_000)]
    [InlineData(
        ALFormat.Stereo16,
        AudioFormatType.Mp3,
        20_000,
        2,
        ALFormat.Stereo16,
        20_000)]
    internal void FillBuffersFromStart_WhenInvoked_FillsBuffersFromStartOfAudioData(
        ALFormat format,
        AudioFormatType formatType,
        int sampleRate,
        int totalChannels,
        ALFormat expectedFormat,
        int expectedSampleRate)
    {
        // Arrange
        const uint srcId = 123u;
        var bufferStats = new BufferStats
        {
            SourceId = srcId,
            DecoderFormat = format,
            FormatType = formatType,
            SampleRate = sampleRate,
            TotalChannels = totalChannels,
        };
        var bufferIds = new[] { 123u, 456u, };
        var flushDataInvoked = false;

        void FlushData() => flushDataInvoked = true;

        var expectedSampleData = new[] { 10f, 20f };
        float[] ReadSamples() => expectedSampleData;

        this.mockAlInvoker.GetSource(Arg.Any<uint>(), ALGetSourcei.BuffersProcessed).Returns(1);

        var sut = new StreamBufferManager(this.mockAlInvoker);

        // Act
        sut.FillBuffersFromStart(bufferStats, bufferIds, FlushData, (Func<float[]>?)ReadSamples);
        var actualSamplePos = sut.GetCurrentSamplePos();

        // Assert
        this.mockAlInvoker.Received(2).SourceStop(srcId);
        this.mockAlInvoker.Received(1).GetSource(srcId, ALGetSourcei.BuffersProcessed);
        this.mockAlInvoker.Received(1).SourceUnqueueBuffers(srcId, 1, ref Arg.Any<uint[]>());
        flushDataInvoked.Should().BeTrue();
        this.mockAlInvoker.Received(1).BufferData(123u, expectedFormat, expectedSampleData, expectedSampleRate);
        this.mockAlInvoker.Received(1).BufferData(456u, expectedFormat, expectedSampleData, expectedSampleRate);
        this.mockAlInvoker.Received(1).SourceQueueBuffer(srcId, ref Arg.Is<uint>(arg => arg == 123u));
        this.mockAlInvoker.Received(1).SourceQueueBuffer(srcId, ref Arg.Is<uint>(arg => arg == 456u));
        actualSamplePos.Should().Be(0);
    }
    #endregion
}
