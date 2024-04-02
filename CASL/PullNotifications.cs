// <copyright file="PullNotifications.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Contains ids that represent pull notifications.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "No logic to test.")]
internal static class PullNotifications
{
    /// <summary>
    /// Gets an ID that represents a pull notification to get the loop state.
    /// </summary>
    public static Guid GetLoopState => new ("1ab916b6-97ab-486f-a940-cec9f3c655b7");
}
