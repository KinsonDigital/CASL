// <copyright file="OpenALInvoker.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.OpenAL
{
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
            var contextResult = this.alc.CreateContext(device, attributes.CreateAttributeArray());
            var error = this.alc.GetError(device);

            var errorMessage = Enum.GetName(typeof(AlcError), error);

            InvokeErrorIfTrue(error != AlcError.NoError, errorMessage);

            return contextResult;
        }

        /// <inheritdoc/>
        public ALDevice OpenDevice(string? deviceName)
        {
            var deviceResult = this.alc.OpenDevice(deviceName);
            var error = this.alc.GetError(deviceResult);

            var errorMessage = Enum.GetName(typeof(AlcError), error);

            InvokeErrorIfTrue(error != AlcError.NoError, errorMessage);

            return deviceResult;
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
                    InvokeErrorIfTrue(true, "Issue destroying the context.");
                }
            }
            else
            {
                result = this.alc.MakeContextCurrent(context);

                var device = GetContextsDevice(context);

                var error = this.alc.GetError(device);

                if (result)
                {
                    var errorMessage = Enum.GetName(typeof(AlcError), error);
                    InvokeErrorIfTrue(error != AlcError.NoError, errorMessage);
                }
                else
                {
                    // Throw an error that the context could not be made current
                    InvokeErrorIfTrue(true, $"Context with handle '{context.Handle}' could not be made current.");
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public uint GenBuffer()
        {
            var buffer = 0;
            this.al.GenBuffers(1, ref buffer);

            var error = GetError();

            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);

            return (uint)buffer;
        }

        /// <inheritdoc/>
        public uint GenSource()
        {
            var source = 0;
            this.al.GenSources(1, ref source);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);

            return (uint)source;
        }

        // TODO: Move to extensions

        /// <inheritdoc/>
        public string GetErrorString(ALError param) => this.al.Get((ALGetString)param);

        /// <inheritdoc/>
        public int GetSource(uint sid, ALGetSourcei param)
        {
            this.al.GetSource(sid, param, out var result);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);

            return result;
        }

        /// <inheritdoc/>
        public bool GetSource(uint sid, ALSourceb param)
        {
            this.al.GetSource(sid, (ALGetSourcei)param, out var result);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);

            return result != 0;
        }

        /// <inheritdoc/>
        public float GetSource(uint sid, ALSourcef param)
        {
            this.al.GetSource(sid, param, out var value);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);

            return value;
        }

        /// <inheritdoc/>
        public int GetBuffer(uint bid, ALGetBufferi param)
        {
            this.al.GetBuffer(bid, param, out var value);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);

            return value;
        }

        /// <inheritdoc/>
        public ALSourceState GetSourceState(uint sid)
        {
            this.al.GetSource(sid, ALGetSourcei.SourceState, out var result);

            return (ALSourceState)result;
        }

        /// <inheritdoc/>
        public ALDevice GetContextsDevice(ALContext context)
        {
            var deviceResult = this.alc.GetContextsDevice(context);

            var error = this.alc.GetError(deviceResult);
            var errorMessage = Enum.GetName(typeof(AlcError), error);

            InvokeErrorIfTrue(error != AlcError.NoError, errorMessage);

            return deviceResult;
        }

        /// <inheritdoc/>
        public string GetString(ALDevice device, AlcGetString param)
        {
            var result = this.alc.GetString(device, param);
            var error = this.alc.GetError(device);

            var errorMessage = Enum.GetName(typeof(AlcError), error);

            InvokeErrorIfTrue(error != AlcError.NoError, errorMessage);

            return result;
        }

        /// TODO: Move to extensions
        /// <inheritdoc/>
        public IList<string> GetDeviceList()
        {
            unsafe
            {
                var nullDevice = new ALDevice(0);

                var stringsStart = this.alc.GetStringPtr(nullDevice, (AlcGetString)AlcGetStringList.AllDevicesSpecifier);
                var error = this.alc.GetError(nullDevice);
                var errorMessage = Enum.GetName(typeof(AlcError), error);

                InvokeErrorIfTrue(error != AlcError.NoError, errorMessage);

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
                fixed (TBuffer* b = buffer)
                {
                    this.al.BufferData(bid, format, b, buffer.Length * sizeof(TBuffer), freq);
                }
            }

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);
        }

        /// <inheritdoc/>
        public void BindBufferToSource(uint source, int buffer)
        {
            this.al.Source(source, ALSourcei.Buffer, buffer);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);
        }

        /// <inheritdoc/>
        public void Source(uint sid, ALSourcei param, int value)
        {
            this.al.Source(sid, param, value);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);
        }

        /// <inheritdoc/>
        public void Source(uint sid, ALSourceb param, bool value)
        {
            this.al.Source(sid, param, value);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);
        }

        /// <inheritdoc/>
        public void Source(uint sid, ALSourcef param, float value)
        {
            this.al.Source(sid, param, value);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);
        }

        /// <inheritdoc/>
        public void SourcePlay(uint sid)
        {
            this.al.SourcePlay(sid);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);
        }

        /// <inheritdoc/>
        public void SourcePause(uint sid)
        {
            this.al.SourcePause(sid);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);
        }

        /// <inheritdoc/>
        public void SourceStop(uint sid)
        {
            this.al.SourceStop(sid);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);
        }

        /// <inheritdoc/>
        public void SourceRewind(uint sid)
        {
            this.al.SourceRewind(sid);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);
        }

        /// <inheritdoc/>
        public bool CloseDevice(ALDevice device)
        {
            var closeResult = this.alc.CloseDevice(device);
            var error = this.alc.GetError(device);
            var errorMessage = Enum.GetName(typeof(AlcError), error);

            InvokeErrorIfTrue(error != AlcError.NoError, errorMessage);

            return closeResult;
        }

        /// <inheritdoc/>
        public void DeleteBuffer(uint buffer)
        {
            var castedBuffer = (int)buffer;
            this.al.DeleteBuffers(1, ref castedBuffer);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);
        }

        /// <inheritdoc/>
        public void DeleteSource(uint source)
        {
            var castedSource = (int)source;
            this.al.DeleteSources(1, ref castedSource);

            var error = GetError();
            var errorMessage = Enum.GetName(typeof(ALError), error);

            InvokeErrorIfTrue(error != ALError.NoError, errorMessage);
        }

        /// <inheritdoc/>
        public void DestroyContext(ALContext context)
        {
            var device = GetContextsDevice(context);

            this.alc.DestroyContext(context);
            var error = this.alc.GetError(device);
            var errorMessage = Enum.GetName(typeof(AlcError), error);

            InvokeErrorIfTrue(error != AlcError.NoError, errorMessage);
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
}
