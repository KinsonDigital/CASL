// <copyright file="AL.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

/* References:
 * OpenAL documentation and other resources can be found at http://www.openal.org/documentation/
 */

// ReSharper disable UnusedType.Local
namespace CASL.OpenAL;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Exceptions;
using NativeInterop;
using NativeInterop.Factories;

/// <summary>
/// Provides access to OpenAL functions.
/// </summary>
[ExcludeFromCodeCoverage]
internal class AL
{
    private readonly ALGetError alGetError;
    private readonly ALGenBuffers alGenBuffers;
    private readonly ALGenSources alGenSources;
    private readonly ALGet alGet;
    private readonly ALGetSourceInt alGetSourceInt;
    private readonly ALGetSourceFloat alGetSourceFloat;
    private readonly ALGetBufferInt alGetBufferInt;
    private readonly ALBufferData alBufferData;
    private readonly ALSourceInt alSourceInt;
    private readonly ALSourceBool alSourceBool;
    private readonly ALSourceFloat alSourceFloat;
    private readonly ALSourcePlay alSourcePlay;
    private readonly ALSourcePause alSourcePause;
    private readonly ALSourceStop alSourceStop;
    private readonly ALSourceRewind alSourceRewind;
    private readonly ALDeleteBuffers alDeleteBuffers;
    private readonly ALDeleteSources alDeleteSources;

