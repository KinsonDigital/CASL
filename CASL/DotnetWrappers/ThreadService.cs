// <copyright file="ThreadService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.DotnetWrappers;

using System.Diagnostics.CodeAnalysis;
using System.Threading;

/// <inheritdoc/>
[ExcludeFromCodeCoverage(Justification = "Directly interacts with dotnet.")]
public class ThreadService : IThreadService
{
    /// <inheritdoc/>
    public void Sleep(int millisecondsTimeout) => Thread.Sleep(millisecondsTimeout);
}
