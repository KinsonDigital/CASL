// <copyright file="Application.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

/// <summary>
/// Provides information about the running application.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Directly interacts with dotnet.")]
internal sealed class Application : IApplication
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Application"/> class.
    /// </summary>
    public Application() => this.Location = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

    /// <inheritdoc/>
    public string Location { get; }
}
