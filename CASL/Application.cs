// <copyright file="Application.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Provides information about the running application.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Directly interacts with dotnet.")]
internal sealed class Application : IApplication
{
    /// <inheritdoc/>
    public string Location => AppContext.BaseDirectory;
}
