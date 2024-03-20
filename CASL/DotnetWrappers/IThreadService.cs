// <copyright file="IThreadService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.DotnetWrappers;

using System.Threading;

/// <inheritdoc cref="Thread"/>
public interface IThreadService
{
    /// <inheritdoc cref="Thread.Sleep(int)"/>
    void Sleep(int millisecondsTimeout);
}
