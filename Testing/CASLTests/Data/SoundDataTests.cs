// <copyright file="SoundDataTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Data;

using System.Collections.ObjectModel;
using System.Linq;
using CASL;
using CASL.Data;
using Xunit;
using FluentAssertions;

/// <summary>
/// Tests the <see cref="SoundData{T}"/> struct.
/// </summary>
public class SoundDataTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WhenInvoked_SetsPropertiesToCorrectValues()
    {
        // Act
        var data = new SoundData<float>(
            new[] { 1f },
            44100,
            2,
            AudioFormat.Stereo16);

        // Assert
        data.BufferData.ToArray().Should().BeEquivalentTo(new[] { 1f });
        data.SampleRate.Should().Be(44100);
        data.Channels.Should().Be(2);
        data.Format.Should().Be(AudioFormat.Stereo16);
    }
    #endregion

    #region Method Tests
    [Fact]
    public void Equals_WhenInvokingOverloadedEqualsOperator_ReturnsCorrectResult()
    {
        // Arrange
        var dataA = new SoundData<float>
        {
            BufferData = new ReadOnlyCollection<float>(new[] { 1f }),
            SampleRate = 44100,
            Channels = 2,
            Format = AudioFormat.Stereo16,
        };

        var dataB = new SoundData<float>
        {
            BufferData = new ReadOnlyCollection<float>(new[] { 1f }),
            SampleRate = 44100,
            Channels = 2,
            Format = AudioFormat.Stereo16,
        };

        // Act
        var actual = dataA == dataB;

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void Equals_WhenInvokingOverloadedNoEqualsOperator_ReturnsCorrectResult()
    {
        // Arrange
        var dataA = new SoundData<float>
        {
            BufferData = new ReadOnlyCollection<float>(new[] { 1f }),
            SampleRate = 44100,
            Channels = 2,
            Format = AudioFormat.Stereo16,
        };

        var dataB = new SoundData<float>
        {
            BufferData = new ReadOnlyCollection<float>(new[] { 1f }),
            SampleRate = 44100,
            Channels = 2,
            Format = AudioFormat.Stereo16,
        };

        // Act
        var actual = dataA != dataB;

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenInvokingWithObjectThatIsNotSoundDataType_ReturnsCorrectResult()
    {
        // Arrange
        var dataA = new SoundData<float>
        {
            BufferData = new ReadOnlyCollection<float>(new[] { 1f }),
            SampleRate = 44100,
            Channels = 2,
            Format = AudioFormat.Stereo16,
        };

        var dataB = new object();

        // Act
        var actual = dataA.Equals(dataB);

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void Equals_WhenInvokingWithObjectSoundDataType_ReturnsCorrectResult()
    {
        // Arrange
        var dataA = new SoundData<float>
        {
            BufferData = new ReadOnlyCollection<float>(new[] { 1f }),
            SampleRate = 44100,
            Channels = 2,
            Format = AudioFormat.Stereo16,
        };

        object dataB = new SoundData<float>
        {
            BufferData = new ReadOnlyCollection<float>(new[] { 1f }),
            SampleRate = 44100,
            Channels = 2,
            Format = AudioFormat.Stereo16,
        };

        // Act
        var actual = dataA.Equals(dataB);

        // Assert
        actual.Should().BeTrue();
    }

    [Theory]
    [InlineData(new[] { 1f }, 44100, 2, AudioFormat.Stereo16, true)]
    [InlineData(new[] { 1f, 2f }, 6000, 2, AudioFormat.Mono16, false)]
    public void Equals_WhenInvokingTypedParam_ReturnsCorrectResult(
        float[] bufferData,
        int sampleRate,
        int channels,
        AudioFormat format,
        bool expected)
    {
        // Arrange
        var dataA = new SoundData<float>
        {
            BufferData = new ReadOnlyCollection<float>(bufferData),
            SampleRate = sampleRate,
            Channels = channels,
            Format = format,
        };

        var dataB = new SoundData<float>
        {
            BufferData = new ReadOnlyCollection<float>(new[] { 1f }),
            SampleRate = 44100,
            Channels = 2,
            Format = AudioFormat.Stereo16,
        };

        // Act
        var actual = dataA.Equals(dataB);

        // Assert
        actual.Should().Be(expected);
    }
    #endregion
}
