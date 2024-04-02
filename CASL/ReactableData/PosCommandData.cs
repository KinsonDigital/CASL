// <copyright file="PosCommandData.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.ReactableData;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents a position in seconds in some audio.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "No logic to test.")]
internal struct PosCommandData
{
    /// <summary>
    /// Gets the source ID of the audio.
    /// </summary>
    public uint SourceId { get; init; }

    /// <summary>
    /// Gets the position in the audio.
    /// </summary>
    public float PositionSeconds { get; init; }
}
