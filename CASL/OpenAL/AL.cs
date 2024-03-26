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
    private readonly ALSourceFloat alSourceFloat;
    private readonly ALSourceQueueBuffers alSourceQueueBuffers;
    private readonly ALSourceUnqueueBuffers alSourceUnqueueBuffers;
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
        this.alSourceQueueBuffers = delegateFactory.CreateDelegate<ALSourceQueueBuffers>(libraryPointer, nameof(this.alSourceQueueBuffers));
        this.alSourceUnqueueBuffers = delegateFactory.CreateDelegate<ALSourceUnqueueBuffers>(libraryPointer, nameof(this.alSourceUnqueueBuffers));
        this.alSourceFloat = delegateFactory.CreateDelegate<ALSourceFloat>(libraryPointer, "alSourcef");
        this.alSourcePlay = delegateFactory.CreateDelegate<ALSourcePlay>(libraryPointer, nameof(this.alSourcePlay));
        this.alSourcePause = delegateFactory.CreateDelegate<ALSourcePause>(libraryPointer, nameof(this.alSourcePause));
        this.alSourceStop = delegateFactory.CreateDelegate<ALSourceStop>(libraryPointer, nameof(this.alSourceStop));
        this.alSourceRewind = delegateFactory.CreateDelegate<ALSourceRewind>(libraryPointer, nameof(this.alSourceRewind));
        this.alDeleteBuffers = delegateFactory.CreateDelegate<ALDeleteBuffers>(libraryPointer, nameof(this.alDeleteBuffers));
        this.alDeleteSources = delegateFactory.CreateDelegate<ALDeleteSources>(libraryPointer, nameof(this.alDeleteSources));
    }

    /// <summary>
    /// Returns the current error state and then clears the error state.
    /// <br/>
    /// <br/>
    /// When an OpenAL error occurs, the error state is set and will not be changed until the error state is retrieved using alGetError.
    /// Whenever alGetError is called, the error state is cleared and the last state (the current
    /// state when the call was made) is returned. To isolate error detection to a specific portion
    /// of code, alGetError should be called before the isolated section to clear the current error state.
    /// </summary>
    /// <returns><see cref="ALError"/> representing the error state.</returns>
    /// <remarks>Requires OpenAL 1.0 or higher.</remarks>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate ALError ALGetError();

    /// <summary>
    /// Generates one or more buffers, which contain audio data <see cref="ALBufferData"/>
    /// References to buffers are <c>uint[]</c> values, which are used wherever a
    /// buffer reference is needed (in calls such as <see cref="ALDeleteBuffers"/>, <see cref="ALSourceInt"/>,
    /// <see cref="ALSourceQueueBuffers"/>, and <see cref="ALSourceUnqueueBuffers"/>).
    /// </summary>
    /// <param name="n">fThe number of buffers to be generated.</param>
    /// <param name="buffers">An array of <c>uint</c> values which will store the names of the new buffers.</param>
    /// <remarks>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// If the requested number of buffers cannot be created, an error will be generated which
    /// can be detected with <see cref="ALGetError"/>. If an error occurs, no buffers will be generated. If n
    /// equals zero, alGenBuffers does nothing and does not return an error.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.InvalidValue"/></item>
    ///         <item><see cref="ALError.OutOfMemory"/></item>
    ///     </list>
    /// </remarks>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALGenBuffers(int n, ref uint buffers);

    /// <summary>
    /// Generates one or more sources.
    /// <br/>
    /// References to sources are <c>uint</c> values, which are used wherever a source reference is needed
    /// (in calls such as <see cref="ALDeleteSources"/> and <see cref="ALSourceInt"/>).
    /// </summary>
    /// <param name="n">The number of sources to be generated.</param>
    /// <param name="sources">An array of <c>uint</c> values which will store the names of the new sources.</param>
    /// <remarks>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// If the requested number of sources cannot be created, an error will be generated which
    /// can be detected with <see cref="ALGetError"/>. If an error occurs, no sources will be generated. If n
    /// equals zero, <see cref="ALGenSources"/> does nothing and does not return an error.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.OutOfMemory"/></item>
    ///         <item><see cref="ALError.InvalidValue"/></item>
    ///         <item><see cref="ALError.InvalidOperation"/></item>
    ///     </list>
    /// </remarks>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALGenSources(int n, ref uint sources);

    /// <summary>
    /// Retrieves an OpenAL string property.
    /// </summary>
    /// <returns>The property to be returned.</returns>
    /// <remarks>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.InvalidEnum"/></item>
    ///     </list>
    /// </remarks>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
    private delegate string ALGet(ALGetString param);

    /// <summary>
    /// Retrieves an integer property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being retrieved.</param>
    /// <param name="param">The name of the attribute to retrieve.</param>
    /// <param name="value">The integer value being retrieved.</param>
    /// <seealso cref="ALSourceInt"/>
    /// <remarks>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.InvalidValue"/></item>
    ///         <item><see cref="ALError.InvalidEnum"/></item>
    ///         <item><see cref="ALError.InvalidName"/></item>
    ///         <item><see cref="ALError.InvalidOperation"/></item>
    ///     </list>
    /// </remarks>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALGetSourceInt(uint source, ALGetSourcei param, [Out] out int value);

    /// <summary>
    /// Retrieves a floating point property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being retrieved.</param>
    /// <param name="param">The name of the attribute to retrieve.</param>
    /// <param name="value">The floating point value being retrieved.</param>
    /// <remarks>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.InvalidValue"/></item>
    ///         <item><see cref="ALError.InvalidEnum"/></item>
    ///         <item><see cref="ALError.InvalidName"/></item>
    ///         <item><see cref="ALError.InvalidOperation"/></item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="ALSourceFloat"/>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALGetSourceFloat(uint source, ALSourcef param, [Out] out float value);

    /// <summary>
    /// Retrieves an integer property of a buffer.
    /// </summary>
    /// <param name="buffer">Buffer name whose attribute is being retrieved.</param>
    /// <param name="param">The name of the attribute to be retrieved.</param>
    /// <param name="value">An <c>int</c> to hold the retrieved data.</param>
    /// <remarks>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.InvalidEnum"/></item>
    ///         <item><see cref="ALError.InvalidName"/></item>
    ///         <item><see cref="ALError.InvalidValue"/></item>
    ///     </list>
    /// </remarks>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALGetBufferInt(uint buffer, ALGetBufferi param, [Out] out int value);

    /// <summary>
    /// Fills a buffer with audio data. All the pre-defined formats are PCM data, but
    /// this function may be used by extensions to load other data types as well.
    /// </summary>
    /// <param name="buffer">Buffer name to be filled with data.</param>
    /// <param name="format">The format type.</param>
    /// <param name="data">Pointer to the audio data.</param>
    /// <param name="size">The size of the audio data in bytes.</param>
    /// <param name="freq">The frequency of the audio data.</param>
    /// <remarks>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// 8-bit PCM data is expressed as an unsigned value over the range 0 to 255, 128 being an
    /// audio output level of zero. 16-bit PCM data is expressed as a signed value over the
    /// range -32768 to 32767, 0 being an audio output level of zero. Stereo data is expressed
    /// in interleaved format, left channel first. Buffers containing more than one channel of data
    /// will be played without 3D spatialization.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.OutOfMemory"/></item>
    ///         <item><see cref="ALError.InvalidValue"/></item>
    ///         <item><see cref="ALError.InvalidEnum"/></item>
    ///     </list>
    /// </remarks>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate void ALBufferData(uint buffer, ALFormat format, void* data, int size, int freq);

    /// <summary>
    /// Sets an integer property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being set.</param>
    /// <param name="param">The name of the attribute to set.</param>
    /// <param name="value">The value to set the attribute to.</param>
    /// <remarks>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// The buffer name zero is reserved as a “NULL Buffer" and is accepted by alSourcei(…,
    /// AL_BUFFER, …) as a valid buffer of zero length. The NULL Buffer is extremely useful
    /// for detaching buffers from a source which were attached using this call or with <see cref="ALSourceQueueBuffers"/>.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.InvalidValue"/></item>
    ///         <item><see cref="ALError.InvalidEnum"/></item>
    ///         <item><see cref="ALError.InvalidName"/></item>
    ///         <item><see cref="ALError.InvalidOperation"/></item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="ALGetSourceInt"/>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSourceInt(uint source, ALSourcei param, int value);

    /// <summary>
    /// Sets an floating point property of a source.
    /// </summary>
    /// <param name="source">Source name whose attribute is being set.</param>
    /// <param name="param">The name of the attribute to set.</param>
    /// <param name="value">The value to set the attribute to.</param>
    /// <remarks>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.InvalidValue"/></item>
    ///         <item><see cref="ALError.InvalidEnum"/></item>
    ///         <item><see cref="ALError.InvalidName"/></item>
    ///         <item><see cref="ALError.InvalidOperation"/></item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="ALGetSourceFloat"/>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSourceFloat(uint source, ALSourcef param, float value);

    /// <summary>
    /// This function queues a set of buffers on a source. All buffers attached to a source will be
    /// played in sequence, and the number of processed buffers can be detected using an
    /// <see cref="ALSourcei"/> call to retrieve <see cref="ALGetSourcei.BuffersProcessed"/>.
    /// </summary>
    /// <param name="source">The name of the source to queue buffers onto.</param>
    /// <param name="n">The number of buffers to be queued.</param>
    /// <param name="buffers">A pointer to an array of buffer names to be queued.</param>
    /// <remarks>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.InvalidName"/></item>
    ///         <item><see cref="ALError.InvalidOperation"/></item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="ALSourceUnqueueBuffers"/>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSourceQueueBuffers(uint source, uint n, ref uint buffers);

    /// <summary>
    /// This function unqueues a set of buffers attached to a source. The number of processed
    /// buffers can be detected using an <see cref="ALSourcei"/> call to retrieve <see cref="ALGetSourcei.BuffersProcessed"/>,
    /// which is the maximum number of buffers that can be unqueued using this call.
    /// </summary>
    /// <param name="source">The name of the source to queue buffers onto.</param>
    /// <param name="n">The number of buffers to be queued.</param>
    /// <param name="buffers">A pointer to an array of buffer names to be queued.</param>
    /// <remarks>
    /// The unqueue operation will only take place if all n buffers can be removed from the queue.
    /// <br/>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.InvalidValue"/></item>
    ///         <item><see cref="ALError.InvalidName"/></item>
    ///         <item><see cref="ALError.InvalidOperation"/></item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="ALSourceQueueBuffers"/>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSourceUnqueueBuffers(uint source, uint n, ref uint buffers);

    /// <summary>
    /// This function plays a source.
    /// </summary>
    /// <param name="source">The name of the source to be played.</param>
    /// <remarks>
    /// The playing source will have its state changed to <see cref="ALSourceState.Playing"/>. When called on a source
    /// which is already playing, the source will restart at the beginning. When the attached
    /// buffer(s) are done playing, the source will progress to the <see cref="ALSourceState.Stopped"/> state.
    /// <br/>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.InvalidName"/></item>
    ///         <item><see cref="ALError.InvalidOperation"/></item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="ALSourcePause"/>
    /// <seealso cref="ALSourceRewind"/>
    /// <seealso cref="ALSourceStop"/>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSourcePlay(uint source);

    /// <summary>
    /// This function pauses a source.
    /// </summary>
    /// <param name="source">The name of the source to be paused.</param>
    /// <remarks>
    /// The paused source will have its state changed to <see cref="ALSourceState.Paused"/>.
    /// <br/>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.InvalidName"/></item>
    ///         <item><see cref="ALError.InvalidOperation"/></item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="ALSourcePlay"/>
    /// <seealso cref="ALSourceRewind"/>
    /// <seealso cref="ALSourceStop"/>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSourcePause(uint source);

    /// <summary>
    /// This function stops a source.
    /// </summary>
    /// <param name="source">The name of the source to be stopped.</param>
    /// <remarks>
    /// The stopped source will have its state changed to <see cref="ALSourceState.Stopped"/>.
    /// <br/>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.InvalidName"/></item>
    ///         <item><see cref="ALError.InvalidOperation"/></item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="ALSourcePlay"/>
    /// <seealso cref="ALSourceRewind"/>
    /// <seealso cref="ALSourcePause"/>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSourceStop(uint source);

    /// <summary>
    /// This function stops the source and sets its state to <see cref="ALSourceState.Initial"/>.
    /// </summary>
    /// <param name="source">The name of the source to be rewound.</param>
    /// <remarks>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.InvalidName"/></item>
    ///         <item><see cref="ALError.InvalidOperation"/></item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="ALSourcePlay"/>
    /// <seealso cref="ALSourceStop"/>
    /// <seealso cref="ALSourcePause"/>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALSourceRewind(uint source);

    /// <summary>
    /// This function stops the source and sets its state to <see cref="ALSourceState.Initial"/>.
    /// </summary>
    /// <param name="n">The number of buffers to be deleted.</param>
    /// <param name="buffers">Pointer to an array of buffer names identifying the buffers to be deleted.</param>
    /// <remarks>
    /// If the requested number of buffers cannot be deleted, an error will be generated which
    /// can be detected with <see cref="ALGetError"/>. If an error occurs, no buffers will be deleted. If n equals
    /// zero, <see cref="ALDeleteBuffers"/> does nothing and will not return an error.
    /// <br/>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.InvalidName"/></item>
    ///         <item><see cref="ALError.InvalidOperation"/></item>
    ///         <item><see cref="ALError.InvalidValue"/></item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="ALGenBuffers"/>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALDeleteBuffers(int n, [In] ref uint buffers);

    /// <summary>
    /// This function deletes one or more sources.
    /// </summary>
    /// <param name="n">The number of buffers to be deleted.</param>
    /// <param name="sources">Pointer ot an array of source names identifying the sources to be deleted.</param>
    /// <remarks>
    /// If the requested number of sources cannot be deleted, an error will be generated which
    /// can be detected with <see cref="ALGetError"/>. If an error occurs, no sources will be deleted. If <paramref name="n"/>
    /// equals zero, <see cref="ALDeleteSources"/> does nothing and will not return an error.
    /// A playing source can be deleted – the source will be stopped and then deleted.
    /// <br/>
    /// Requires OpenAL 1.0 or higher.
    /// <br/>
    /// Possible Errors:
    ///     <list type="bullet">
    ///         <item><see cref="ALError.InvalidName"/></item>
    ///         <item><see cref="ALError.InvalidOperation"/></item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="ALGenSources"/>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALDeleteSources(int n, ref uint sources);

    /// <summary>
    /// Gets the OpenAL context API.
    /// </summary>
    /// <returns>The api object.</returns>
    public static AL GetApi() => IoC.Container.GetInstance<AL>();

    /// <inheritdoc cref="ALGetError"/>
    public ALError GetError() => this.alGetError();

    /// <inheritdoc cref="ALGenBuffers"/>
    public void GenBuffers(int n, ref uint buffers) => this.alGenBuffers(n, ref buffers);

    /// <inheritdoc cref="ALGenSources"/>
    public void GenSources(int n, ref uint sources) => this.alGenSources(n, ref sources);

    /// <inheritdoc cref="ALGet"/>
    public string Get(ALGetString param) => this.alGet(param);

    /// <inheritdoc cref="ALGetSourceInt"/>
    public void GetSource(uint source, ALGetSourcei param, [Out] out int value) => this.alGetSourceInt(source, param, out value);

    /// <inheritdoc cref="ALGetSourceFloat"/>
    public void GetSource(uint source, ALSourcef param, [Out] out float value) => this.alGetSourceFloat(source, param, out value);

    /// <inheritdoc cref="ALSourceQueueBuffers"/>
    public void SourceQueueBuffers(uint source, int n, ref uint buffers) => this.alSourceQueueBuffers(source, (uint)n, ref buffers);

    /// <inheritdoc cref="ALSourceUnqueueBuffers"/>
    public void SourceUnqueueBuffers(uint source, int n, ref uint buffers) => this.alSourceUnqueueBuffers(source, (uint)n, ref buffers);

    /// <inheritdoc cref="ALGetBufferInt"/>
    public void GetBuffer(uint buffer, ALGetBufferi param, [Out] out int value) => this.alGetBufferInt(buffer, param, out value);

    /// <inheritdoc cref="ALBufferData"/>
    public unsafe void BufferData(uint buffer, ALFormat format, void* data, int size, int freq) => this.alBufferData(buffer, format, data, size, freq);

    /// <inheritdoc cref="ALSourceInt"/>
    public void Source(uint source, ALSourcei param, int value) => this.alSourceInt(source, param, value);

    /// <inheritdoc cref="ALSourceInt"/>
    public void Source(uint source, ALSourceb param, bool value) => this.alSourceInt(source, (ALSourcei)param, value ? 1 : 0);

    /// <inheritdoc cref="ALSourceFloat"/>
    public void Source(uint source, ALSourcef param, float value) => this.alSourceFloat(source, param, value);

    /// <inheritdoc cref="ALSourcePlay"/>
    public void SourcePlay(uint source) => this.alSourcePlay(source);

    /// <inheritdoc cref="ALSourcePause"/>
    public void SourcePause(uint source) => this.alSourcePause(source);

    /// <inheritdoc cref="ALSourceStop"/>
    public void SourceStop(uint source) => this.alSourceStop(source);

    /// <inheritdoc cref="ALSourceRewind"/>
    public void SourceRewind(uint source) => this.alSourceRewind(source);

    /// <inheritdoc cref="ALDeleteBuffers"/>
    public void DeleteBuffers(int n, [In] ref uint buffers) => this.alDeleteBuffers(n, ref buffers);

    /// <inheritdoc cref="ALDeleteSources"/>
    public void DeleteSources(int n, ref uint sources) => this.alDeleteSources(n, ref sources);
}
