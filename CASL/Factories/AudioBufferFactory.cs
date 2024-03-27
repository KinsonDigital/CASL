// <copyright file="AudioBufferFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Factories;

using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Data;
using Data.Decoders;
using Devices;
using DotnetWrappers;
using OpenAL;

/// <inheritdoc/>
[ExcludeFromCodeCoverage(Justification = "Directly interacts with the IoC container.")]
internal sealed class AudioBufferFactory : IAudioBufferFactory
{
    /// <inheritdoc/>
    public IAudioBuffer CreateFullBuffer(string filePath)
    {
        var alInvoker = IoC.Container.GetInstance<IOpenALInvoker>();
        var audioManager = IoC.Container.GetInstance<IAudioDeviceManager>();
        var dataStreamFactory = IoC.Container.GetInstance<IAudioDecoderFactory>();
        var reactableFactory = IoC.Container.GetInstance<IReactableFactory>();
        var path = IoC.Container.GetInstance<IPath>();
        var file = IoC.Container.GetInstance<IFile>();
        var audioStream = new AudioDecoder(filePath, dataStreamFactory, path, file);

        return new FullBuffer(
            alInvoker,
            audioManager,
            audioStream,
            reactableFactory,
            path,
            file);
    }

    /// <inheritdoc/>
    public IAudioBuffer CreateStreamBuffer(string filePath)
    {
        var alInvoker = IoC.Container.GetInstance<IOpenALInvoker>();
        var audioManager = IoC.Container.GetInstance<IAudioDeviceManager>();
        var reactableFactory = IoC.Container.GetInstance<IReactableFactory>();
        var dataStreamFactory = IoC.Container.GetInstance<IAudioDecoderFactory>();
        var bufferManager = IoC.Container.GetInstance<IStreamBufferManager>();
        var taskService = IoC.Container.GetInstance<ITaskService>();
        var threadService = IoC.Container.GetInstance<IThreadService>();
        var path = IoC.Container.GetInstance<IPath>();
        var file = IoC.Container.GetInstance<IFile>();
        var audioStream = new AudioDecoder(filePath, dataStreamFactory, path, file);

        return new StreamBuffer(
            alInvoker,
            audioManager,
            audioStream,
            bufferManager,
            reactableFactory,
            taskService,
            threadService,
            path,
            file);
    }
}
