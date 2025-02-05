// <copyright file="IMP3StreamWrapper.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Wrappers;

using System;
using MP3Sharp;

/// <summary>
/// A thin wrapper around the mp3 stream API for the purpose of abstraction and testing.
/// </summary>
internal interface IMP3StreamWrapper : IDisposable
{
    /// <inheritdoc cref="MP3Stream.ChannelCount"/>
    int ChannelCount { get; }

    /// <inheritdoc cref="MP3Stream.Format"/>
    SoundFormat Format { get; }

    /// <inheritdoc cref="MP3Stream.Frequency"/>
    int Frequency { get; }

    /// <summary>
    /// Loads a mp3 file at the given <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    void Load(string filePath);

    /// <inheritdoc cref="MP3Stream.Read(byte[],int,int)"/>
    int Read(byte[] buffer, int offset, int count);

    /// <summary>
    /// <inheritdoc cref="MP3Stream.Flush"/>
    /// </summary>
    void Flush();
}
