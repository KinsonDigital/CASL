// <copyright file="ALDevice.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.OpenAL
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Opaque handle to an OpenAL device.
    /// </summary>
    public struct ALDevice : IEquatable<ALDevice>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ALDevice"/> struct.
        /// </summary>
        /// <param name="handle">The handle of the device.</param>
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
        /// Returns a value indicating whether the <paramref name="left"/> and
        /// <paramref name="right"/> operators are equal.
        /// </summary>
        /// <param name="left">The left operator in the comparison.</param>
        /// <param name="right">The right operator in the comparison.</param>
        /// <returns>True if both operators are equal to eachother.</returns>
        public static bool operator ==(ALDevice left, ALDevice right) => left.Equals(right);

        /// <summary>
        /// Returns a value indicating whether the <paramref name="left"/> and
        /// <paramref name="right"/> operators are not equal.
        /// </summary>
        /// <param name="left">The left operator in the comparison.</param>
        /// <param name="right">The right operator in the comparison.</param>
        /// <returns>True if both operators are not equal to eachother.</returns>
        public static bool operator !=(ALDevice left, ALDevice right) => !(left == right);

        /// <summary>
        /// Returns a null <see cref="ALDevice"/>.
        /// </summary>
        /// <returns>A device that points to nothing.</returns>
        public static ALDevice Null() => new (0);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is ALDevice device && Equals(device);

        /// <inheritdoc/>
        public bool Equals(ALDevice other) => Handle.Equals(other.Handle);

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override int GetHashCode() => HashCode.Combine(Handle);
    }
}
