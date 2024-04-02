// <copyright file="ReactableFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Factories;

using System.Diagnostics.CodeAnalysis;
using Carbonate.OneWay;
using ReactableData;

/// <inheritdoc/>
[ExcludeFromCodeCoverage(Justification = "Directly interacts with the IoC container.")]
internal sealed class ReactableFactory : IReactableFactory
{
    /// <inheritdoc/>
    public IPushReactable<AudioCommandData> CreateAudioCmndReactable() => IoC.Container.GetInstance<IPushReactable<AudioCommandData>>();

    /// <inheritdoc/>
    public IPushReactable<PosCommandData> CreatePositionCmndReactable() => IoC.Container.GetInstance<IPushReactable<PosCommandData>>();

    /// <inheritdoc/>
    public IPullReactable<bool> CreateIsLoopingReactable() => IoC.Container.GetInstance<IPullReactable<bool>>();
}
