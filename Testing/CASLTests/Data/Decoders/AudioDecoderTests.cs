// <copyright file="AudioDecoderTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Data.Decoders;

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using CASL;
using CASL.Data.Decoders;
using CASL.Exceptions;
using CASL.Factories;
using CASL.OpenAL;
using FluentAssertions;
using Helpers;
using NSubstitute;
using Xunit;

/// <summary>
/// Tests the <see cref="AudioDecoder"/> class.
/// </summary>
public class AudioDecoderTests
{
    private readonly IAudioDecoderFactory mockDecoderFactory;
    private readonly IPath mockPath;
    private readonly IFile mockFile;
    private readonly IAudioFileDecoder<byte> mockMp3AudioFileDecoder;
    private readonly IAudioFileDecoder<float> mockOggAudioFileDecoder;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDecoderTests"/> class.
    /// </summary>
    public AudioDecoderTests()
    {
        this.mockMp3AudioFileDecoder = Substitute.For<IAudioFileDecoder<byte>>();
        this.mockOggAudioFileDecoder = Substitute.For<IAudioFileDecoder<float>>();

        this.mockDecoderFactory = Substitute.For<IAudioDecoderFactory>();
        this.mockDecoderFactory.CreateMp3AudioDecoder(Arg.Any<string>()).Returns(this.mockMp3AudioFileDecoder);
        this.mockDecoderFactory.CreateOggAudioDecoder(Arg.Any<string>()).Returns(this.mockOggAudioFileDecoder);

        this.mockPath = Substitute.For<IPath>();
        this.mockFile = Substitute.For<IFile>();
    }

