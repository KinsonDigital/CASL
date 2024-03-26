// <copyright file="SoundTime.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a time value of a sound.
/// </summary>
/// <remarks>
///     This could represent the current position or the length of a sound.
/// </remarks>
public readonly record struct SoundTime
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SoundTime"/> struct.
    /// </summary>
    /// <param name="totalSeconds">The total number of seconds of the time.</param>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1642:Constructor summary documentation should begin with standard text",
        Justification = "The standard text is a struct, not a class.")]
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
