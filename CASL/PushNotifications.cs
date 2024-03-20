// <copyright file="PushNotifications.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Contains ids that represent push notifications.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "No logic to test.")]
internal static class PushNotifications
{
    /// <summary>
    /// Gets an ID that represents a push notification to send an audio command to update the sound state.
    /// </summary>
    public static Guid SendAudioCmd => new ("3ad715a2-5436-4a15-8838-210d25fdfaa7");

    /// <summary>
    /// Gets an ID that represents a push notification to update the sound position.
    /// </summary>
    public static Guid UpdateSoundPos => new ("077d224e-46c1-4c34-bc9b-e71e2e4fdd42");
}
