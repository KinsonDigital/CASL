﻿// <copyright file="AudioDataTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Data;

using CASL;
using CASL.Data;
using Xunit;
using FluentAssertions;

/// <summary>
/// Tests the <see cref="AudioData{T}"/> struct.
/// </summary>
public class AudioDataTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WhenInvoked_SetsPropertiesToCorrectValues()
    {
        // Act
        var data = new AudioData<float>(
            new[] { 1f },
            44100,
            2,
            AudioFormat.Stereo16);

        // Assert
        data.BufferData.Should().BeEquivalentTo(new[] { 1f });
        data.SampleRate.Should().Be(44100);
        data.Channels.Should().Be(2);
        data.Format.Should().Be(AudioFormat.Stereo16);
    }
    #endregion
}