// <copyright file="IOpenALInvoker.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.OpenAL;

using System;
using System.Collections.Generic;

/// <summary>
/// Invokes OpenAL functions.
/// </summary>
internal interface IOpenALInvoker
{
    /// <summary>
    /// Occurs when an OpenAL error occurs.
    /// </summary>
    event Action<string>? ErrorCallback;

    /// <summary>
    ///     Error support. Obtain the most recent error generated in the AL state machine.
    ///     When an error is detected by AL, a flag is set and the error code is recorded.
    ///     Further errors, if they occur, do not affect this recorded code. When alGetError
    ///     is called, the code is returned and the flag is cleared, so that a further error
    ///     will again record its code.
    /// </summary>
    /// <returns>
    ///     The first error that occurred. can be used with AL.GetString. Returns an OpenAL enum
    ///     representing the error state. When an OpenAL error occurs, the error state is
    ///     set and will not be changed until the error state is retrieved using alGetError.
    ///     Whenever alGetError is called, the error state is cleared and the last state
    ///     (the current state when the call was made) is returned. To isolate error detection
    ///     to a specific portion of code, alGetError should be called before the isolated
    ///     section to clear the current error state.
    /// </returns>
    ALError GetError();

    /// <summary>
    /// This function creates a context using a specified device.
    /// </summary>
    /// <param name="device">A pointer to a device.</param>
    /// <param name="attributes">The ALContext attributes to request.</param>
    /// <returns>Returns a pointer to the new context (NULL on failure).</returns>
    /// <remarks>
    ///     The attribute list can be NULL, or a zero terminated list of integer pairs composed
    ///     of valid ALC attribute tokens and requested values.
    /// </remarks>
    ALContext CreateContext(ALDevice device, ALContextAttributes attributes);

    /// <summary>
    /// This function opens a device by name.
    /// </summary>
    /// <param name="deviceName">A null-terminated string describing a device.</param>
    /// <returns>
    ///     Returns a pointer to the opened device. The return
    ///     value will be NULL if there is an error.
    /// </returns>
    ALDevice OpenDevice(string? deviceName);

    /// <summary>
    /// This function makes a specified context the current context.
    /// </summary>
    /// <param name="context">A pointer to the new context.</param>
    /// <returns>Returns <see langword="true"/> on success, or <see langword="false"/> on failure.</returns>
    bool MakeContextCurrent(ALContext context);

    /// <summary>
    ///     This function generates one buffer only, which contain audio data (see AL.BufferData).
    ///     References to buffers are uint values, which are used wherever a buffer reference
    ///     is needed (in calls such as AL.DeleteBuffers, AL.Source with parameter ALSourcei,
    ///     AL.SourceQueueBuffers, and AL.SourceUnqueueBuffers).
    /// </summary>
    /// <returns>Pointer to an uint value which will store the name of the new buffer.</returns>
    uint GenBuffer();

    /// <summary>
    ///     This function generates one or more buffers, which contain audio data (see
    ///     <see cref="BufferData{TBuffer}"/>). References to buffers are uint values, which are used wherever a
    ///     buffer reference is needed (in calls such as alDeleteBuffers, alSourcei,
    ///     alSourceQueueBuffers, and <see cref="SourceUnqueueBuffers"/>).
    /// </summary>
    /// <param name="count">The number of buffers to generate.</param>
    /// <returns>The array of buffer ids.</returns>
    uint[] GenBuffers(int count);

    /// <summary>
    /// This function generates one source only.
    /// </summary>
    /// <returns>Pointer to an int value which will store the name of the new source.</returns>
    uint GenSource();

    /// <summary>
    /// This function retrieves an OpenAL string property.
    /// </summary>
    /// <param name="param">The human-readable error string to be returned.</param>
    /// <returns>Returns a pointer to a null-terminated string.</returns>
    string GetErrorString(ALError param);

    /// <summary>
    /// This function retrieves an integer property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being retrieved.</param>
    /// <param name="param">
    ///     The name of the attribute to retrieve: ALSourcei.SourceRelative,
    ///     Buffer, SourceState, BuffersQueued, BuffersProcessed.
    /// </param>
    /// <returns>A pointer to the integer value being retrieved.</returns>
    int GetSource(uint source, ALGetSourcei param);

    /// <summary>
    /// This function retrieves a bool property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being retrieved.</param>
    /// <param name="param">The name of the attribute to get: ALSourceb.SourceRelative, Looping.</param>
    /// <returns>A pointer to the bool value being retrieved.</returns>
    bool GetSource(uint source, ALSourceb param);

