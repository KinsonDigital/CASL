// <copyright file="ALDevice.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.OpenAL;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Opaque handle to an OpenAL device.
/// </summary>
internal readonly record struct ALDevice
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ALDevice"/> struct.
    /// </summary>
    /// <param name="handle">The handle of the device.</param>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1642:Constructor summary documentation should begin with standard text",
        Justification = "Incorrectly flagging this as a violation")]
    public ALDevice(nint handle) => Handle = handle;

    /// <summary>
    /// Gets the handle of the context.
    /// </summary>
    public nint Handle { get; }

    /// <summary>
    /// Implicitly casts the given <see cref="ALDevice"/> to an <see cref="nint"/>.
    /// </summary>
    /// <param name="device">The audio device to convert.</param>
    public static implicit operator nint(ALDevice device) => device.Handle;

    /// <summary>
    /// Returns a null <see cref="ALDevice"/>.
    /// </summary>
    /// <returns>A device that points to nothing.</returns>
    public static ALDevice Null() => new (0);
}
