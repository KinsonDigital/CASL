// <copyright file="StreamBufferManager.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using OpenAL;

/// <inheritdoc/>
internal class StreamBufferManager : IStreamBufferManager
{
    private const long ChunkSize = 4096L;
    private readonly IOpenALInvoker alInvoker;
    private long currentSamplePos;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamBufferManager"/> class.
    /// </summary>
    /// <param name="alInvoker">Provides access to OpenAL.</param>
    public StreamBufferManager(IOpenALInvoker alInvoker)
    {
        ArgumentNullException.ThrowIfNull(alInvoker);

        this.alInvoker = alInvoker;
    }

    /// <inheritdoc/>
    public long GetCurrentSamplePos() => this.currentSamplePos;

    /// <inheritdoc/>
    public void ResetSamplePos() => this.currentSamplePos = 0;

    /// <inheritdoc/>
    public void SetSamplePos(long value) => this.currentSamplePos = value;

    /// <inheritdoc/>
    public long ToPositionSamples(float posSeconds, float totalSeconds, long totalSampleFrames) =>
        posSeconds.MapValue(0f, totalSeconds, 0L, totalSampleFrames);

    /// <inheritdoc/>
    public float ToPositionSeconds(long totalSampleFrames, float totalSeconds) =>
        this.currentSamplePos.MapValue(0L, totalSampleFrames, 0f, totalSeconds);

    /// <inheritdoc/>
    public void ManageBuffers<T>(BufferStats bufferStats, Func<T[]> readSamples)
        where T : unmanaged
    {
        // Check if any buffers have finished playing
        var totalBufferIdsProcessed = this.alInvoker.GetSource(bufferStats.SourceId, ALGetSourcei.BuffersProcessed);
        while (totalBufferIdsProcessed-- > 0)
        {
            // Unqueue the buffer
            var unqueuedBufferId = 0u;
            this.alInvoker.SourceUnqueueBuffer(bufferStats.SourceId, ref unqueuedBufferId);

            // Refill the buffer with new data and requeue it
            var sampleData = readSamples();

            if (sampleData.Length > 0)
            {
                // Refill the unqueued buffer with new data
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                // The invalid format type is checked below
                switch (bufferStats.FormatType)
                {
                    case AudioFormatType.Mp3:
                        this.alInvoker.BufferData(
                            unqueuedBufferId,
                            bufferStats.DecoderFormat,
                            sampleData,
                            bufferStats.SampleRate);
                        break;
                    case AudioFormatType.Ogg:
                        this.alInvoker.BufferData(
                            unqueuedBufferId,
                            bufferStats.DecoderFormat,
                            sampleData,
                            bufferStats.SampleRate);
                        break;
                }

                // Queue the newly filled buffer. This binds the buffer to the source.
                this.alInvoker.SourceQueueBuffer(bufferStats.SourceId, ref unqueuedBufferId);
            }

            // Advance the current position in the audio file in the units of samples
            this.currentSamplePos += bufferStats.FormatType switch
            {
                // NOTE: Divide by total number of channels because the chunk size for
                // mp3 data is in bytes, not samples
                AudioFormatType.Mp3 => ChunkSize / bufferStats.TotalChannels,

                // NOTE: No need to divide for ogg because each float array element is a sample
                AudioFormatType.Ogg => ChunkSize,
                _ => throw new InvalidEnumArgumentException(
                    $"this.{nameof(bufferStats.FormatType)}",
                    (int)bufferStats.FormatType,
                    typeof(AudioFormatType)),
            };
        }
    }

    /// <inheritdoc/>
    public void UnqueueProcessedBuffers(uint srcId)
    {
        this.alInvoker.SourceStop(srcId);

        var totalBufferIdsProcessed = this.alInvoker.GetSource(srcId, ALGetSourcei.BuffersProcessed);

        while (totalBufferIdsProcessed-- > 0)
        {
            var unqueuedBuffers = new uint[1];

            this.alInvoker.SourceUnqueueBuffers(srcId, 1, ref unqueuedBuffers);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidEnumArgumentException">
    ///     Thrown if the <see cref="AudioFormat"/> is not a valid enum value.
    /// </exception>
    public void FillBuffersFromStart<T>(BufferStats bufferStats, IEnumerable<uint> bufferIds, Action flushDecoderData, Func<T[]> readSamples)
        where T : unmanaged
    {
        this.alInvoker.SourceStop(bufferStats.SourceId);
        UnqueueProcessedBuffers(bufferStats.SourceId);

        flushDecoderData();

        foreach (var id in bufferIds)
        {
            var bufferId = id;

            var sampleData = readSamples();

            if (sampleData.Length <= 0)
            {
                continue;
            }

            switch (bufferStats.FormatType)
            {
                case AudioFormatType.Mp3:
                    this.alInvoker.BufferData(
                        bufferId,
                        bufferStats.DecoderFormat,
                        sampleData,
                        bufferStats.SampleRate);
                    break;
                case AudioFormatType.Ogg:
                    this.alInvoker.BufferData(
                        bufferId,
                        bufferStats.DecoderFormat,
                        sampleData,
                        bufferStats.SampleRate);
                    break;
                default:
                    throw new InvalidEnumArgumentException(
                        $"this.{nameof(bufferStats.FormatType)}",
                        (int)bufferStats.FormatType,
                        typeof(AudioFormatType));
            }

            this.alInvoker.SourceQueueBuffer(bufferStats.SourceId, ref bufferId);
        }

        ResetSamplePos();
    }
}
