// <copyright file="SoundTime.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

/// <summary>
/// Represents a time value of a sound.
/// </summary>
/// <remarks>
///     This could represent the current position or the length of a sound.
/// </remarks>
public struct SoundTime
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SoundTime"/> struct.
    /// </summary>
    /// <param name="totalSeconds">The total number of seconds of the time.</param>
    public SoundTime(float totalSeconds)
    {
        Milliseconds = totalSeconds * 1000f;
        Seconds = totalSeconds % 60f;
        Minutes = totalSeconds / 60f;
    }

    /// <summary>
    /// Gets the milliseconds of the sound.
    /// </summary>
    public float Milliseconds { get; }

    /// <summary>
    /// Gets the seconds of the sound.
    /// </summary>
    public float Seconds { get; }

    /// <summary>
    /// Gets the minutes of the sound.
    /// </summary>
    public float Minutes { get; }

    /// <summary>
    /// Gets the total number of seconds of the sound.
    /// </summary>
    public float TotalSeconds => Minutes * 60f;
}