    /// <summary>
    /// This function retrieves a floating-point property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being retrieved.</param>
    /// <param name="param">
    ///     The name of the attribute to set: ALSourcef.Pitch, Gain, MinGain, MaxGain, MaxDistance,
    ///     RolloffFactor, ConeOuterGain, ConeInnerAngle, ConeOuterAngle, SecOffset, ReferenceDistance,
    ///     EfxAirAbsorptionFactor, EfxRoomRolloffFactor, EfxConeOuterGainHighFrequency.
    /// </param>
    /// <returns>A pointer to the floating-point value being retrieved.</returns>
    float GetSource(uint source, ALSourcef param);

    /// <summary>
    /// <inheritdoc cref="AL.GetBuffer"/>
    /// </summary>
    /// <param name="bid">Buffer name whose attribute is being retrieved.</param>
    /// <param name="param">The name of the attribute to be retrieved: ALGetBufferi.Frequency, Bits, Channels, Size, and the currently hidden AL_DATA (dangerous).</param>
    /// <returns>The buffer stat.</returns>
    int GetBuffer(uint bid, ALGetBufferi param);

    /// <summary>
    /// This function retrieves an integer property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being retrieved.</param>
    /// <returns>The source state.</returns>
    ALSourceState GetSourceState(uint source);

    /// <summary>
    /// This function retrieves a context's device pointer.
    /// </summary>
    /// <param name="context">A pointer to a context.</param>
    /// <returns>Returns a pointer to the specified context's device.</returns>
    ALDevice GetContextsDevice(ALContext context);

    /// <summary>
    /// This strings related to the context.
    /// </summary>
    /// <param name="device">A pointer to the device to be queried.</param>
    /// <param name="param">
    ///     An attribute to be retrieved: ALC_DEFAULT_DEVICE_SPECIFIER, ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER,
    ///     ALC_DEVICE_SPECIFIER, ALC_CAPTURE_DEVICE_SPECIFIER, ALC_EXTENSIONS.
    /// </param>
    /// <returns>A string containing the name of the Device.</returns>
    /// <remarks>
    ///     ALC_DEFAULT_DEVICE_SPECIFIER will return the name of the default output device.
    ///     ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER will return the name of the default capture
    ///     device. ALC_DEVICE_SPECIFIER will return the name of the specified output device
    ///     if a pointer is supplied, or will return a list of all available devices if a
    ///     NULL device pointer is supplied. A list is a pointer to a series of strings separated
    ///     by NULL characters, with the list terminated by two NULL characters. See Enumeration
    ///     Extension for more details. ALC_CAPTURE_DEVICE_SPECIFIER will return the name
    ///     of the specified capture device if a pointer is supplied, or will return a list
    ///     of all available devices if a NULL device pointer is supplied. ALC_EXTENSIONS
    ///     returns a list of available context extensions, with each extension separated
    ///     by a space and the list terminated by a NULL character.
    /// </remarks>
    string GetString(ALDevice device, AlcGetString param);

    /// <summary>
    /// This function returns a List of strings related to the context.
    /// </summary>
    /// <returns>A List of strings containing the names of the Devices.</returns>
    /// <remarks>
    ///     ALC_DEVICE_SPECIFIER will return the name of the specified output device if a
    ///     pointer is supplied, or will return a list of all available devices if a NULL
    ///     device pointer is supplied. A list is a pointer to a series of strings separated
    ///     by NULL characters, with the list terminated by two NULL characters. See Enumeration
    ///     Extension for more details. ALC_CAPTURE_DEVICE_SPECIFIER will return the name
    ///     of the specified capture device if a pointer is supplied, or will return a list
    ///     of all available devices if a NULL device pointer is supplied. ALC_EXTENSIONS
    ///     returns a list of available context extensions, with each extension separated
    ///     by a space and the list terminated by a NULL character.
    /// </remarks>
    IList<string> GetDeviceList();

    /// <summary>
    /// Gets the name of the default audio device.
    /// </summary>
    /// <returns>The name of the default audio device.</returns>
    string GetDefaultDevice();

    /// <summary>
    ///     This function fills a buffer with audio buffer. All the predefined formats are
    ///     PCM buffer, but this function may be used by extensions to load other buffer
    ///     types as well.
    /// </summary>
    /// <typeparam name="TBuffer">The type of the data buffer.</typeparam>
    /// <param name="bid">Buffer Handle/Name to be filled with buffer.</param>
    /// <param name="format">
    ///     Format type from among the following: ALFormat.Mono8,
    ///     ALFormat.Mono16, ALFormat.Stereo8, ALFormat.Stereo16.
    /// </param>
    /// <param name="buffer">The audio buffer.</param>
    /// <param name="freq">The frequency of the audio data.</param>
    void BufferData<TBuffer>(uint bid, ALFormat format, TBuffer[] buffer, int freq)
        where TBuffer : unmanaged;

    /// <summary>
    /// Binds a Buffer to a Source handle.
    /// </summary>
    /// <param name="source">Source name to attach the Buffer to.</param>
    /// <param name="buffer">Buffer name which is attached to the source.</param>
    void BindBufferToSource(uint source, int buffer);

