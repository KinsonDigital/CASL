// <copyright file="AudioTime.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a time value of a audio.
/// </summary>
/// <remarks>
///     This could represent the current position or the length of a audio.
/// </remarks>
public readonly record struct AudioTime
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AudioTime"/> struct.
    /// </summary>
    /// <param name="totalSeconds">The total number of seconds of the time.</param>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1642:Constructor summary documentation should begin with standard text",
        Justification = "The standard text is a struct, not a class.")]
    public AudioTime(float totalSeconds)
    {
        Milliseconds = totalSeconds * 1000f;
        Seconds = totalSeconds % 60f;
        Minutes = totalSeconds / 60f;
    }

    /// <summary>
    /// Gets the milliseconds of the audio.
    /// </summary>
    public float Milliseconds { get; }

    /// <summary>
    /// Gets the seconds of the audio.
    /// </summary>
    public float Seconds { get; }

    /// <summary>
    /// Gets the minutes of the audio.
    /// </summary>
    public float Minutes { get; }

    /// <summary>
    /// Gets the total number of seconds of the audio.
    /// </summary>
    public float TotalSeconds => Minutes * 60f;
}
