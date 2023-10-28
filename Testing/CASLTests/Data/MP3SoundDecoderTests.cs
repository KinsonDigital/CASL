// <copyright file="MP3SoundDecoderTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Data;

using System;
using System.Collections.ObjectModel;
using CASL;
using CASL.Data;
using Moq;
using Xunit;
using FluentAssertions;

/// <summary>
/// Tests the <see cref="MP3SoundDecoder"/> class.
/// </summary>
public class MP3SoundDecoderTests
{
    private readonly Mock<IAudioDataStream<byte>> mockDataStream;

    /// <summary>
    /// Initializes a new instance of the <see cref="MP3SoundDecoderTests"/> class.
    /// </summary>
    public MP3SoundDecoderTests() => this.mockDataStream = new Mock<IAudioDataStream<byte>>();

    #region Method Tests
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void LoadData_WhenFileNameIsEmptyOrNull_ThrowsException(string fileName)
    {
        // Arrange
        var decoder = new MP3SoundDecoder(this.mockDataStream.Object);

        // Act
        var action = () => decoder.LoadData(fileName);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("The param must not be null or empty. (Parameter 'fileName')");
    }

    [Fact]
    public void LoadData_WhenUsingFileNameWithWrongFileExtension_ThrowsException()
    {
        // Arrange
        var decoder = new MP3SoundDecoder(this.mockDataStream.Object);

        // Act
        var action = () => decoder.LoadData("sound.wav");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("The file name must have an mp3 file extension. (Parameter 'fileName')");
    }

    [Fact]
    public unsafe void LoadData_WhenInvoked_ReturnsCorrectResult()
    {
        // Arrange
        var bufferData = new byte[2];

        this.mockDataStream.SetupGet(p => p.SampleRate).Returns(1);
        this.mockDataStream.SetupGet(p => p.Channels).Returns(1);
        this.mockDataStream.SetupGet(p => p.Format).Returns(AudioFormat.Stereo16);
        this.mockDataStream.Setup(m => m.ReadSamples(bufferData, 0, It.IsAny<int>()))
            .Returns<byte[], int, int>((buffer, _, _) =>
            {
                fixed (byte* pBuffer = buffer)
                {
                    pBuffer[0] = 10;
                    pBuffer[1] = 20;
                }

                return 2;
            });

        var decoder = new MP3SoundDecoder(this.mockDataStream.Object);
        var expected = new SoundData<byte>
        {
            BufferData = new ReadOnlyCollection<byte>(new byte[] { 10, 20 }),
            Channels = 1,
            Format = AudioFormat.Stereo16,
            SampleRate = 1,
        };

        // Act
        var actual = decoder.LoadData("sound.mp3");

        // Assert
        actual.Should().Be(expected);
        this.mockDataStream.Verify(m => m.ReadSamples(new byte[] { 10, 20 }, 0, 2), Times.Exactly(2));
    }

    [Fact]
    public void Dispose_WhenInvoked_ProperlyDisposesDecoder()
    {
        // Arrange
        var decoder = new MP3SoundDecoder(this.mockDataStream.Object);

        // Act
        decoder.Dispose();
        decoder.Dispose();

        // Assert
        this.mockDataStream.Verify(m => m.Dispose(), Times.Once());
    }
    #endregion
}