    /// <summary>
    /// Initializes a new instance of the <see cref="AL"/> class.
    /// </summary>
    /// <param name="libraryLoader">Loads the OpenAL library.</param>
    /// <param name="delegateFactory">Creates delegates to the loaded native library functions.</param>
    public AL(ILibraryLoader libraryLoader, IDelegateFactory delegateFactory)
    {
        var libraryPointer = libraryLoader.LoadLibrary();

        if (libraryPointer == 0)
        {
            throw new LoadLibraryException($"The library '{libraryLoader.LibraryName}' could not be loaded.");
        }

        this.alGetError = delegateFactory.CreateDelegate<ALGetError>(libraryPointer, nameof(this.alGetError));
        this.alGenBuffers = delegateFactory.CreateDelegate<ALGenBuffers>(libraryPointer, nameof(this.alGenBuffers));
        this.alGenSources = delegateFactory.CreateDelegate<ALGenSources>(libraryPointer, nameof(this.alGenSources));
        this.alGet = delegateFactory.CreateDelegate<ALGet>(libraryPointer, "alGetString");
        this.alGetSourceInt = delegateFactory.CreateDelegate<ALGetSourceInt>(libraryPointer, "alGetSourcei");
        this.alGetSourceFloat = delegateFactory.CreateDelegate<ALGetSourceFloat>(libraryPointer, "alGetSourcef");
        this.alGetBufferInt = delegateFactory.CreateDelegate<ALGetBufferInt>(libraryPointer, "alGetBufferi");
        this.alBufferData = delegateFactory.CreateDelegate<ALBufferData>(libraryPointer, nameof(this.alBufferData));
        this.alSourceInt = delegateFactory.CreateDelegate<ALSourceInt>(libraryPointer, "alSourcei");
        this.alSourceBool = delegateFactory.CreateDelegate<ALSourceBool>(libraryPointer, "alSourcei");
        this.alSourceFloat = delegateFactory.CreateDelegate<ALSourceFloat>(libraryPointer, "alSourcef");
        this.alSourcePlay = delegateFactory.CreateDelegate<ALSourcePlay>(libraryPointer, nameof(this.alSourcePlay));
        this.alSourcePause = delegateFactory.CreateDelegate<ALSourcePause>(libraryPointer, nameof(this.alSourcePause));
        this.alSourceStop = delegateFactory.CreateDelegate<ALSourceStop>(libraryPointer, nameof(this.alSourceStop));
        this.alSourceRewind = delegateFactory.CreateDelegate<ALSourceRewind>(libraryPointer, nameof(this.alSourceRewind));
        this.alDeleteBuffers = delegateFactory.CreateDelegate<ALDeleteBuffers>(libraryPointer, nameof(this.alDeleteBuffers));
        this.alDeleteSources = delegateFactory.CreateDelegate<ALDeleteSources>(libraryPointer, nameof(this.alDeleteSources));
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate ALError ALGetError();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALGenBuffers(int n, ref uint buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALGenSources(int n, ref uint sources);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
    private delegate string ALGet(ALGetString param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALGetSourceInt(uint source, ALGetSourcei param, [Out] out int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALGetSourceFloat(uint source, ALSourcef param, out float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALGetBufferInt(uint buffer, ALGetBufferi param, [Out] out int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate void ALBufferData(uint buffer, ALFormat format, void* data, int size, int freq);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSourceInt(uint source, ALSourcei param, int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSourceBool(uint source, ALSourceb param, bool value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSourceFloat(uint source, ALSourcef param, float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSourcePlay(uint source);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSourcePause(uint source);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSourceStop(uint source);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSourceRewind(uint source);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALDeleteBuffers(int n, [In] ref uint buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALDeleteSources(int n, ref uint sources);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSpeedOfSound(float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALDopplerVelocity(float value);

    /// <summary>
    /// Gets the OpenAL context API.
    /// </summary>
    /// <returns>The api object.</returns>
    public static AL GetApi() => IoC.Container.GetInstance<AL>();

    /// <summary>
    /// Error support. Obtain the most recent error generated in the AL state machine. When an error is detected by AL, a flag is set and the error code is recorded. Further errors, if they occur, do not affect this recorded code. When alGetError is called, the code is returned and the flag is cleared, so that a further error will again record its code.</summary>
    /// <returns>
    /// The first error that occured. can be used with AL.GetString. Returns an enum representing the error state. When an OpenAL error occurs, the error state is set and will not be changed until the error state is retrieved using alGetError. Whenever alGetError is called, the error state is cleared and the last state (the current state when the call was made) is returned. To isolate error detection to a specific portion of code, alGetError should be called before the isolated section to clear the current error state.
    /// </returns>
    public ALError GetError() => this.alGetError();

    /// <summary>
    /// This function generates one or more buffers, which contain audio buffer (see AL.BufferData).
    /// </summary>
    /// <param name="n">The number of buffers to be generated.</param>
    /// <param name="buffers">Pointer to an array of int values which will store the names of the new buffers.</param>
    public void GenBuffers(int n, ref uint buffers) => this.alGenBuffers(n, ref buffers);

    /// <summary>
    /// This function generates one or more sources.
    /// </summary>
    /// <param name="n">The number of sources to be generated.</param>
    /// <param name="sources">Pointer to an array of int values which will store the names of the new sources.</param>
    public void GenSources(int n, ref uint sources) => this.alGenSources(n, ref sources);

    /// <summary>
    /// This function retrieves an OpenAL string property.
    /// </summary>
    /// <param name="param">The property to be returned: Vendor, Version, Renderer and Extensions.</param>
    /// <returns>
    /// Returns a pointer to a null-terminated string.
    /// </returns>
    public string Get(ALGetString param) => this.alGet(param);

    /// <summary>
    /// This function retrieves an integer property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being retrieved.</param>
    /// <param name="param">The name of the attribute to retrieve: ALSourcei.SourceRelative, Buffer, SourceState, BuffersQueued, BuffersProcessed.</param>
    /// <param name="value">A pointer to the integer value being retrieved.</param>
    public void GetSource(uint source, ALGetSourcei param, [Out] out int value) => this.alGetSourceInt(source, param, out value);

    /// <summary>
    /// This function retrieves a floating-point property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being retrieved.</param>
    /// <param name="param">The name of the attribute to set: ALSourcef.Pitch, Gain, MinGain, MaxGain, MaxDistance, RolloffFactor, ConeOuterGain, ConeInnerAngle, ConeOuterAngle, SecOffset, ReferenceDistance, EfxAirAbsorptionFactor, EfxRoomRolloffFactor, EfxConeOuterGainHighFrequency.</param>
    /// <param name="value">A pointer to the floating-point value being retrieved.</param>
    public void GetSource(uint source, ALSourcef param, out float value) => this.alGetSourceFloat(source, param, out value);

    /// <summary>
    /// This function retrieves an integer property of a buffer.
    /// </summary>
    /// <param name="buffer">Buffer name whose attribute is being retrieved.</param>
    /// <param name="param">The name of the attribute to be retrieved: ALGetBufferi.Frequency, Bits, Channels, Size, and the currently hidden AL_DATA (dangerous).</param>
    /// <param name="value">A pointer to an int to hold the retrieved buffer.</param>
    public void GetBuffer(uint buffer, ALGetBufferi param, [Out] out int value) => this.alGetBufferInt(buffer, param, out value);

    /// <summary>
    /// This function fills a buffer with audio buffer. All the pre-defined formats are PCM buffer, but this function may be used by extensions to load other buffer types as well.
    /// </summary>
    /// <param name="buffer">buffer Handle/Name to be filled with buffer.</param>
    /// <param name="format">Format type from among the following: ALFormat.Mono8, ALFormat.Mono16, ALFormat.Stereo8, ALFormat.Stereo16.</param>
    /// <param name="data">Pointer to a pinned audio buffer.</param>
    /// <param name="size">The size of the audio buffer in bytes.</param>
    /// <param name="freq">The frequency of the audio buffer.</param>
    public unsafe void BufferData(uint buffer, ALFormat format, void* data, int size, int freq) => this.alBufferData(buffer, format, data, size, freq);

    /// <summary>
    /// This function sets an integer property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being set.</param>
    /// <param name="param">The name of the attribute to set: ALSourcei.SourceRelative, ConeInnerAngle, ConeOuterAngle, Looping, Buffer, SourceState.</param>
    /// <param name="value">The value to set the attribute to.</param>
    public void Source(uint source, ALSourcei param, int value) => this.alSourceInt(source, param, value);

    /// <summary>
    /// This function sets an bool property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being set.</param>
    /// <param name="param">The name of the attribute to set: ALSourceb.SourceRelative, Looping.</param>
    /// <param name="value">The value to set the attribute to.</param>
    public void Source(uint source, ALSourceb param, bool value) => this.alSourceBool(source, param, value);

    /// <summary>
    /// This function sets a floating-point property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being set.</param>
    /// <param name="param">The name of the attribute to set: ALSourcef.Pitch, Gain, MinGain, MaxGain, MaxDistance, RolloffFactor, ConeOuterGain, ConeInnerAngle, ConeOuterAngle, SecOffset, ReferenceDistance, EfxAirAbsorptionFactor, EfxRoomRolloffFactor, EfxConeOuterGainHighFrequency.</param>
    /// <param name="value">The value to set the attribute to.</param>
    public void Source(uint source, ALSourcef param, float value) => this.alSourceFloat(source, param, value);

    /// <summary>
    /// This function plays, replays or resumes a source. The playing source will have it's state changed to ALSourceState.Playing. When called on a source which is already playing, the source will restart at the beginning. When the attached buffer(s) are done playing, the source will progress to the ALSourceState.Stopped state.
    /// </summary>
    /// <param name="source">The name of the source to be played.</param>
    public void SourcePlay(uint source) => this.alSourcePlay(source);

    /// <summary>
    /// This function pauses a source. The paused source will have its state changed to ALSourceState.Paused.
    /// </summary>
    /// <param name="source">The name of the source to be paused.</param>
    public void SourcePause(uint source) => this.alSourcePause(source);

    /// <summary>
    /// This function stops a source. The stopped source will have it's state changed to ALSourceState.Stopped.
    /// </summary>
    /// <param name="source">The name of the source to be stopped.</param>
    public void SourceStop(uint source) => this.alSourceStop(source);

    /// <summary>
    /// This function stops the source and sets its state to ALSourceState.Initial.
    /// </summary>
    /// <param name="source">The name of the source to be rewound.</param>
    public void SourceRewind(uint source) => this.alSourceRewind(source);

    /// <summary>
    /// This function deletes one or more buffers, freeing the resources used by the buffer. Buffers which are attached to a source can not be deleted. See AL.Source (ALSourcei) and AL.SourceUnqueueBuffers for information on how to detach a buffer from a source.
    /// </summary>
    /// <param name="n">The number of buffers to be deleted.</param>
    /// <param name="buffers">Pointer to an array of buffer names identifying the buffers to be deleted.</param>
    public void DeleteBuffers(int n, [In] ref uint buffers) => this.alDeleteBuffers(n, ref buffers);

    /// <summary>
    /// This function deletes one or more sources.
    /// </summary>
    /// <param name="n">The number of sources to be deleted.</param>
    /// <param name="sources">Reference to a single source, or an array of source names identifying the sources to be deleted.</param>
    public void DeleteSources(int n, ref uint sources) => this.alDeleteSources(n, ref sources);
}
