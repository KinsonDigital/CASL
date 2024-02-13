// <copyright file="OpenALInvoker.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.OpenAL;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Extensions.Enumeration;
using Silk.NET.OpenAL.Extensions.EXT;

// TODO: Remove these after the CASL low level types have been removed
using SilkAL = Silk.NET.OpenAL.AL;
using SilkALC = Silk.NET.OpenAL.ALContext;

// TODO: Need to go into each method and make sure to call the GetError first, then call the AL function, then call error again
// This is to make sure that any previous errors are cleared first before calling the AL function.  Create a private ClearError()
// method to do this to make it clear.  This will of course just call geterror.
// Also, we need to implement the ability to only check for errors in debug mode for performance reasons when using release mode

/// <summary>
/// Invokes OpenAL functions.
/// </summary>
[ExcludeFromCodeCoverage]
internal class OpenALInvoker : IOpenALInvoker
{
    private readonly SilkAL al;
    private readonly SilkALC alc;

    // private readonly ALC alc;
    // private readonly AL al;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenALInvoker"/> class.
    /// </summary>
    public OpenALInvoker()
    {
        this.al = SilkAL.GetApi();
        this.alc = SilkALC.GetApi();
    }

    /// <inheritdoc/>
    public event Action<string>? ErrorCallback;

    /// <inheritdoc/>
    public AudioError GetError() => this.al.GetError();

    /// <inheritdoc/>
    public Context CreateContext(Device device, ALContextAttributes attributes)
    {
        unsafe
        {
            Context* contextResult;

            var attrArray = attributes.CreateAttributeArray();

            fixed (int* attrArrayPtr = attrArray)
            {
                contextResult = this.alc.CreateContext(&device, attrArrayPtr);
            }

            var error = this.alc.GetError(&device);

            var errorMessage = Enum.GetName(typeof(ContextError), error);

            InvokeErrorIfTrue(error != ContextError.NoError, errorMessage);

            return *contextResult;
        }
    }

    /// <inheritdoc/>
    public Device OpenDevice(string deviceName)
    {
        ArgumentNullException.ThrowIfNull(deviceName); // TODO: Test this

        unsafe
        {
            var deviceResult = this.alc.OpenDevice(deviceName);
            var error = this.alc.GetError(deviceResult);

            var errorMessage = Enum.GetName(typeof(ContextError), error);

            InvokeErrorIfTrue(error != ContextError.NoError, errorMessage);

            return *deviceResult;
        }
    }

    /// <inheritdoc/>
    public bool MakeContextCurrent(Context context)
    {
        var result = false;

        unsafe
        {
            var contextPtr = &context;

            if (contextPtr == null)
            {
                // TODO: Test this out by sending in a null context.  This will have to be done by creating a
                // null Context* pointer first and then dereferencing it before sending it in
                var errorMsg = $"The parameter '{nameof(context)}' points to a null device.";
                errorMsg += $"\nIf you want to destroy the context, then use the '{nameof(DestroyContext)}' method.";

                InvokeErrorIfTrue(true, errorMsg);
            }
            else
            {
                result = this.alc.MakeContextCurrent(contextPtr);

                var device = GetContextsDevice(*contextPtr);

                var error = this.alc.GetError(&device);

                if (result)
                {
                    var errorMessage = Enum.GetName(typeof(ContextError), error);
                    InvokeErrorIfTrue(error != ContextError.NoError, errorMessage);
                }
                else
                {
                    // Throw an error that the context could not be made current
                    InvokeErrorIfTrue(true, $"Context with handle '{new IntPtr(contextPtr)}' could not be made current.");
                }
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public uint GenBuffer()
    {
        unsafe
        {
            var buffer = 0u;
            this.al.GenBuffers(1, &buffer);

            var error = GetError();

            var errorMessage = Enum.GetName(typeof(AudioError), error);

            InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);

            return buffer;
        }
    }

    /// <inheritdoc/>
    public uint GenSource()
    {
        unsafe
        {
            var source = 0u;
            this.al.GenSources(1, &source);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(AudioError), error);

            InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);

            return source;
        }
    }

