// <copyright file="OggSoundDecoderTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Data;

using System;
using System.Collections.ObjectModel;
using CASL;
using CASL.Data;
using CASL.Data.Exceptions;
using Moq;
using Xunit;
using Assert = Helpers.AssertExtensions;
using FluentAssertions;

/// <summary>
/// Tests the <see cref="OggSoundDecoder"/> class.
/// </summary>
public class OggSoundDecoderTests
{
    private readonly Mock<IAudioDataStream<float>> mockDataStream;

    /// <summary>
    /// Initializes a new instance of the <see cref="OggSoundDecoderTests"/> class.
    /// </summary>
    public OggSoundDecoderTests() => this.mockDataStream = new Mock<IAudioDataStream<float>>();

    #region Method Tests
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void LoadData_WhenFileNameIsEmptyOrNull_ThrowsException(string fileName)
    {
        // Arrange
        var decoder = new OggSoundDecoder(this.mockDataStream.Object);

        // Act
        var action = () => decoder.LoadData(fileName);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("The param must not be null or empty. (Parameter 'fileName')");
    }

    [Fact]
    public void LoadData_WhenUsingFileNameWithWrongFileExtension_ThrowsException()
    {
        // Arrange
        var decoder = new OggSoundDecoder(this.mockDataStream.Object);

        // Act
        var action = () => decoder.LoadData("sound.wav");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("The file name must have an ogg file extension. (Parameter 'fileName')");
    }

    [Fact]
    public void LoadData_WhenUsingInvalidFormat_ThrowsException()
    {
        // Arrange
        this.mockDataStream.SetupGet(p => p.Channels).Returns(1234);
        var decoder = new OggSoundDecoder(this.mockDataStream.Object);

        // Act
        var action = () => decoder.LoadData("sound.ogg");

        // Assert
        action.Should().Throw<SoundDataException>().WithMessage("Only supported formats are Mono 32-bit and Stereo 32-bit.");
    }

    [Theory]
    [InlineData(AudioFormat.MonoFloat32, 1, new[] { 10f })]
    [InlineData(AudioFormat.StereoFloat32, 2, new[] { 10f, 20f })]
    public unsafe void LoadData_WhenInvoked_ReturnsCorrectResult(AudioFormat format, int channels, float[] bufferData)
    {
        // Arrange
        var invokeCount = 0;
        this.mockDataStream.SetupGet(p => p.SampleRate).Returns(1);
        this.mockDataStream.SetupGet(p => p.Channels).Returns(channels);
        this.mockDataStream.SetupGet(p => p.Format).Returns(format);
        this.mockDataStream.Setup(m => m.ReadSamples(It.IsAny<float[]>(), 0, It.IsAny<int>()))
            .Returns<float[], int, int>((buffer, _, _) =>
            {
                // This prevents the while loop from continuing forever.
                // This simulates that the data is finished being read
                if (invokeCount == 1)
                {
                    return 0;
                }

                fixed (float* pBuffer = buffer)
                {
                    switch (channels)
                    {
                        case 1:
                            pBuffer[0] = 10f;
                            break;
                        case 2:
                            pBuffer[0] = 10f;
                            pBuffer[1] = 20f;
                            break;
                    }
                }

                invokeCount += 1;

                return 1;
            });

        var decoder = new OggSoundDecoder(this.mockDataStream.Object);
        var expected = new SoundData<float>
        {
            BufferData = new ReadOnlyCollection<float>(bufferData),
            Channels = channels,
            Format = format,
            SampleRate = 1,
        };

        // Act
        var actual = decoder.LoadData("sound.ogg");

        // Assert
        actual.Should().Be(expected);
        this.mockDataStream.Verify(m => m.ReadSamples(It.IsAny<float[]>(), 0, channels), Times.Exactly(2));
    }

    [Fact]
    public void Dispose_WhenInvoked_ProperlyDisposesDecoder()
    {
        // Arrange
        var decoder = new OggSoundDecoder(this.mockDataStream.Object);

        // Act
        decoder.Dispose();
        decoder.Dispose();

        // Assert
        this.mockDataStream.Verify(m => m.Dispose(), Times.Once());
    }
    #endregion
}
