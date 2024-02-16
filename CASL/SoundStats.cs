// <copyright file="SoundStats.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

/// <summary>
/// Represents the state of a single sound.
/// </summary>
internal struct SoundStats
{
    /// <summary>
    /// The OpenAL id of the sound source that this state links to.
    /// </summary>
    public uint SourceId;

    /// <summary>
    /// The current time position of the sound.
    /// </summary>
    public float TimePosition;

    /// <summary>
    /// The total number of seconds of the sound.
    /// </summary>
    public float TotalSeconds;

    /// <summary>
    /// The current playback state of the sound.
    /// </summary>
    public SoundState PlaybackState;

    /// <summary>
    /// The speed that the sound is playing at.
    /// </summary>
    /// <remarks>
    ///     A value of 1.0 is normal speed.  A value of 2.0 is twice the normal speed.  A value of 0.5 is half the normal speed.
    /// </remarks>
    public float PlaySpeed;
}
