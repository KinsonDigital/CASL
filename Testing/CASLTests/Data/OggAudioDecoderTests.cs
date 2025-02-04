// <copyright file="OggAudioDecoderTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Data;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CASL.Data.Decoders;
using CASL.OpenAL;
using CASL.Wrappers;
using FluentAssertions;
using Helpers;
using NSubstitute;
using Xunit;

/// <summary>
/// Tests the <see cref="OggAudioDecoder"/> class.
/// </summary>
public class OggAudioDecoderTests
{
    private readonly IVorbisReaderWrapper mockVorbisReader;

    /// <summary>
    /// Initializes a new instance of the <see cref="OggAudioDecoderTests"/> class.
    /// </summary>
    public OggAudioDecoderTests() => this.mockVorbisReader = Substitute.For<IVorbisReaderWrapper>();

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullFilePathParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new OggAudioDecoder(null, this.mockVorbisReader);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'filePath')");
    }

    [Fact]
    public void Ctor_WithEmptyFilePathParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new OggAudioDecoder(string.Empty, this.mockVorbisReader);
        };

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("The value cannot be an empty string. (Parameter 'filePath')");
    }

    [Fact]
    public void Ctor_WithNullVorbisReaderWrapperParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => new OggAudioDecoder("test-path", null);

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("vorbisReaderWrapper");
    }
    #endregion

    #region Property Tests
    [Fact]
    public void TotalChannels_WhenGettingValue_ReturnsCorrectResult()
    {
        // Arrange
        this.mockVorbisReader.Channels.Returns(123);
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.TotalChannels;

        // Assert
        actual.Should().Be(123);
    }

    [Theory]
    [InlineData(1, ALFormat.MonoFloat32Ext)]
    [InlineData(2, ALFormat.StereoFloat32Ext)]
    internal void Format_WhenGettingValue_ReturnsCorrectFormat(int channels, ALFormat expected)
    {
        // Arrange
        this.mockVorbisReader.Channels.Returns(channels);
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.Format;

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1202:'public' members should come before 'internal' members",
        Justification = "The standard text is a struct, not a class.")]
    public void SampleRate_WhenGettingValue_ReturnsCorrectResult()
    {
        // Arrange
        this.mockVorbisReader.SampleRate.Returns(789);
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.SampleRate;

        // Assert
        actual.Should().Be(789);
    }

    [Fact]
    public void TotalSampleFrames_WhenGettingValue_ReturnsCorrectResult()
    {
        // Arrange
        this.mockVorbisReader.TotalSamples.Returns(100);
        this.mockVorbisReader.Channels.Returns(2);
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.TotalSampleFrames;

        // Assert
        actual.Should().Be(100L);
    }

    [Fact]
    public void TotalBytes_WhenInvoked_ReturnsCorrectResult()
    {
        // Arrange
        this.mockVorbisReader.TotalSamples.Returns(1000);
        this.mockVorbisReader.Channels.Returns(2);

        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.TotalBytes;

        // Assert
        actual.Should().Be(8000);
    }

    [Fact]
    public void TotalSeconds_WhenInvoked_ReturnsCorrectResult()
    {
        // Arrange
        this.mockVorbisReader.TotalSeconds.Returns(456);
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.TotalSeconds;

        // Assert
        actual.Should().Be(456f);
    }
    #endregion

    #region Method Tests
    [Fact]
    public void Flush_WhenInvoked_FlushesReader()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act
        sut.Flush();

        // Assert
        this.mockVorbisReader.Received(1).Dispose();
        this.mockVorbisReader.Received(2).Load("test-path");
    }

    [Fact]
    public void ReadUpTo_WhenInvoked_ReadsDataUpToGivenAmount()
    {
        // Arrange
        var buffer = new[] { 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f };

        this.mockVorbisReader.ReadSamples(Arg.Any<float[]>(), Arg.Any<int>(), Arg.Any<int>());
        var sut = CreateSystemUnderTest();

        // Act
        sut.ReadUpTo(buffer, 4U);

        // Assert
        this.mockVorbisReader.Received(1).Dispose();
        this.mockVorbisReader.Received(2).Load("test-path");
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
        var buffer = TestExtensions.RangeOfFloats(1f, 200).ToArray();
        this.mockVorbisReader.ReadSamples(Arg.Any<float[]>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(200);

        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.ReadSamples(buffer, 10, 20);

        // Assert
        actual.Should().Be(200);
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
        var buffer = TestExtensions.RangeOfFloats(1f, 200).ToArray();
        this.mockVorbisReader.ReadSamples(Arg.Any<float[]>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(175);

        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.ReadSamples(buffer, 10, 20);

        // Assert
        actual.Should().Be(175);
        buffer.Should().AllSatisfy(sample =>
        {
            if (sample <= 175)
            {
                sample.Should().Be(sample);
            }
            else
            {
                sample.Should().Be(0);
            }
        });
    }

    [Fact]
    public void ReadSamples_With1ParamOverload_ReadsAllSamples()
    {
        // Arrange
        var buffer = TestExtensions.RangeOfFloats(1f, 100).ToArray();
        var sut = CreateSystemUnderTest();

        // Act
        sut.ReadSamples(buffer);

        // Assert
        this.mockVorbisReader.Received(1).ReadSamples(buffer, 0, buffer.Length);
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
        this.mockVorbisReader.Received(1).Dispose();
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="OggAudioDecoder"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private OggAudioDecoder CreateSystemUnderTest()
        => new ("test-path", this.mockVorbisReader);
}
