// <copyright file="IoC.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL;

using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using CASL.Data;
using CASL.Devices;
using CASL.NativeInterop;
using CASL.NativeInterop.Factories;
using CASL.OpenAL;
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

        // Register the proper data stream to be the implementation if the consumer is a certain decoder
        IoCContainer.RegisterConditional<IAudioDataStream<float>, OggAudioDataStream>(
            context =>
            {
                return !context.HasConsumer || context.Consumer.ImplementationType == typeof(OggSoundDecoder);
            }, true);

        IoCContainer.Register<ISoundDecoder<float>, OggSoundDecoder>(true);

        IoCContainer.RegisterConditional<IAudioDataStream<byte>, Mp3AudioDataStream>(
            context =>
            {
                return !context.HasConsumer || context.Consumer.ImplementationType == typeof(MP3SoundDecoder);
            }, true);

        IoCContainer.Register<ISoundDecoder<byte>, MP3SoundDecoder>(true);
    }
}
