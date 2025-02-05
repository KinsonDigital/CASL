// <copyright file="VorbisReaderWrapper.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Wrappers;

using System;
using System.Diagnostics.CodeAnalysis;
using NVorbis;

/// <inheritdoc/>
[ExcludeFromCodeCoverage(Justification = "Directly interacts with audio files.")]
internal class VorbisReaderWrapper : IVorbisReaderWrapper
{
    private VorbisReader? vorbisReader;

    /// <inheritdoc/>
    public int Channels => this.vorbisReader?.Channels ?? 0;

    /// <inheritdoc/>
    public int SampleRate => this.vorbisReader?.SampleRate ?? 0;

    /// <inheritdoc/>
    public long TotalSamples => this.vorbisReader?.TotalSamples ?? 0;

    /// <inheritdoc/>
    public float TotalSeconds =>
        this.vorbisReader is null
            ? 0f
            : (float)this.vorbisReader.TotalTime.TotalSeconds;

    /// <inheritdoc/>
    public void Load(string filePath) => this.vorbisReader = new VorbisReader(filePath);

    /// <inheritdoc/>
    public int ReadSamples(float[] buffer, int offset, int count) => this.vorbisReader?.ReadSamples(buffer, offset, count) ?? 0;

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        this.vorbisReader?.Dispose();
        GC.SuppressFinalize(this);
    }
}
