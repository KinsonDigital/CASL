// <copyright file="ALContext.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.OpenAL;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Encapsulates the state of a given instance of the state machine.
/// </summary>
internal readonly record struct ALContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ALContext"/> struct.
    /// </summary>
    /// <param name="handle">The handle of the context.</param>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1642:Constructor summary documentation should begin with standard text",
        Justification = "Incorrectly flagging this as a violation")]
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
    /// Returns a null <see cref="ALContext"/>.
    /// </summary>
    /// <returns>A context that points to nothing.</returns>
    public static ALContext Null() => new (0);
}
