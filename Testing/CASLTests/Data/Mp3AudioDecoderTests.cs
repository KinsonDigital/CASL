// <copyright file="Mp3AudioDecoderTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Data;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CASL.Data.Decoders;
using CASL.OpenAL;
using CASL.Wrappers;
using Shouldly;
using Helpers;
using MP3Sharp;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ReceivedExtensions;
using Xunit;

/// <summary>
/// Tests the <see cref="Mp3AudioDecoder"/> class.
/// </summary>
public class Mp3AudioDecoderTests
{
    private readonly IMP3StreamWrapper mockMP3Stream;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mp3AudioDecoderTests"/> class.
    /// </summary>
    public Mp3AudioDecoderTests() => this.mockMP3Stream = Substitute.For<IMP3StreamWrapper>();

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullFilePathParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => _ = new Mp3AudioDecoder(null, this.mockMP3Stream);

        // Assert
        Should.Throw<ArgumentNullException>(act)
            .Message.ShouldBe("Value cannot be null. (Parameter 'filePath')");
    }

    [Fact]
    public void Ctor_WithEmptyFilePathParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => _ = new Mp3AudioDecoder(string.Empty, this.mockMP3Stream);

        // Assert
        Should.Throw<ArgumentException>(act)
            .Message.ShouldBe("The value cannot be an empty string. (Parameter 'filePath')");
    }

    [Fact]
    public void Ctor_WithNullMp3StreamWrapperParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => new Mp3AudioDecoder("test-path", null);

        // Assert
        Should.Throw<ArgumentNullException>(act).WithNullParamMsg("mp3StreamWrapper");
    }

    [Fact]
    public void Ctor_WhenInvoked_CalculatesBytesSamplesSeconds()
    {
        // Arrange & Act
        MockSampleTimeCalcs(500_000, 1);

        var sut = CreateSystemUnderTest();

        // Assert
        this.mockMP3Stream.Received(1).Flush();
        this.mockMP3Stream.Received(1).Dispose();
        this.mockMP3Stream.Received(2).Load("test-path");
        sut.TotalBytes.ShouldBe(500_000);
        sut.TotalSamples.ShouldBe(250_000);
        sut.TotalSeconds.ShouldBe(2.6041667f);
    }
    #endregion

    #region Prop Tests
    [Fact]
    public void TotalChannels_WhenGettingValue_ReturnsCorrectResult()
    {
        // Arrange
        this.mockMP3Stream.ChannelCount.Returns(123);

        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.TotalChannels;

        // Assert
        actual.ShouldBe(123);
    }

    [Theory]
    [InlineData(SoundFormat.Pcm16BitMono, ALFormat.Mono16)]
    [InlineData(SoundFormat.Pcm16BitStereo, ALFormat.Stereo16)]
    internal void Format_WhenGettingValue_ReturnsCorrectResult(SoundFormat format, ALFormat expected)
    {
        // Arrange
        this.mockMP3Stream.Format.Returns(format);
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.Format;

        // Assert
        actual.ShouldBe(expected);
    }

    [Fact]
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1202:'public' members should come before 'internal' members",
        Justification = "The standard text is a struct, not a class.")]
    public void SampleRate_WhenGettingValue_ReturnsCorrectResult()
    {
        // Arrange
        this.mockMP3Stream.Frequency.Returns(456);
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.SampleRate;

        // Assert
        actual.ShouldBe(456);
    }

    [Fact]
    public void TotalSampleFrames_WhenGettingValue_ReturnsCorrectResult()
    {
        // Arrange
        MockSampleTimeCalcs(500_000, 1);

        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.TotalSampleFrames;

        // Assert
        actual.ShouldBe(125_000);
    }
    #endregion

    #region Method Tests
    [Fact]
    public void ReadSamples_With1ParamOverload_ReadsAllSamples()
    {
        // Arrange
        var buffer = TestExtensions.RangeOfBytes(1, 100).ToArray();
        var sut = CreateSystemUnderTest();

        // Act
        sut.ReadSamples(buffer);

        // Assert
        this.mockMP3Stream.Received(1).Read(buffer, 0, buffer.Length);
    }

    [Fact]
    public void ReadSamples_With3ParamOverloadAndWhenSamplesReadIsGreaterThanBufferLength_SkipsNonCompleteChunk()
    {
        /* DESCRIPTION:
         * When reading the samples, if the amount of data actually read is greater than or
         * equal to the amount of samples that can fit in the buffer, this means that the rest
         * of the buffer data does not need to be converted to silence and is usable data.
         * This means no crunching sounds will be heard at the end of the audio.
         */
        // Arrange
        var buffer = TestExtensions.RangeOfBytes(1, 199).ToArray();
        MockSampleTimeCalcs(200, 2);

        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.ReadSamples(buffer, 10, 20);

        // Assert
        actual.ShouldBe(200);
    }

    [Fact]
    public void ReadSamples_With3ParamOverloadAndWhenSamplesReadIsLessThanBufferLength_SetLeftOverDataAsSilent()
    {
        /* DESCRIPTION:
         * When reading the samples, if the amount of data actually read is less than the amount
         * of samples that can fit in the buffer, this means that the rest of the buffer data
         * needs to be converted to silence to prevent crunchy sounds at the end of the audio.
         */
        // Arrange
        var buffer = TestExtensions.RangeOfBytes(1, 200).ToArray();
        MockSampleTimeCalcs(175, 2);
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.ReadSamples(buffer, 10, 20);

        // Assert
        actual.ShouldBe(175);
        buffer.Aggregate(0f, (acc, sample) =>
        {
            if (acc >= 176f)
            {
                sample.ShouldBe((byte)0f);
            }

            return acc + 1f;
        });
    }

    [Fact]
    public void ReadUpTo_WhenInvoked_ReadsUpToSetSamplePosition()
    {
        // Arrange
        var buffer = TestExtensions.RangeOfBytes(1, 500).ToArray();
        MockSampleTimeCalcs(1_000, 2);

        this.mockMP3Stream.When(x => x.Read(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>()))
            .Do(Callback.First(FirstAssertCallback)
                .Then(SecondAssertCallback)
                .Then(ThirdAssertCallback));

        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.ReadUpTo(buffer, 250);

        // Assert
        this.mockMP3Stream.Received(2).Dispose();
        actual.ShouldBe(0);

        return;

        void FirstAssertCallback(CallInfo callInfo)
        {
            var bufferArg = callInfo.Arg<byte[]>() ?? [];
            var offsetArg = callInfo.ArgAt<int>(1);
            var upToArg = callInfo.ArgAt<int>(2);

            bufferArg.Length.ShouldBe(8192);
            offsetArg.ShouldBe(0);
            upToArg.ShouldBe(8192);
        }

        void SecondAssertCallback(CallInfo callInfo)
        {
            var bufferArg = callInfo.Arg<byte[]>() ?? [];
            var offsetArg = callInfo.ArgAt<int>(1);
            var upToArg = callInfo.ArgAt<int>(2);

            bufferArg.Length.ShouldBe(250);
            offsetArg.ShouldBe(0);
            upToArg.ShouldBe(250);
        }

        void ThirdAssertCallback(CallInfo callInfo)
        {
            var bufferArg = callInfo.Arg<byte[]>() ?? [];
            var offsetArg = callInfo.ArgAt<int>(1);
            var upToArg = callInfo.ArgAt<int>(2);

            bufferArg.Length.ShouldBe(501);
            offsetArg.ShouldBe(0);
            upToArg.ShouldBe(501);
        }
    }

    [Fact]
    public void Dispose_WhenInvoked_DisposesOfDecoder()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act
        sut.Dispose();
        sut.Dispose();

        // Assert
        this.mockMP3Stream.Received(2).Dispose();
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="Mp3AudioDecoder"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private Mp3AudioDecoder CreateSystemUnderTest()
        => new ("test-path", this.mockMP3Stream);

    private void MockSampleTimeCalcs(int samplesToReturn, int totalAllowedInvokes)
    {
        var readInvokeCount = 0;
        this.mockMP3Stream.ChannelCount.Returns(2);
        this.mockMP3Stream.Frequency.Returns(48_000);

        this.mockMP3Stream.Read(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(_ =>
            {
                readInvokeCount += 1;
                var result = readInvokeCount == totalAllowedInvokes ? samplesToReturn : 0;

                return result;
            });
    }
}
