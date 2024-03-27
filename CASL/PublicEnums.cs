// <copyright file="PublicEnums.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

/// <summary>
/// Represents different audio formats.
/// </summary>
public enum AudioFormat
{
    /// <summary>
    /// 8 bit single channel format.
    /// </summary>
    Mono8 = 1,

    /// <summary>
    /// 16 bit single channel format.
    /// </summary>
    Mono16 = 2,

    /// <summary>
    /// 32 bit single channel format.
    /// </summary>
    MonoFloat32 = 3,

    /// <summary>
    /// 8 bit 2 channel format.
    /// </summary>
    Stereo8 = 4,

    /// <summary>
    /// 16 bit 2 channel format.
    /// </summary>
    Stereo16 = 5,

    /// <summary>
    /// 32 bit floating point 2 channel format.
    /// </summary>
    StereoFloat32 = 6,
}

/// <summary>
/// The state of a sound.
/// </summary>
public enum AudioState
{
    /// <summary>
    /// The state of a sound when it is playing.
    /// </summary>
    Playing = 1,

    /// <summary>
    /// The state of the sound when it is paused.
    /// </summary>
    Paused = 2,

    /// <summary>
    /// The state of the sound when it is stopped.
    /// </summary>
    Stopped = 3,
}

/// <summary>
/// The type of audio to use for audio playback.
/// </summary>
public enum BufferType
{
    /// <summary>
    /// Loads the entire buffer for playback.
    /// </summary>
    Full = 1,

    /// <summary>
    /// Streams the buffer for playback.
    /// </summary>
    Stream = 2,
}

/// <summary>
/// The type of audio format.
/// </summary>
public enum AudioFormatType
{
    /// <summary>
    /// Represents the MP3 audio format.
    /// </summary>
    Mp3 = 1,

    /// <summary>
    /// Represents the Ogg audio format.
    /// </summary>
    Ogg = 2,
}
