// <copyright file="IAudioDecoder.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data.Decoders;

using System;
using OpenAL;

/// <summary>
/// Represents an audio decoder.
/// </summary>
internal interface IAudioDecoder : IDisposable
{
    /// <summary>
    /// Gets the total number of seconds of audio.
    /// </summary>
    float TotalSeconds { get; }

    /// <summary>
    /// Gets the total number of channels in the audio.
    /// </summary>
    /// <remarks>
    ///     Common types are mono (1 channel) and stereo (2 channels).
    /// </remarks>
    int TotalChannels { get; }

    /// <summary>
    /// Gets the audio sample rate.
    /// </summary>
    /// <remarks>
    ///     This represents the number of samples per second in hertz.
    /// </remarks>
    int SampleRate { get; }

    /// <summary>
    /// Gets the total number of samples in the audio.
    /// </summary>
    /// <remarks>
    ///     This does not represent total bytes.  Each sample always has more than one byte
    ///     but this depends on the type of audio data as well as the number of channels.
    /// </remarks>
    long TotalSamples { get; }

    /// <summary>
    /// Gets the total number of sample frames.
    /// </summary>
    /// <remarks>
    ///     A sample frame is all of the samples that consist of all the channels combined.
    /// </remarks>
    long TotalSampleFrames { get; }

    /// <summary>
    /// Gets the format of the audio.
    /// </summary>
    ALFormat Format { get; }

    /// <summary>
    /// Reads audio data starting from the beginning and up to the given <paramref name="upTo"/> samples value.
    /// </summary>
    /// <param name="upTo">The samples to read up to.</param>
    /// <returns>The total number of samples read.</returns>
    /// <remarks>
    ///     The data can be retrieved using the <see cref="GetSampleData{T}"/> method.
    /// </remarks>
    int ReadUpTo(uint upTo);

    /// <summary>
    /// Reads the from the audio.
    /// </summary>
    /// <returns>The total number of samples read.</returns>
    /// <remarks>
    ///     The data can be retrieved using the <see cref="GetSampleData{T}"/> method.
    /// </remarks>
    int ReadSamples();

    /// <summary>
    /// Reads all of the samples from the audio.
    /// </summary>
    /// <remarks>
    ///     The data can be retrieved using the <see cref="GetSampleData{T}"/> method.
    /// </remarks>
    void ReadAllSamples();

    /// <summary>
    /// Returns the audio sample data as an array of the given type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of each data item.</typeparam>
    /// <returns>The audio sample data.</returns>
    /// <remarks>
    ///     For MP3 data this would be bytes and for OGG data this would be float.
    /// </remarks>
    T[] GetSampleData<T>();

    /// <summary>
    /// Flushes the audio data.
    /// </summary>
    void Flush();
}
