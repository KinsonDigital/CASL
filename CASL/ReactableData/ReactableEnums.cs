// <copyright file="ReactableEnums.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.ReactableData;

using Data;

/// <summary>
/// Represents the different types of audio commands to send in
/// the <see cref="FullBuffer"/> and <see cref="StreamBuffer"/> types.
/// </summary>
internal enum AudioCommands
{
    /// <summary>
    /// The play command.
    /// </summary>
    Play,

    /// <summary>
    /// The pause command.
    /// </summary>
    Pause,

    /// <summary>
    /// The reset command.
    /// </summary>
    Reset,

    /// <summary>
    /// Enable looping command.
    /// </summary>
    EnableLooping,

    /// <summary>
    /// Disable looping command.
    /// </summary>
    DisableLooping,
}
