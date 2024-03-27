// <copyright file="Assembly.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.DotnetWrappers;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Loader;

/// <inheritdoc cref="IAssembly"/>
[ExcludeFromCodeCoverage(Justification = $"Contains direct interaction with the '{nameof(AssemblyLoadContext)}")]
internal sealed class Assembly : IAssembly, IDisposable
{
    private readonly AssemblyLoadContext? loadContext;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Assembly"/> class.
    /// </summary>
    public Assembly()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();

        ArgumentNullException.ThrowIfNull(assembly);
        this.loadContext = AssemblyLoadContext.GetLoadContext(assembly);

        ArgumentNullException.ThrowIfNull(this.loadContext);
        this.loadContext.Unloading += LoadContextOnUnloading;
    }

    /// <inheritdoc/>
    public event Action? Unloading;

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose() => Dispose(true);

    /// <summary>
    /// Invokes the <see cref="Unloading"/> event.
    /// </summary>
    /// <param name="ctx">The load context.</param>
    private void LoadContextOnUnloading(AssemblyLoadContext ctx) => this.Unloading?.Invoke();

    /// <summary>
    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// </summary>
    /// <param name="isDisposing">True to dispose of managed resources.</param>
    private void Dispose(bool isDisposing)
    {
        if (this.isDisposed)
        {
            return;
        }

        if (isDisposing && this.loadContext is not null)
        {
            this.loadContext.Unloading -= LoadContextOnUnloading;
        }

        this.isDisposed = true;
    }
}
