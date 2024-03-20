// <copyright file="IAudioFileDecoder.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data.Decoders;

using System;
using OpenAL;

/// <summary>
/// Streams mp3 audio data from a mp3 file.
/// </summary>
/// <typeparam name="T">The type of data from the audio stream.</typeparam>
internal interface IAudioFileDecoder<T> : IDisposable
{
    /// <summary>
    /// Gets the number of audio channels.
    /// </summary>
    int TotalChannels { get; }

    /// <summary>
    /// Gets the audio format.
    /// </summary>
    ALFormat Format { get; }

    /// <summary>
    /// Gets the audio sample rate.
    /// </summary>
    int SampleRate { get; }

    /// <summary>
    /// Gets the total number of samples in the audio stream.
    /// </summary>
    /// <remarks>This includes the samples for all channels.</remarks>
    long TotalSamples { get; }

    /// <summary>
    /// Gets the total number of bytes in the audio.
    /// </summary>
    /// <remarks>
    /// This is not a representation of the byte length
    /// of and audio file.  This represents the byte length of
    /// the uncompressed audio data only.
    /// </remarks>
    long TotalBytes { get; }

    /// <summary>
    /// Gets the total number of seconds.
    /// </summary>
    float TotalSeconds { get; }

    /// <summary>
    /// Gets the total number of sample frames.
    /// </summary>
    /// <remarks>
    ///     A sample frame represents the data of all the channels combined.
    /// </remarks>
    long TotalSampleFrames { get; }

    /// <summary>
    /// Reads audio data starting from the beginning and up to the given <paramref name="upTo"/> samples value.
    /// </summary>
    /// <param name="buffer">The buffer to fill with data.</param>
    /// <param name="upTo">The samples to read up to.</param>
    /// <returns>The total number of samples read.</returns>
    int ReadUpTo(T[] buffer, uint upTo);

    /// <summary>
    /// Reads an amount of samples into the given <paramref name="buffer"/> with the same number of samples
    /// as the length of the <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">The buffer to fill with data.</param>
    /// <returns>The number of samples read.</returns>
    /// <remarks>Invoking this simply fills the <paramref name="buffer"/> param with data.</remarks>
    int ReadSamples(T[] buffer);

    /// <summary>
    ///     Reads an amount of samples starting at the given <paramref name="offset"/>
    ///     and for a number of samples by the given <paramref name="count"/>.
    /// </summary>
    /// <param name="buffer">The buffer to fill with data.</param>
    /// <param name="offset">The starting location of where to read sample data.</param>
    /// <param name="count">The number of samples to read relative to the <paramref name="offset"/>.</param>
    /// <returns>The number of samples read.</returns>
    /// <remarks>Invoking this simply fills the <paramref name="buffer"/> param with data.</remarks>
    int ReadSamples(T[] buffer, int offset, int count);

    /// <summary>
    /// Flushes the reader back to the beginning.
    /// </summary>
    void Flush();
}
