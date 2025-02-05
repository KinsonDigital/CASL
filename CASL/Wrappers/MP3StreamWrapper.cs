// <copyright file="MP3StreamWrapper.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Wrappers;

using System;
using System.Diagnostics.CodeAnalysis;
using MP3Sharp;

/// <inheritdoc/>
[ExcludeFromCodeCoverage(Justification = "Directly interacts with audio files.")]
internal class MP3StreamWrapper : IMP3StreamWrapper
{
    private MP3Stream? mp3Stream;

    /// <inheritdoc/>
    public int ChannelCount => this.mp3Stream?.ChannelCount ?? 0;

    /// <inheritdoc/>
    public SoundFormat Format => this.mp3Stream?.Format ?? SoundFormat.Pcm16BitMono;

    /// <inheritdoc/>
    public int Frequency => this.mp3Stream?.Frequency ?? 0;

    /// <inheritdoc/>
    public void Load(string filePath) => this.mp3Stream = new MP3Stream(filePath);

    /// <inheritdoc/>
    public int Read(byte[] buffer, int offset, int count) => this.mp3Stream?.Read(buffer, offset, count) ?? 0;

    /// <inheritdoc/>
    public void Flush() => this.mp3Stream?.Flush();

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        this.mp3Stream?.Dispose();
        GC.SuppressFinalize(this);
    }
}
