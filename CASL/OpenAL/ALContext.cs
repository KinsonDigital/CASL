// <copyright file="ALContext.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.OpenAL;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Encapsulates the state of a given instance of the state machine.
/// </summary>
public struct ALContext : IEquatable<ALContext>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ALContext"/> struct.
    /// </summary>
    /// <param name="handle">The handle of the context.</param>
    public ALContext(nint handle) => Handle = handle;

    /// <summary>
    /// Gets the handle of the context.
    /// </summary>
    public nint Handle { get; }

    /// <summary>
    /// Implicitly casts the given <see cref="ALContext"/> to an <see cref="nint"/>.
    /// </summary>
    /// <param name="context">The context to convert.</param>
    public static implicit operator nint(ALContext context) => context.Handle;

    /// <summary>
    /// Returns a value indicating whether the <paramref name="left"/> and
    /// <paramref name="right"/> operators are equal.
    /// </summary>
    /// <param name="left">The left operator in the comparison.</param>
    /// <param name="right">The right operator in the comparison.</param>
    /// <returns>True if both operators are equal to eachother.</returns>
    public static bool operator ==(ALContext left, ALContext right) => left.Equals(right);

    /// <summary>
    /// Returns a value indicating whether the <paramref name="left"/> and
    /// <paramref name="right"/> operators are not equal.
    /// </summary>
    /// <param name="left">The left operator in the comparison.</param>
    /// <param name="right">The right operator in the comparison.</param>
    /// <returns>True if both operators are not equal to eachother.</returns>
    public static bool operator !=(ALContext left, ALContext right) => !(left == right);

    /// <summary>
    /// Returns a null <see cref="ALContext"/>.
    /// </summary>
    /// <returns>A context that points to nothing.</returns>
    public static ALContext Null() => new (0);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ALContext handle && Equals(handle);

    /// <inheritdoc/>
    public bool Equals(ALContext other) => Handle.Equals(other.Handle);

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override int GetHashCode() => HashCode.Combine(Handle);
}