    /// <inheritdoc/>
    public int GetSourceProperty(uint sid, GetSourceInteger param)
    {
        this.al.GetSourceProperty(sid, param, out var result);

        var error = GetError();
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);

        return result;
    }

    /// <inheritdoc/>
    public bool GetSourceProperty(uint sid, SourceBoolean param)
    {
        this.al.GetSourceProperty(sid, param, out var result);

        var error = GetError();
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);

        return result;
    }

    /// <inheritdoc/>
    public float GetSourceProperty(uint sid, SourceFloat param)
    {
        this.al.GetSourceProperty(sid, param, out var value);

        var error = GetError();
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);

        return value;
    }

    /// <inheritdoc/>
    public int GetBuffer(uint bid, GetBufferInteger param)
    {
        this.al.GetBufferProperty(bid, param, out var value);

        var error = GetError();
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);

        return value;
    }

    /// <inheritdoc/>
    public SourceState GetSourceState(uint sid)
    {
        this.al.GetSourceProperty(sid, GetSourceInteger.SourceState, out var result);

        return (SourceState)result;
    }

    /// <inheritdoc/>
    public Device GetContextsDevice(Context context)
    {
        unsafe
        {
            var deviceResult = this.alc.GetContextsDevice(&context);

            var error = this.alc.GetError(deviceResult);
            var errorMessage = Enum.GetName(typeof(ContextError), error);

            InvokeErrorIfTrue(error != ContextError.NoError, errorMessage);

            return *deviceResult;
        }
    }

    /// <inheritdoc/>
    public string GetString(Device device, GetContextString param)
    {
        unsafe
        {
            var result = this.alc.GetContextProperty(&device, param);
            var error = this.alc.GetError(&device);

            var errorMessage = Enum.GetName(typeof(ContextError), error);

            InvokeErrorIfTrue(error != ContextError.NoError, errorMessage);

            return result;
        }
    }

    /// <inheritdoc/>
    public ImmutableArray<string> GetDeviceList()
    {
        // TODO: Manually test this out!!
        const string extensionName = "ALC_ENUMERATE_ALL_EXT";

        var deviceListResult = new List<string>();

        unsafe
        {
            var device = this.alc.OpenDevice(string.Empty);

            // Check if the device enumeration extension is available
            if (this.alc.IsExtensionPresent(device, extensionName))
            {
                // Get the enumeration extension
                var getExtensionUnsuccessful = !this.alc.TryGetExtension<Enumeration>(device, out var ext);

                if (getExtensionUnsuccessful)
                {
                    InvokeErrorIfTrue(true, $"The OpenAL context extension {extensionName} could not be retrieved.");

                    // TODO: Look into this warning
                    Array.Empty<string>();
                }

                // Get a list of all available device specifiers
                foreach (var deviceName in ext.GetStringList(GetEnumerationContextStringList.DeviceSpecifiers))
                {
                    if (!string.IsNullOrEmpty(deviceName))
                    {
                        deviceListResult.Add(deviceName);
                    }
                }
            }
        }

        return deviceListResult.ToImmutableArray();
    }

    /// <inheritdoc/>
    public string GetDefaultDevice()
    {
        // TODO: Need to manually test this out
        // The hope here is based on the OpenAL docs, we can just send in a null device and it will return the default device
        // https://www.openal.org/documentation/OpenAL_Programmers_Guide.pdf#page=132&zoom=100,0,0

        unsafe
        {
            var defaultDeviceName = this.alc.GetContextProperty(null, GetContextString.DeviceSpecifier);

            return defaultDeviceName;
        }
    }

    /// <inheritdoc/>
    public void BufferData<TBuffer>(uint buffer, BufferFormat format, TBuffer[] data, int freq)
        where TBuffer : unmanaged
    {
        // TODO: NEeds testing.  BufferFormat does not have a stereo 32bit float value.  Need to look into this

        unsafe
        {
            fixed (TBuffer* b = data)
            {
                this.al.BufferData(buffer, format, b, data.Length * sizeof(TBuffer), freq);
            }
        }

        var error = GetError();
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);
    }

    /// <inheritdoc/>
    public void BufferData<TBuffer>(uint buffer, FloatBufferFormat format, TBuffer[] data, int freq)
        where TBuffer : unmanaged
    {
        this.al.BufferData(buffer, format, data, freq);

        var error = GetError();
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);
    }

    /// <inheritdoc/>
    public void SetSourceProperty(uint source, int buffer)
    {
        this.al.SetSourceProperty(source, SourceInteger.Buffer, buffer);

        var error = GetError();
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);
    }

    /// <inheritdoc/>
    public void SetSourceProperty(uint source, SourceInteger param, int value)
    {
        this.al.SetSourceProperty(source, param, value);

        var error = GetError();
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);
    }

    /// <inheritdoc/>
    public void SetSourceProperty(uint source, SourceBoolean param, bool value)
    {
        this.al.SetSourceProperty(source, param, value);

        var error = GetError();
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);
    }

    /// <inheritdoc/>
    public void SetSourceProperty(uint source, SourceFloat param, float value)
    {
        this.al.SetSourceProperty(source, param, value);

        var error = GetError();
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);
    }

    /// <inheritdoc/>
    public void SourcePlay(uint source)
    {
        this.al.SourcePlay(source);

        var error = GetError();
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);
    }

    /// <inheritdoc/>
    public void SourcePause(uint source)
    {
        this.al.SourcePause(source);

        var error = GetError();
        // TODO: When getting the error message, let's build a ref table of each audio error for perf reasons
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);
    }

    /// <inheritdoc/>
    public void SourceStop(uint source)
    {
        this.al.SourceStop(source);

        var error = GetError();
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);
    }

    /// <inheritdoc/>
    public void SourceRewind(uint source)
    {
        this.al.SourceRewind(source);

        var error = GetError();
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);
    }

    /// <inheritdoc/>
    public bool CloseDevice(Device device)
    {
        unsafe
        {
            var closeResult = this.alc.CloseDevice(&device);
            var error = this.alc.GetError(&device);
            var errorMessage = Enum.GetName(typeof(ContextError), error);

            InvokeErrorIfTrue(error != ContextError.NoError, errorMessage);

            return closeResult;
        }
    }

    /// <inheritdoc/>
    public void DeleteBuffer(uint buffer)
    {
        unsafe
        {
            this.al.DeleteBuffers(1, &buffer);
        }

        var error = GetError();
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);
    }

    /// <inheritdoc/>
    public void DeleteSource(uint source)
    {
        unsafe
        {
            this.al.DeleteSources(1, &source);
        }

        var error = GetError();
        var errorMessage = Enum.GetName(typeof(AudioError), error);

        InvokeErrorIfTrue(error != AudioError.NoError, errorMessage);
    }

    /// <inheritdoc/>
    public void DestroyContext(Context context)
    {
        unsafe
        {
            var device = GetContextsDevice(context);
            this.alc.DestroyContext(&context);

            var error = this.alc.GetError(&device);
            var errorMessage = Enum.GetName(typeof(ContextError), error);

            InvokeErrorIfTrue(error != ContextError.NoError, errorMessage);
        }
    }

    /// <summary>
    /// Invokes the error callback if the <paramref name="shouldInvoke"/> is true.
    /// </summary>
    /// <param name="shouldInvoke">If true, invokes the error callback.</param>
    /// <param name="errorMessage">The error message.</param>
    private void InvokeErrorIfTrue(bool shouldInvoke, string? errorMessage)
    {
        if (shouldInvoke)
        {
            this.ErrorCallback?.Invoke(string.IsNullOrEmpty(errorMessage) ? "OpenAL" : errorMessage);
        }
    }
}
