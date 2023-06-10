// <copyright file="IApplication.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

/// <summary>
/// Provides information about the running application.
/// </summary>
internal interface IApplication
{
    /// <summary>
    /// Gets the file path of the current application.
    /// </summary>
    string Location { get; }
}
