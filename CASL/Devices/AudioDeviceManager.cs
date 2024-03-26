// <copyright file="AudioDeviceManager.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.Devices;

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Exceptions;
using CASL.Exceptions;
using OpenAL;

/// <summary>
/// Manages audio devices on the system using OpenAL.
/// </summary>
internal sealed class AudioDeviceManager : IAudioDeviceManager
{
    private const string DeviceNamePrefix = "OpenAL Soft on "; // All device names returned are prefixed with this
    private readonly IOpenALInvoker alInvoker;
    private readonly string isDisposedExceptionMessage = $"The '{nameof(AudioDeviceManager)}' has not been initialized.\nInvoked the '{nameof(InitDevice)}()' to initialize the device manager.";
    private nint device;
    private ALContext context;
    private ALContextAttributes? attributes;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioDeviceManager"/> class.
    /// </summary>
    /// <param name="alInvoker">Provides access to OpenAL.</param>
    public AudioDeviceManager(IOpenALInvoker alInvoker)
    {
        ArgumentNullException.ThrowIfNull(alInvoker);

        this.alInvoker = alInvoker;
        this.alInvoker.ErrorCallback += ErrorCallback;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="AudioDeviceManager"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Finalizers cannot be tested")]
    ~AudioDeviceManager() => Dispose(false);

    /// <inheritdoc/>
    public event EventHandler<EventArgs>? DeviceChanging;

    /// <inheritdoc/>
    public event EventHandler<EventArgs>? DeviceChanged;

    /// <inheritdoc/>
    public bool IsInitialized => !AudioIsNull();

    /// <inheritdoc/>
    public string DeviceInUse { get; private set; } = string.Empty;

    /// <inheritdoc/>
    /// <returns>The list of device names.</returns>
    /// <exception cref="AudioDeviceManagerNotInitializedException">
    ///     Occurs if this method is executed without initializing the <see cref="IAudioDeviceManager.InitDevice"/>() method.
    ///     This can be done by invoking the <see cref="InitDevice(string?)"/>.
    /// </exception>
    public ImmutableArray<string> GetDeviceNames()
    {
        if (!IsInitialized)
        {
            throw new AudioDeviceManagerNotInitializedException(this.isDisposedExceptionMessage);
        }

        var result = this.alInvoker.GetDeviceList()
            .Select(n => n.Replace(DeviceNamePrefix, string.Empty, StringComparison.Ordinal)).ToArray();

        return result.ToImmutableArray();
    }

    /// <inheritdoc/>
    public void InitDevice(string? name = null)
    {
        var nameResult = name == null ? name : $"{DeviceNamePrefix}{name}";

        if (this.device == 0)
        {
            this.device = this.alInvoker.OpenDevice(nameResult);
        }

        this.attributes ??= new ALContextAttributes();

        if (this.context.Handle == 0)
        {
            this.context = new ALContext(this.alInvoker.CreateContext(new ALDevice(this.device), this.attributes));
        }

        var setCurrentResult = this.alInvoker.MakeContextCurrent(this.context);

        DeviceInUse = this.alInvoker.GetDefaultDevice();

        if (!setCurrentResult)
        {
            throw new InitializeDeviceException();
        }
    }

    /// <inheritdoc/>
    public void ChangeDevice(string name)
    {
        if (name == DeviceInUse)
        {
            return;
        }

        if (!IsInitialized)
        {
            throw new AudioDeviceManagerNotInitializedException(this.isDisposedExceptionMessage);
        }

        var deviceNames = GetDeviceNames();

        if (!deviceNames.Contains(name))
        {
            throw new AudioDeviceDoesNotExistException("The audio device does not exist.", name);
        }

        this.DeviceChanging?.Invoke(this, EventArgs.Empty);

        DestroyDevice();
        InitDevice(name);

        this.DeviceChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Invoked when there is an OpenAL specific error.
    /// </summary>
    /// <param name="errorMsg">The error message from OpenAL.</param>
    [ExcludeFromCodeCoverage]
    private static void ErrorCallback(string errorMsg) => throw new AudioException(errorMsg);

    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// <param name="disposing"><see langword="true"/> to dispose of managed resources.</param>
    private void Dispose(bool disposing)
    {
        if (this.isDisposed)
        {
            return;
        }

        DestroyDevice();

        if (disposing)
        {
            this.DeviceChanging = null;
            this.DeviceChanged = null;
            this.alInvoker.ErrorCallback -= ErrorCallback;
        }

        this.isDisposed = true;
    }

    /// <summary>
    /// Destroys the current audio device.
    /// </summary>
    /// <remarks>
    ///     To use another audio device, the <see cref="InitDevice(string?)"/>
    ///     will have to be invoked again.
    /// </remarks>
    private void DestroyDevice()
    {
        this.alInvoker.MakeContextCurrent(ALContext.Null());
        this.alInvoker.DestroyContext(this.context);
        this.context = ALContext.Null();

        this.alInvoker.CloseDevice(new ALDevice(this.device));
        this.device = ALDevice.Null();

        this.attributes = null;
    }

    /// <summary>
    /// Returns a value indicating if the audio device and context are null.
    /// </summary>
    /// <returns><see langword="true"/> if the device and context are null.</returns>
    private bool AudioIsNull() => this.device == ALDevice.Null() && this.context == ALContext.Null() && this.attributes is null;
}
