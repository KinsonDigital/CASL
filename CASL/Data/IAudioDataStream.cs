﻿// <copyright file="IAudioDataStream.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Data;

using System;

/// <summary>
/// Streams mp3 audio data from a mp3 file.
/// </summary>
/// <typeparam name="T">The type of data from the audio stream.</typeparam>
internal interface IAudioDataStream<T> : IDisposable
{
    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    string Filename { get; set; }

    /// <summary>
    /// Gets the number of audio channels.
    /// </summary>
    int Channels { get; }

    /// <summary>
    /// Gets the audio format.
    /// </summary>
    AudioFormat Format { get; }

    /// <summary>
    /// Gets the audio sample rate.
    /// </summary>
    int SampleRate { get; }

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
