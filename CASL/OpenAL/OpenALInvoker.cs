// <copyright file="OpenALInvoker.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.OpenAL;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Invokes OpenAL functions.
/// </summary>
[ExcludeFromCodeCoverage]
internal class OpenALInvoker : IOpenALInvoker
{
    private readonly ALC alc;
    private readonly AL al;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenALInvoker"/> class.
    /// </summary>
    public OpenALInvoker()
    {
        this.alc = ALC.GetApi();
        this.al = AL.GetApi();
    }

    /// <inheritdoc/>
    public event Action<string>? ErrorCallback;

    /// <inheritdoc/>
    public ALError GetError() => this.al.GetError();

    /// <inheritdoc/>
    public ALContext CreateContext(ALDevice device, ALContextAttributes attributes)
    {
        ClearAlcError(device);
        var contextResult = this.alc.CreateContext(device, attributes.CreateAttributeArray());
        ProcessAlcError(device);

        return contextResult;
    }

    /// <inheritdoc/>
    public ALDevice OpenDevice(string? deviceName)
    {
        ClearAlcError(ALDevice.Null());
        var device = this.alc.OpenDevice(deviceName);
        ProcessAlcError(device);

        return device;
    }

    /// <inheritdoc/>
    public bool MakeContextCurrent(ALContext context)
    {
        bool result;

        // If the context is null, then the attempt is to destroy the context
        if (context == ALContext.Null())
        {
            result = this.alc.MakeContextCurrent(context);

            if (!result)
            {
                this.ErrorCallback?.Invoke("Issue destroying the context.");
            }
        }
        else
        {
            ClearAlcError(ALDevice.Null());
            result = this.alc.MakeContextCurrent(context);
            ProcessAlcError(ALDevice.Null());

            if (!result)
            {
                // Throw an error that the context could not be made current
                this.ErrorCallback?.Invoke($"Context with handle '{context.Handle}' could not be made current.");
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public uint GenBuffer()
    {
        ClearAlError();
        var buffer = 0u;
        this.al.GenBuffers(1, ref buffer);
        ProcessAlError();

        return buffer;
    }

    /// <inheritdoc/>
    public uint[] GenBuffers(int count)
    {
        var buffers = new uint[count];

        ClearAlError();
        this.al.GenBuffers(count, ref buffers[0]);
        ProcessAlError();

        return buffers;
    }

    /// <inheritdoc/>
    public uint GenSource()
    {
        ClearAlError();
        var source = 0u;
        this.al.GenSources(1, ref source);
        ProcessAlError();

        return source;
    }

    /// <inheritdoc/>
    public string GetErrorString(ALError param)
    {
        ClearAlError();
        var result = this.al.Get((ALGetString)param);
        ProcessAlError();

        return result;
    }

    /// <inheritdoc/>
    public int GetSource(uint source, ALGetSourcei param)
    {
        ClearAlError();
        this.al.GetSource(source, param, out var result);
        ProcessAlError();

        return result;
    }

    /// <inheritdoc/>
    public bool GetSource(uint source, ALSourceb param)
    {
        ClearAlError();
        this.al.GetSource(source, (ALGetSourcei)param, out var result);
        ProcessAlError();

        return result != 0;
    }

    /// <inheritdoc/>
    public float GetSource(uint source, ALSourcef param)
    {
        ClearAlError();
        this.al.GetSource(source, param, out var value);
        ProcessAlError();

        return value;
    }

    /// <inheritdoc/>
    public int GetBuffer(uint bid, ALGetBufferi param)
    {
        ClearAlError();
        this.al.GetBuffer(bid, param, out var value);
        ProcessAlError();

        return value;
    }

    /// <inheritdoc/>
    public ALSourceState GetSourceState(uint source)
    {
        ClearAlError();
        this.al.GetSource(source, ALGetSourcei.SourceState, out var result);
        ProcessAlError();

        return (ALSourceState)result;
    }

    /// <inheritdoc/>
    public ALDevice GetContextsDevice(ALContext context)
    {
        ClearAlcError(ALDevice.Null());
        var device = this.alc.GetContextsDevice(context);
        ProcessAlcError(device);

        return device;
    }

    /// <inheritdoc/>
    public string GetString(ALDevice device, AlcGetString param)
    {
        ClearAlcError(device);
        var result = this.alc.GetString(device, param);
        ProcessAlcError(device);

        return result;
    }

    /// <inheritdoc/>
    public IList<string> GetDeviceList()
    {
        unsafe
        {
            var nullDevice = new ALDevice(0);
            ClearAlcError(nullDevice);

            var stringsStart = this.alc.GetStringPtr(nullDevice, (AlcGetString)AlcGetStringList.AllDevicesSpecifier);
            ProcessAlcError(nullDevice);

            return ((nint)stringsStart).ToStrings();
        }
    }

    /// <inheritdoc/>
    public string GetDefaultDevice() => GetString(new ALDevice(0), AlcGetString.DefaultDeviceSpecifier);

    /// <inheritdoc/>
    public void BufferData<TBuffer>(uint bid, ALFormat format, TBuffer[] buffer, int freq)
        where TBuffer : unmanaged
    {
        unsafe
        {
            ClearAlError();

            fixed (TBuffer* b = buffer)
            {
                this.al.BufferData(bid, format, b, buffer.Length * sizeof(TBuffer), freq);
            }

            ProcessAlError();
        }
    }

    /// <inheritdoc/>
    public void BindBufferToSource(uint source, int buffer)
    {
        ClearAlError();
        this.al.Source(source, ALSourcei.Buffer, buffer);
        ProcessAlError();
    }

    /// <inheritdoc/>
    public void SourceQueueBuffers(int source, int count, ref uint[] buffers)
    {
        ClearAlError();
        ProcessAlError();
    }

    /// <inheritdoc/>
    public void SourceUnqueueBuffers(int source, int count, ref uint[] buffers)
    {
        ClearAlError();
        ProcessAlError();
    }

    /// <inheritdoc/>
    public void Source(uint source, ALSourcei param, int value)
    {
        ClearAlError();
        this.al.Source(source, param, value);
        ProcessAlError();
    }

    /// <inheritdoc/>
    public void Source(uint source, ALSourceb param, bool value)
    {
        ClearAlError();
        this.al.Source(source, param, value);
        ProcessAlError();
    }

    /// <inheritdoc/>
    public void Source(uint source, ALSourcef param, float value)
    {
        ClearAlError();
        this.al.Source(source, param, value);
        ProcessAlError();
    }

    /// <inheritdoc/>
    public void SourcePlay(uint source)
    {
        ClearAlError();
        this.al.SourcePlay(source);
        ProcessAlError();
    }

    /// <inheritdoc/>
    public void SourcePause(uint source)
    {
        ClearAlError();
        this.al.SourcePause(source);
        ProcessAlError();
    }

    /// <inheritdoc/>
    public void SourceStop(uint source)
    {
        ClearAlError();
        this.al.SourceStop(source);
        ProcessAlError();
    }

    /// <inheritdoc/>
    public void SourceRewind(uint source)
    {
        ClearAlError();
        this.al.SourceRewind(source);
        ProcessAlError();
    }

    /// <inheritdoc/>
    public bool CloseDevice(ALDevice device)
    {
        ClearAlcError(device);
        var closeResult = this.alc.CloseDevice(device);
        ProcessAlcError(device);

        return closeResult;
    }

    /// <inheritdoc/>
    public void DeleteBuffer(uint buffer)
    {
        ClearAlError();
        this.al.DeleteBuffers(1, ref buffer);
        ProcessAlError();
    }

    /// <inheritdoc/>
    public void DeleteSource(uint source)
    {
        ClearAlError();
        this.al.DeleteSources(1, ref source);
        ProcessAlError();
    }

    /// <inheritdoc/>
    public void DestroyContext(ALContext context)
    {
        var device = GetContextsDevice(context);
        ClearAlcError(device);
        this.alc.DestroyContext(context);
        ProcessAlcError(device);
    }

    /// <summary>
    /// Processes any possible OpenAL errors.
    /// </summary>
    private void ProcessAlError()
    {
#if DEBUG
        var error = this.al.GetError();

        var errorMessage = Enum.GetName(typeof(ALError), error);

        if (error != ALError.NoError)
        {
            this.ErrorCallback?.Invoke(string.IsNullOrEmpty(errorMessage) ? "OpenAL" : errorMessage);
        }
#endif
    }

    /// <summary>
    /// Processes any possible OpenAL context errors.
    /// </summary>
    /// <param name="device">The device related to the error.</param>
    private void ProcessAlcError(ALDevice device)
    {
#if DEBUG
        var error = this.alc.GetError(device);

        var errorMessage = Enum.GetName(typeof(AlcError), error);

        if (error != AlcError.NoError)
        {
            this.ErrorCallback?.Invoke(string.IsNullOrEmpty(errorMessage) ? "OpenAL" : errorMessage);
        }
#endif
    }

    /// <summary>
    /// Clears the OpenAL error.
    /// </summary>
    private void ClearAlError()
    {
#if DEBUG
        this.al.GetError();
#endif
    }

    /// <summary>
    /// Clears the OpenAL context error for the given <paramref name="device"/>.
    /// </summary>
    /// <param name="device">The device related to the error.</param>
    private void ClearAlcError(ALDevice device)
    {
#if DEBUG
        this.alc.GetError(device);
#endif
    }
}
