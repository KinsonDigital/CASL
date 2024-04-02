// <copyright file="IAssembly.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.DotnetWrappers;

using System;

/// <summary>
/// Provides assembly related operations.
/// </summary>
internal interface IAssembly
{
    /// <summary>
    /// Occurs when the assembly is unloading.
    /// </summary>
    event Action Unloading;
}