    #region Constructor
    [Fact]
    public void Ctor_WithNullFilePath_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new AudioDecoder(
                null,
                this.mockDecoderFactory,
                this.mockPath,
                this.mockFile);
        };

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("filePath");
    }

    [Fact]
    public void Ctor_WithEmptyFilePath_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new AudioDecoder(
                string.Empty,
                this.mockDecoderFactory,
                this.mockPath,
                this.mockFile);
        };

        // Assert
        act.Should().ThrowArgException().WithEmptyStringParamMsg("filePath");
    }

    [Fact]
    public void Ctor_WithNullDataDecoderFactoryParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new AudioDecoder(
                "test-file.ogg",
                null,
                this.mockPath,
                this.mockFile);
        };

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("dataDecoderFactory");
    }

    [Fact]
    public void Ctor_WithNullPathParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new AudioDecoder(
                "test-file.ogg",
                this.mockDecoderFactory,
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
            _ = new AudioDecoder(
                "test-file.ogg",
                this.mockDecoderFactory,
                this.mockPath,
                null);
        };

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("file");
    }

    [Fact]
    public void Ctor_WithMp3AudioFile_CreatesDecoder()
    {
        // Arrange & Act
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".mp3");
        this.mockMp3AudioFileDecoder.TotalSeconds.Returns(123);
        this.mockMp3AudioFileDecoder.TotalChannels.Returns(1);
        this.mockMp3AudioFileDecoder.TotalSamples.Returns(1234);
        this.mockMp3AudioFileDecoder.Format.Returns(ALFormat.Mono16);
        this.mockMp3AudioFileDecoder.SampleRate.Returns(4321);
        this.mockMp3AudioFileDecoder.TotalSampleFrames.Returns(617);
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".mp3");
        var sut = CreateSystemUnderTest("test-file.mp3");

        // Assert
        this.mockDecoderFactory.CreateMp3AudioDecoder("test-file.mp3");
        sut.TotalSeconds.Should().Be(123);
        sut.TotalChannels.Should().Be(1);
        sut.TotalSamples.Should().Be(1234);
        sut.Format.Should().Be(ALFormat.Mono16);
        sut.SampleRate.Should().Be(4321);
        sut.TotalSampleFrames.Should().Be(617);
    }

    [Fact]
    public void Ctor_WithOggAudioFile_CreatesDecoder()
    {
        // Arrange & Act
        this.mockOggAudioFileDecoder.TotalSeconds.Returns(123);
        this.mockOggAudioFileDecoder.TotalChannels.Returns(2);
        this.mockOggAudioFileDecoder.TotalSamples.Returns(1234);
        this.mockOggAudioFileDecoder.Format.Returns(ALFormat.StereoFloat32Ext);
        this.mockOggAudioFileDecoder.SampleRate.Returns(4321);
        this.mockOggAudioFileDecoder.TotalSampleFrames.Returns(617);
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");
        var sut = CreateSystemUnderTest("test-file.ogg");

        // Assert
        this.mockDecoderFactory.CreateMp3AudioDecoder("test-file.ogg");
        sut.TotalSeconds.Should().Be(123);
        sut.TotalChannels.Should().Be(2);
        sut.TotalSamples.Should().Be(1234);
        sut.Format.Should().Be(ALFormat.StereoFloat32Ext);
        sut.SampleRate.Should().Be(4321);
        sut.TotalSampleFrames.Should().Be(617);
    }

    [Fact]
    public void Ctor_WithInvalidExtension_ThrowsException()
    {
        // Arrange & Act
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".invalid");
        var expected = "The file extension '.invalid' is not supported.  Supported extensions are: '.mp3', '.ogg'.";
        var act = () => CreateSystemUnderTest("test-file.invalid");

        // Assert
        act.Should().Throw<AudioException>(expected);
    }
    #endregion

    #region Method Tests
    [Fact]
    public void ReadUpTo_WithInvalidAudioFormatType_ThrowsException()
    {
        // Arrange
        var expected = "The value of argument 'this.audioFormatType' (1000) is invalid for Enum type 'AudioFormatType'.";
        expected += " (Parameter 'this.audioFormatType')";

        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");

        var sut = CreateSystemUnderTest("test-file.ogg");
        sut.SetEnumFieldValue("audioFormatType", (AudioFormatType)1000);

        // Act
        var act = () => sut.ReadUpTo(123);

        // Assert
        act.Should().Throw<InvalidEnumArgumentException>()
            .WithMessage(expected);
    }

    [Theory]
    [InlineData(0, 4096)]
    [InlineData(1, 4096)]
    [InlineData(2, 8192)]
    public void ReadUpTo_WhenReadingMp3Data_ReadsData(int totalChannels, int bufferSize)
    {
        // Arrange
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".mp3");
        this.mockMp3AudioFileDecoder.TotalChannels.Returns(totalChannels);
        var sut = CreateSystemUnderTest("test-file.mp3");

        // Act
        sut.ReadUpTo(123);

        // Assert
        this.mockMp3AudioFileDecoder.Received(1).ReadUpTo(Arg.Is<byte[]>(arg => arg.Length == bufferSize), 123);
        this.mockOggAudioFileDecoder.DidNotReceive().ReadUpTo(Arg.Any<float[]>(), Arg.Any<uint>());
    }

    [Theory]
    [InlineData(0, 4096)]
    [InlineData(1, 4096)]
    [InlineData(2, 8192)]
    public void ReadUpTo_WhenReadingOggData_ReadsData(int totalChannels, int bufferSize)
    {
        // Arrange
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");
        this.mockOggAudioFileDecoder.TotalChannels.Returns(totalChannels);
        var sut = CreateSystemUnderTest("test-file.ogg");

        // Act
        sut.ReadUpTo(123);

        // Assert
        this.mockOggAudioFileDecoder.Received(1).ReadUpTo(Arg.Is<float[]>(arg => arg.Length == bufferSize), 123);
        this.mockMp3AudioFileDecoder.DidNotReceive().ReadUpTo(Arg.Any<byte[]>(), Arg.Any<uint>());
    }

    [Fact]
    public void ReadSamples_WithInvalidAudioFormatType_ThrowsException()
    {
        // Arrange
        var expected = "The value of argument 'this.audioFormatType' (1000) is invalid for Enum type 'AudioFormatType'.";
        expected += " (Parameter 'this.audioFormatType')";

        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");

        var sut = CreateSystemUnderTest("test-file.ogg");
        sut.SetEnumFieldValue("audioFormatType", (AudioFormatType)1000);

        // Act
        var act = () => sut.ReadSamples();

        // Assert
        act.Should().Throw<InvalidEnumArgumentException>()
            .WithMessage(expected);
    }

    [Fact]
    public void ReadSamples_WithNullMp3Buffer_ThrowsException()
    {
        // Arrange
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".mp3");

        var sut = CreateSystemUnderTest("test-file.mp3");
        sut.SetArrayFieldToNull<byte>("mp3Buffer");

        // Act
        var act = () => sut.ReadSamples();

        // Assert
        act.Should().ThrowArgNullException()
            .WithNullParamMsg("this.mp3Buffer");
    }

    [Fact]
    public void ReadSamples_WithNullOggBuffer_ThrowsException()
    {
        // Arrange
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".Ogg");

        var sut = CreateSystemUnderTest("test-file.Ogg");
        sut.SetArrayFieldToNull<float>("oggBuffer");

        // Act
        var act = () => sut.ReadSamples();

        // Assert
        act.Should().ThrowArgNullException()
            .WithNullParamMsg("this.oggBuffer");
    }

    [Theory]
    [InlineData(0, 4096)]
    [InlineData(1, 4096)]
    [InlineData(2, 8192)]
    public void ReadSamples_WhenReadingMp3Data_ReadsData(int totalChannels, int bufferSize)
    {
        // Arrange
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".mp3");
        this.mockMp3AudioFileDecoder.TotalChannels.Returns(totalChannels);
        var sut = CreateSystemUnderTest("test-file.mp3");

        // Act
        sut.ReadSamples();

        // Assert
        this.mockMp3AudioFileDecoder.Received(1)
            .ReadSamples(Arg.Is<byte[]>(arg => arg.Length == bufferSize), 0, bufferSize);
    }

    [Theory]
    [InlineData(0, 4096)]
    [InlineData(1, 4096)]
    [InlineData(2, 8192)]
    public void ReadSamples_WhenReadingOggData_ReadsData(int totalChannels, int bufferSize)
    {
        // Arrange
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".Ogg");
        this.mockOggAudioFileDecoder.TotalChannels.Returns(totalChannels);
        var sut = CreateSystemUnderTest("test-file.Ogg");

        // Act
        sut.ReadSamples();

        // Assert
        this.mockOggAudioFileDecoder.Received(1)
            .ReadSamples(Arg.Is<float[]>(arg => arg.Length == bufferSize), 0, bufferSize);
    }

    [Fact]
    public void ReadAllSamples_WithInvalidAudioFormatType_ThrowsException()
    {
        // Arrange
        var expected = "The value of argument 'this.audioFormatType' (1000) is invalid for Enum type 'AudioFormatType'.";
        expected += " (Parameter 'this.audioFormatType')";

        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");

        var sut = CreateSystemUnderTest("test-file.ogg");
        sut.SetEnumFieldValue("audioFormatType", (AudioFormatType)1000);

        // Act
        var act = () => sut.ReadAllSamples();

        // Assert
        act.Should().Throw<InvalidEnumArgumentException>()
            .WithMessage(expected);
    }

    [Fact]
    public void ReadAllSamples_WithNullMp3Decoder_ThrowsException()
    {
        // Arrange
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".mp3");

        var sut = CreateSystemUnderTest("test-file.mp3");
        sut.SetFieldToNull("mp3DataDecoder");

        // Act
        var act = () => sut.ReadAllSamples();

        // Assert
        act.Should().ThrowArgNullException()
            .WithNullParamMsg("this.mp3DataDecoder");
    }

    [Fact]
    public void ReadAllSamples_WithNullOggDecoder_ThrowsException()
    {
        // Arrange
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");

        var sut = CreateSystemUnderTest("test-file.ogg");
        sut.SetFieldToNull("oggDataDecoder");

        // Act
        var act = () => sut.ReadAllSamples();

        // Assert
        act.Should().ThrowArgNullException()
            .WithNullParamMsg("this.oggDataDecoder");
    }

    [Theory]
    [InlineData(0, 1234)]
    [InlineData(1, 1234)]
    [InlineData(2, 1234)]
    public void ReadAllSamples_WhenReadingMp3Data_ReadsData(int totalChannels, int bufferSize)
    {
        // Arrange
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".mp3");
        this.mockMp3AudioFileDecoder.TotalChannels.Returns(totalChannels);
        this.mockMp3AudioFileDecoder.TotalBytes.Returns(bufferSize);
        var sut = CreateSystemUnderTest("test-file.mp3");

        // Act
        sut.ReadAllSamples();

        // Assert
        this.mockMp3AudioFileDecoder.Received(1).Flush();
        this.mockMp3AudioFileDecoder.Received(1)
            .ReadSamples(Arg.Is<byte[]>(arg => arg.Length == bufferSize));
    }

    [Theory]
    [InlineData(0, 1234)]
    [InlineData(1, 1234)]
    [InlineData(2, 1234)]
    public void ReadAllSamples_WhenReadingOggData_ReadsData(int totalChannels, int bufferSize)
    {
        // Arrange
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");
        this.mockOggAudioFileDecoder.TotalChannels.Returns(totalChannels);
        this.mockOggAudioFileDecoder.TotalSamples.Returns(bufferSize);
        var sut = CreateSystemUnderTest("test-file.ogg");

        // Act
        sut.ReadAllSamples();

        // Assert
        this.mockOggAudioFileDecoder.Received(1).Flush();
        this.mockOggAudioFileDecoder.Received(1)
            .ReadSamples(Arg.Is<float[]>(arg => arg.Length == bufferSize));
    }

    [Fact]
    public void GetSampleData_WithInvalidFormatType_ThrowsException()
    {
        // Arrange
        const string expected = "The value of argument 'this.audioFormatType' (1000) is invalid for Enum type 'AudioFormatType'." +
                                " (Parameter 'this.audioFormatType')";

        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");

        var sut = CreateSystemUnderTest("test-file.ogg");
        sut.SetEnumFieldValue("audioFormatType", (AudioFormatType)1000);

        // Act
        var act = () => sut.GetSampleData<float>();

        // Assert
        act.Should().Throw<InvalidEnumArgumentException>()
            .WithMessage(expected);
    }

    [Fact]
    public void GetSampleData_WhenMp3DataIsNull_ReturnsEmptyArray()
    {
        // Arrange
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".mp3");
        var sut = CreateSystemUnderTest("test-file.mp3");
        sut.SetArrayFieldToNull<byte>("mp3Buffer");

        // Act
        var actual = sut.GetSampleData<byte>();

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeEmpty();
    }

    [Fact]
    public void GetSampleData_WhenOggDataIsNull_ReturnsEmptyArray()
    {
        // Arrange
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");
        var sut = CreateSystemUnderTest("test-file.ogg");
        sut.SetArrayFieldToNull<float>("oggBuffer");

        // Act
        var actual = sut.GetSampleData<float>();

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, 4096)]
    [InlineData(1, 4096)]
    [InlineData(2, 8192)]
    public void GetSampleData_WhenReadingMp3Data_ReturnsData(int totalChannels, int bufferSize)
    {
        // Arrange
        this.mockMp3AudioFileDecoder.TotalChannels.Returns(totalChannels);
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".mp3");
        var sut = CreateSystemUnderTest("test-file.mp3");

        // Act
        var actual = sut.GetSampleData<byte>();

        // Assert
        actual.Should().NotBeNull();
        actual.Should().NotBeEmpty();
        actual.Length.Should().Be(bufferSize);
    }

    [Theory]
    [InlineData(0, 4096)]
    [InlineData(1, 4096)]
    [InlineData(2, 8192)]
    public void GetSampleData_WhenReadingOggData_ReturnsData(int totalChannels, int bufferSize)
    {
        // Arrange
        this.mockOggAudioFileDecoder.TotalChannels.Returns(totalChannels);
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");
        var sut = CreateSystemUnderTest("test-file.ogg");

        // Act
        var actual = sut.GetSampleData<float>();

        // Assert
        actual.Should().NotBeNull();
        actual.Should().NotBeEmpty();
        actual.Length.Should().Be(bufferSize);
    }

    [Fact]
    public void Flush_WithInvalidAudioFormatType_ThrowsException()
    {
        // Arrange
        const string expected = "The value of argument 'this.audioFormatType' (1000) is invalid for Enum type 'AudioFormatType'." +
                                " (Parameter 'this.audioFormatType')";

        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");

        var sut = CreateSystemUnderTest("test-file.ogg");
        sut.SetEnumFieldValue("audioFormatType", (AudioFormatType)1000);

        // Act
        var act = () => sut.Flush();

        // Assert
        act.Should().Throw<InvalidEnumArgumentException>()
            .WithMessage(expected);
    }

    [Fact]
    public void Flush_WhenFlushingMp3Data_FlushesData()
    {
        // Arrange
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".mp3");
        var sut = CreateSystemUnderTest("test-file.mp3");

        // Act
        sut.Flush();

        // Assert
        this.mockMp3AudioFileDecoder.Received(1).Flush();
        this.mockOggAudioFileDecoder.DidNotReceive().Flush();
    }

    [Fact]
    public void Flush_WhenFlushingOggData_FlushesData()
    {
        // Arrange
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(".ogg");
        var sut = CreateSystemUnderTest("test-file.ogg");

        // Act
        sut.Flush();

        // Assert
        this.mockOggAudioFileDecoder.Received(1).Flush();
        this.mockMp3AudioFileDecoder.DidNotReceive().Flush();
    }

    [Theory]
    [InlineData(AudioFormatType.Mp3)]
    [SuppressMessage("csharpsquid", "S3966", Justification = "Need to execute dispose twice for testing.")]
    public void Dispose_WhenInvoked_DisposesOfDecoder(AudioFormatType formatType)
    {
        // Arrange
        var extension = formatType == AudioFormatType.Mp3 ? ".mp3" : ".ogg";
        this.mockPath.GetExtension(Arg.Any<string>()).Returns(extension);
        var sut = CreateSystemUnderTest($"test-file{extension}");

        sut.SetEnumFieldValue("audioFormatType", formatType);

        // Act
        sut.Dispose();
        sut.Dispose();

        // Assert
        this.mockMp3AudioFileDecoder.Received(formatType == AudioFormatType.Mp3 ? 1 : 0).Dispose();
        this.mockOggAudioFileDecoder.Received(formatType == AudioFormatType.Ogg ? 1 : 0).Dispose();
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="AudioDecoder"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private AudioDecoder CreateSystemUnderTest(string filePath)
        => new (filePath,
            this.mockDecoderFactory,
            this.mockPath,
            this.mockFile);
}
