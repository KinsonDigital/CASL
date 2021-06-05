// <copyright file="ISound.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL
{
    using System;

    /// <summary>
    /// A single sound that can be played, paused etc.
    /// </summary>
    public interface ISound : IContent
    {
        /// <summary>
        /// Gets or sets the volume of the sound.
        /// </summary>
        /// <remarks>
        ///     The only valid value accepted is 0-100. If a value outside of
        ///     this range is used, it will be set within that range.
        /// </remarks>
        float Volume { get; set; }

        /// <summary>
        /// Gets the current time position of the sound.
        /// </summary>
        SoundTime Position { get; }

        /// <summary>
        /// Gets the length of the song.
        /// </summary>
        SoundTime Length { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the sound loops back to the beginning once the end has been reached.
        /// </summary>
        bool IsLooping { get; set; }

        /// <summary>
        /// Gets the state of the sound.
        /// </summary>
        SoundState State { get; }

        /// <summary>
        /// Gets or sets the play speed to the given value.
        /// </summary>
        /// <param name="value">The speed that the sound should play at.</param>
        /// <remarks>
        ///     The valid range of <paramref name="value"/> is between 0.25 and 2.0
        ///     with a <paramref name="value"/> less then 0.25 defaulting to 0.25 and
        ///     with a <paramref name="value"/> greater then 2.0 defaulting to 2.0.
        /// </remarks>
        float PlaySpeed { get; set; }

        /// <summary>
        /// Plays the sound.
        /// </summary>
        void Play();

        /// <summary>
        /// Pauses the sound.
        /// </summary>
        void Pause();

        /// <summary>
        /// Stops the sound.
        /// </summary>
        /// <remarks>This will set the time position back to the beginning.</remarks>
        void Stop();

        /// <summary>
        /// Resets the sound.
        /// </summary>
        /// <remarks>
        ///     This will stop the sound and set the time position back to the beginning.
        /// </remarks>
        void Reset();

        /// <summary>
        /// Sets the time position of the sound to the given value.
        /// </summary>
        /// <param name="seconds">The time position in seconds of where to set the sound.</param>
        void SetTimePosition(float seconds);

        /// <summary>
        /// Rewinds the sound by the given amound of <paramref name="seconds"/>.
        /// </summary>
        /// <param name="seconds">The amount of seconds to rewind the song.</param>
        void Rewind(float seconds);

        /// <summary>
        /// Fast forwards the sound by the given amount of <paramref name="seconds"/>.
        /// </summary>
        /// <param name="seconds">The amount of seconds to fast forward the song.</param>
        void FastForward(float seconds);
    }
}
