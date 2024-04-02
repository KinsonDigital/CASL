// <copyright file="AudioCommandData.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.ReactableData;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents an audio command for an OpenAL audio source.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "No logic to test.")]
internal readonly record struct AudioCommandData
{
    /// <summary>
    /// Gets the id of the audio source.
    /// </summary>
    public uint SourceId { get; init; }

    /// <summary>
    /// Gets the command to execute on the audio source.
    /// </summary>
    public AudioCommands Command { get; init; }
}
