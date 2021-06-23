// <copyright file="SoundSource.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL
{
    /// <summary>
    /// Represents a single OpenAL sound source.
    /// </summary>
    internal struct SoundSource
    {
        /// <summary>
        /// The OpenAL id of the sound source.
        /// </summary>
        public uint SourceId;

        /// <summary>
        /// The total number of seconds of the sound.
        /// </summary>
        public float TotalSeconds;
    }
}
