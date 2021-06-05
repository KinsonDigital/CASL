﻿// <copyright file="SoundStats.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL
{
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
        public float PlaySpeed;
    }
}
