// <copyright file="AudioTimeTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests;

using CASL;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests the <see cref="AudioTime"/> struct.
/// </summary>
public class AudioTimeTests
{
    #region Ctor Tests
    [Fact]
    public void Ctor_WhenInvoked_ProperlySetsPropValues()
    {
        // Arrange
        const int expectedMilliseconds = 90000;
        const int expectedSeconds = 30;
        const float expectedMinutes = 1.5f;
        const int expectedTotalSeconds = 90;

        // Act
        var time = new AudioTime(90);

        // Assert
        time.Milliseconds.Should().Be(expectedMilliseconds);
        time.Seconds.Should().Be(expectedSeconds);
        time.Minutes.Should().Be(expectedMinutes);
        time.TotalSeconds.Should().Be(expectedTotalSeconds);
    }
    #endregion
}
