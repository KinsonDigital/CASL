// <copyright file="IReactableFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Factories;

using Carbonate.OneWay;
using ReactableData;

/// <summary>
/// Creates various reactables to simplify constructor injection.
/// </summary>
internal interface IReactableFactory
{
    /// <summary>
    /// Creates a reactable for changing the state of the audio such as play, pause, and stop.
    /// </summary>
    /// <returns>The reactable.</returns>
    IPushReactable<AudioCommandData> CreateAudioCmndReactable();

    /// <summary>
    /// Creates a reactable for setting the position within the audio.
    /// </summary>
    /// <returns>The reactable.</returns>
    IPushReactable<PosCommandData> CreatePositionCmndReactable();

    /// <summary>
    /// Creates a reactable for setting the audio loop state.
    /// </summary>
    /// <returns>The reactable.</returns>
    IPullReactable<bool> CreateIsLoopingReactable();
}
