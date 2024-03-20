// <copyright file="IStreamBufferManager.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data;

using System;
using System.Collections.Generic;

/// <summary>
/// Manages a stream of audio data buffers.
/// </summary>
internal interface IStreamBufferManager
{
    /// <summary>
    /// Gets the current sample position in the audio buffer stream.
    /// </summary>
    /// <returns>The current position in samples.</returns>
    long GetCurrentSamplePos();

    /// <summary>
    /// Resets the sample position to the beginning of the audio buffer stream.
    /// </summary>
    void ResetSamplePos();

    /// <summary>
    /// Sets the sample position in the audio buffer stream.
    /// </summary>
    /// <param name="value">The sample position.</param>
    void SetSamplePos(long value);

    /// <summary>
    /// Converts the given position in seconds to an equivalent position in samples.
    /// </summary>
    /// <param name="posSeconds">The position in seconds.</param>
    /// <param name="totalSeconds">The total number of seconds in the audio.</param>
    /// <param name="totalSampleFrames">The total number of audio sample frames in the audio.</param>
    /// <returns>The position in samples.</returns>
    /// <remarks>
    ///     A sample frame represents the data of all the channels combined.
    /// </remarks>
    public long ToPositionSamples(float posSeconds, float totalSeconds, long totalSampleFrames);

    /// <summary>
    /// Converts the given position in samples to an equivalent position in seconds.
    /// </summary>
    /// <param name="totalSampleFrames">The total number of audio sample frames in the audio.</param>
    /// <param name="totalSeconds">The total number of seconds in the audio.</param>
    /// <returns>The position in seconds.</returns>
    /// <remarks>
    ///     A sample frame represents the data of all the channels combined.
    /// </remarks>
    public float ToPositionSeconds(long totalSampleFrames, float totalSeconds);

    /// <summary>
    /// Manages the audio buffer stream.
    /// </summary>
    /// <param name="bufferStats">The buffer stats.</param>
    /// <param name="readSamples">Returns the audio samples.</param>
    /// <typeparam name="T">The type of audio sample data.</typeparam>
    /// <remarks><typeparamref name="T"/> will be byte for mp3 and float for ogg.</remarks>
    void ManageBuffers<T>(BufferStats bufferStats, Func<T[]> readSamples)
        where T : unmanaged;

    /// <summary>
    /// Unqueues all processed buffers from the audio source.
    /// </summary>
    /// <param name="srcId">The sound source id.</param>
    void UnqueueProcessedBuffers(uint srcId);

    /// <summary>
    /// Fills the buffers from the beginning of the audio data.
    /// </summary>
    /// <param name="bufferStats"></param>
    /// <param name="bufferIds"></param>
    /// <param name="flushData"></param>
    /// <param name="readSamples"></param>
    void FillBuffersFromStart<T>(BufferStats bufferStats, IEnumerable<uint> bufferIds, Action flushData, Func<T[]> readSamples)
        where T : unmanaged;
}
