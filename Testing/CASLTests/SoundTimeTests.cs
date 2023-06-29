// <copyright file="SoundTimeTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests;

using CASL;
using Xunit;

/// <summary>
/// Tests the <see cref="SoundTime"/> struct.
/// </summary>
public class SoundTimeTests
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
        var time = new SoundTime(90);

        // Assert
        Assert.Equal(expectedMilliseconds, time.Milliseconds);
        Assert.Equal(expectedSeconds, time.Seconds);
        Assert.Equal(expectedMinutes, time.Minutes);
        Assert.Equal(expectedTotalSeconds, time.TotalSeconds);
    }
    #endregion
}
