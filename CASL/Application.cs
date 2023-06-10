// <copyright file="Application.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

/// <summary>
/// Provides information about the running application.
/// </summary>
[ExcludeFromCodeCoverage]
public class Application : IApplication
{
    /// <inheritdoc/>
    public string Location => Assembly.GetExecutingAssembly().Location;
}
