// <copyright file="IVorbisReaderWrapper.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Wrappers;

using System;
using NVorbis;

/// <summary>
/// A thin wrapper around the vorbis reader for the purpose of abstraction and testing.
/// </summary>
internal interface IVorbisReaderWrapper : IDisposable
{
    /// <inheritdoc cref="VorbisReader.Channels"/>
    int Channels { get; }

    /// <inheritdoc cref="VorbisReader.SampleRate"/>
    int SampleRate { get; }

    /// <inheritdoc cref="VorbisReader.TotalSamples"/>
    long TotalSamples { get; }

    /// <summary>
    /// Gets the total duration of the decoded stream in seconds.
    /// </summary>
    float TotalSeconds { get; }

    /// <summary>
    /// Loads a vorbis file at the given <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    void Load(string filePath);

    /// <inheritdoc cref="VorbisReader.ReadSamples(float[],int,int)"/>
    int ReadSamples(float[] buffer, int offset, int count);
}
