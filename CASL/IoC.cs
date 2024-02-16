// <copyright file="IoC.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Devices;
using Factories;
using NativeInterop;
using NativeInterop.Factories;
using OpenAL;
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

        IoCContainer.Register<IApplication, Application>(Lifestyle.Singleton);
        IoCContainer.Register<IPlatform, Platform>(Lifestyle.Singleton);
        IoCContainer.Register<ILibrary, OpenALLibrary>(Lifestyle.Singleton);
        IoCContainer.Register<IDelegateFactory, DelegateFactory>(Lifestyle.Singleton);
        IoCContainer.Register<IDependencyManager, OpenALDependencyManager>(Lifestyle.Singleton);
        IoCContainer.Register<ILibraryLoader, NativeLibraryLoader>(Lifestyle.Singleton);
        IoCContainer.Register<IFilePathResolver, NativeLibPathResolver>(Lifestyle.Singleton);
        IoCContainer.Register<IAudioDataStreamFactory, AudioDataStreamFactory>(Lifestyle.Singleton);

        SetupAudio();

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
    }
}
