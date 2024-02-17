// <copyright file="AudioData.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data;

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Holds data related to a single sound.
/// </summary>
/// <typeparam name="T">The type of buffer data of the sound.</typeparam>
internal readonly record struct AudioData<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AudioData{T}"/> struct.
    /// </summary>
    /// <param name="bufferData">The buffer data.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="channels">The total number of channels.</param>
    /// <param name="format">The audio format.</param>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1642:Constructor summary documentation should begin with standard text",
        Justification = "Incorrectly flagging this as a violation")]
    public AudioData(T[] bufferData, int sampleRate, int channels, AudioFormat format)
        : this()
    {
        BufferData = new ReadOnlyCollection<T>(bufferData);
        SampleRate = sampleRate;
        Channels = channels;
        Format = format;
    }

    /// <summary>
    /// Gets the buffer.
    /// </summary>
    public ReadOnlyCollection<T> BufferData { get; init; }

    /// <summary>
    /// Gets the rate that samples are read in the audio file.
    /// </summary>
    /// <remarks>
    ///     This would the be frequency in Hz of how many samples are read per second.
    /// </remarks>
    public int SampleRate { get; init; }

    /// <summary>
    /// Gets the total number of channels.
    /// </summary>
    public int Channels { get; init; }

    /// <summary>
    /// Gets the audio format of the audio file.
    /// </summary>
    public AudioFormat Format { get; init; }
}
