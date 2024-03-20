// <copyright file="IoC.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Carbonate.OneWay;
using Data;
using Devices;
using DotnetWrappers;
using Factories;
using NativeInterop;
using NativeInterop.Factories;
using OpenAL;
using ReactableData;
using SimpleInjector;

/// <summary>
/// Provides dependency injection for the application.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class IoC
{
    private static readonly FileSystem FileSystem = new ();
    private static readonly Container IoCContainer = new ();
    private static bool isInitialized;

    /// <summary>
    /// Gets the inversion of control container used to get instances of objects.
    /// </summary>
    public static Container Container
    {
        get
        {
            if (!isInitialized)
            {
                SetupContainer();
            }

            return IoCContainer;
        }
    }

    /// <summary>
    /// Sets up the IoC container.
    /// </summary>
    private static void SetupContainer()
    {
        IoCContainer.Register(() => FileSystem.File, Lifestyle.Singleton);
        IoCContainer.Register(() => FileSystem.Directory, Lifestyle.Singleton);
        IoCContainer.Register(() => FileSystem.Path, Lifestyle.Singleton);

        IoCContainer.Register<ITaskService, TaskService>(true);
        IoCContainer.Register<IThreadService, ThreadService>(Lifestyle.Singleton);
        IoCContainer.Register<IApplication, Application>(Lifestyle.Singleton);
        IoCContainer.Register<IPlatform, Platform>(Lifestyle.Singleton);
        IoCContainer.Register<ILibrary, OpenALLibrary>(Lifestyle.Singleton);
        IoCContainer.Register<IDelegateFactory, DelegateFactory>(Lifestyle.Singleton);
        IoCContainer.Register<IDependencyManager, OpenALDependencyManager>(Lifestyle.Singleton);
        IoCContainer.Register<ILibraryLoader, NativeLibraryLoader>(Lifestyle.Singleton);
        IoCContainer.Register<IFilePathResolver, NativeLibPathResolver>(Lifestyle.Singleton);
        IoCContainer.Register<IAudioDecoderFactory, AudioDecoderFactory>(Lifestyle.Singleton);

        SetupAudio();
        SetupReactables();

        isInitialized = true;
    }

    /// <summary>
    /// Setup container registration related to audio.
    /// </summary>
    private static void SetupAudio()
    {
        IoCContainer.Register<ALC>(Lifestyle.Singleton);
        IoCContainer.Register<AL>(Lifestyle.Singleton);
        IoCContainer.Register<IOpenALInvoker, OpenALInvoker>(Lifestyle.Singleton);
        IoCContainer.Register<IAudioDeviceManager, AudioDeviceManager>(Lifestyle.Singleton);
        IoCContainer.Register<IStreamBufferManager, StreamBufferManager>(Lifestyle.Singleton);
    }

    /// <summary>
    /// Setup container registration related to reactables.
    /// </summary>
    private static void SetupReactables()
    {
        IoCContainer.Register<IReactableFactory, ReactableFactory>(Lifestyle.Singleton);
        IoCContainer.Register<IPushReactable<AudioCommandData>, PushReactable<AudioCommandData>>(Lifestyle.Singleton);
        IoCContainer.Register<IPushReactable<PosCommandData>, PushReactable<PosCommandData>>(Lifestyle.Singleton);
        IoCContainer.Register<IPullReactable<bool>, PullReactable<bool>>(Lifestyle.Singleton);
    }
}