    /// <summary>
    /// This function queues a set of buffers on a source. All buffers attached to a source will be
    /// played in sequence, and the number of processed buffers can be detected using an
    /// <see cref="GetSource(uint,CASL.OpenAL.ALGetSourcei)"/> call to retrieve <see cref="ALGetSourcei.BuffersProcessed"/>.
    /// </summary>
    /// <param name="source">The name of the source to queue buffers onto.</param>
    /// <param name="count">The number of buffers to be queued.</param>
    /// <param name="buffers">The array of buffer names to be queued.</param>
    void SourceQueueBuffers(int source, int count, ref uint[] buffers);

    /// <summary>
    /// This function unqueues a set of buffers on a source. All buffers attached to a source will be
    /// played in sequence, and the number of processed buffers can be detected using an
    /// <see cref="GetSource(uint,CASL.OpenAL.ALGetSourcei)"/> call to retrieve <see cref="ALGetSourcei.BuffersProcessed"/>.
    /// </summary>
    /// <param name="source">The name of the source to queue buffers onto.</param>
    /// <param name="count">The number of buffers to be queued.</param>
    /// <param name="buffers">The array of buffer names to be queued.</param>
    void SourceUnqueueBuffers(int source, int count, ref uint[] buffers);

    /// <summary>
    /// This function sets an integer property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being set.</param>
    /// <param name="param">
    ///     The name of the attribute to set: ALSourcei.SourceRelative, ConeInnerAngle, ConeOuterAngle,
    ///     Looping, Buffer, SourceState.
    /// </param>
    /// <param name="value">The value to set the attribute to.</param>
    void Source(uint source, ALSourcei param, int value);

    /// <summary>
    /// This function sets an bool property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being set.</param>
    /// <param name="param">The name of the attribute to set: ALSourceb.SourceRelative, Looping.</param>
    /// <param name="value">The value to set the attribute to.</param>
    void Source(uint source, ALSourceb param, bool value);

    /// <summary>
    /// This function sets a floating-point property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being set.</param>
    /// <param name="param">
    ///     The name of the attribute to set: ALSourcef.Pitch, Gain, MinGain, MaxGain, MaxDistance,
    ///     RolloffFactor, ConeOuterGain, ConeInnerAngle, ConeOuterAngle, SecOffset, ReferenceDistance,
    ///     EfxAirAbsorptionFactor, EfxRoomRolloffFactor, EfxConeOuterGainHighFrequency.
    /// </param>
    /// <param name="value">The value to set the attribute to.</param>
    void Source(uint source, ALSourcef param, float value);

    /// <summary>
    ///     This function plays, replays or resumes a source. The playing source will have
    ///     it's state changed to ALSourceState.Playing. When called on a source which is
    ///     already playing, the source will restart at the beginning. When the attached
    ///     buffer(s) are done playing, the source will progress to the ALSourceState.Stopped
    ///     state.
    /// </summary>
    /// <param name="source">The name of the source to be played.</param>
    void SourcePlay(uint source);

    /// <summary>
    ///     This function pauses a source. The paused source will have its state changed
    ///     to ALSourceState.Paused.
    /// </summary>
    /// <param name="source">The name of the source to be paused.</param>
    void SourcePause(uint source);

    /// <summary>
    ///     This function stops a source. The stopped source will have it's state changed
    ///     to ALSourceState.Stopped.
    /// </summary>
    /// <param name="source">The name of the source to be stopped.</param>
    void SourceStop(uint source);

    /// <summary>
    ///     This function stops the source and sets its state to ALSourceState.Initial.
    /// </summary>
    /// <param name="source">The name of the source to be rewound.</param>
    void SourceRewind(uint source);

    /// <summary>
    /// This function closes a device by name.
    /// </summary>
    /// <param name="device">A pointer to an opened device.</param>
    /// <returns>
    ///     <see langword="true"/> will be returned on success or <see langword="false"/> on failure. Closing a device will fail
    ///     if the device contains any contexts or buffers.
    /// </returns>
    bool CloseDevice(ALDevice device);

    /// <summary>
    ///     This function deletes one buffer only, freeing the resources used by the buffer.
    ///     Buffers which are attached to a source can not be deleted. See AL.Source (ALSourcei)
    ///     and AL.SourceUnqueueBuffers for information on how to detach a buffer from a
    ///     source.
    /// </summary>
    /// <param name="buffer">Pointer to a buffer name identifying the buffer to be deleted.</param>
    void DeleteBuffer(uint buffer);

    /// <summary>
    /// This function deletes one source only.
    /// </summary>
    /// <param name="source">Pointer to a source name identifying the source to be deleted.</param>
    void DeleteSource(uint source);

    /// <summary>
    /// This function destroys a context.
    /// </summary>
    /// <param name="context">A pointer to the new context.</param>
    void DestroyContext(ALContext context);
}
