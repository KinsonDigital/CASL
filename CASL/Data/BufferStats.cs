// <copyright file="BufferStats.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data;

using System.Diagnostics.CodeAnalysis;
using OpenAL;

/// <summary>
/// Represents stats about an audio buffer.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "No logic to test.")]
internal readonly record struct BufferStats
{
    /// <summary>
    /// Gets the OpenAL source ID.
    /// </summary>
    public uint SourceId { get; init; }

    /// <summary>
    /// Gets the type of audio format.
    /// </summary>
    public AudioFormatType FormatType { get; init; }

    /// <summary>
    /// Gets the OpenAL format type.
    /// </summary>
    public ALFormat DecoderFormat { get; init; }

    /// <summary>
    /// Gets the sample rate of the audio.
    /// </summary>
    public int SampleRate { get; init; }

    /// <summary>
    /// Gets the total number of channels in the audio.
    /// </summary>
    public int TotalChannels { get; init; }
}
