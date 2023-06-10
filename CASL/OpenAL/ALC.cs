// <copyright file="ALC.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.OpenAL;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using CASL.Exceptions;
using CASL.NativeInterop;
using CASL.NativeInterop.Factories;

/// <summary>
/// Provides access to OpenAL context functions.
/// </summary>
[ExcludeFromCodeCoverage]
internal class ALC
{
    private static nint libraryPointer;

    private readonly ALCGetError alcGetError;
    private readonly ALCOpenDevice alcOpenDevice;
    private readonly ALCCreateContext alcCreateContext;
    private readonly ALCMakeContextCurrent alcMakeContextCurrent;
    private readonly ALCGetContextsDevice alcGetContextsDevice;
    private readonly ALCGetString alcGetString;
    private readonly ALCGetStringPtr alcGetStringPtr;
    private readonly ALCCloseDevice alcCloseDevice;
    private readonly ALCDestroyContext alcDestroyContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ALC"/> class.
    /// </summary>
    /// <param name="libraryLoader">Loads the OpenAL library.</param>
    /// <param name="delegateFactory">Creates delegates to the loaded native library functions.</param>
    public ALC(ILibraryLoader libraryLoader, IDelegateFactory delegateFactory)
    {
        libraryPointer = libraryLoader.LoadLibrary();

        if (libraryPointer == 0)
        {
            throw new LoadLibraryException($"The library '{libraryLoader.LibraryName}' could not be loaded.");
        }

        this.alcGetError = delegateFactory.CreateDelegate<ALCGetError>(libraryPointer, nameof(this.alcGetError));
        this.alcOpenDevice = delegateFactory.CreateDelegate<ALCOpenDevice>(libraryPointer, nameof(this.alcOpenDevice));
        this.alcCreateContext = delegateFactory.CreateDelegate<ALCCreateContext>(libraryPointer, nameof(this.alcCreateContext));
        this.alcMakeContextCurrent = delegateFactory.CreateDelegate<ALCMakeContextCurrent>(libraryPointer, nameof(this.alcMakeContextCurrent));
        this.alcGetContextsDevice = delegateFactory.CreateDelegate<ALCGetContextsDevice>(libraryPointer, nameof(this.alcGetContextsDevice));
        this.alcGetString = delegateFactory.CreateDelegate<ALCGetString>(libraryPointer, nameof(this.alcGetString));
        this.alcGetStringPtr = delegateFactory.CreateDelegate<ALCGetStringPtr>(libraryPointer, nameof(this.alcGetString));
        this.alcCloseDevice = delegateFactory.CreateDelegate<ALCCloseDevice>(libraryPointer, nameof(this.alcCloseDevice));
        this.alcDestroyContext = delegateFactory.CreateDelegate<ALCDestroyContext>(libraryPointer, nameof(this.alcDestroyContext));
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate AlcError ALCGetError([In] ALDevice device);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate ALDevice ALCOpenDevice(string? devicename);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate ALContext ALCCreateContext([In] ALDevice device, [In] int[] attributeList);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool ALCMakeContextCurrent(ALContext context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate ALDevice ALCGetContextsDevice(ALContext context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
    private delegate string ALCGetString(ALDevice device, AlcGetString param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private unsafe delegate byte* ALCGetStringPtr([In] ALDevice device, AlcGetString param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool ALCCloseDevice([In] ALDevice device);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ALCDestroyContext(ALContext context);

    /// <summary>
    /// Gets the OpenAL context API.
    /// </summary>
    /// <returns>The api object.</returns>
    public static ALC GetApi() => IoC.Container.GetInstance<ALC>();

    /// <summary>
    /// This function retrieves the current context error state.
    /// </summary>
    /// <param name="device">A pointer to the device to retrieve the error state from.</param>
    /// <returns>
    ///     Errorcode Int32.
    /// </returns>
    public AlcError GetError([In] ALDevice device) => this.alcGetError(device);

    /// <summary>
    /// This function creates a context using a specified device.
    /// </summary>
    /// <param name="device">A pointer to a device.</param>
    /// <param name="attributeList">A zero terminated array of a set of attributes: ALC_FREQUENCY, ALC_MONO_SOURCES, ALC_REFRESH, ALC_STEREO_SOURCES, ALC_SYNC.</param>
    /// <returns>
    ///     Returns a pointer to the new context (NULL on failure).
    /// </returns>
    /// <remarks>The attribute list can be NULL, or a zero terminated list of integer pairs composed of valid ALC attribute tokens and requested values.</remarks>
    public ALContext CreateContext([In] ALDevice device, [In] int[] attributeList) => this.alcCreateContext(device, attributeList);

    /// <summary>
    /// This function opens a device by name.
    /// </summary>
    /// <param name="devicename">A null-terminated string describing a device.</param>
    /// <returns>
    ///     Returns a pointer to the opened device. The return value will be NULL if there is an error.
    /// </returns>
    public ALDevice OpenDevice(string? devicename) => this.alcOpenDevice(devicename);

    /// <summary>
    /// This function makes a specified context the current context.
    /// </summary>
    /// <param name="context">A pointer to the new context.</param>
    /// <returns>
    ///     Returns True on success, or False on failure.
    /// </returns>
    public bool MakeContextCurrent(ALContext context) => this.alcMakeContextCurrent(context);

    /// <summary>
    /// This function retrieves a context's device pointer.
    /// </summary>
    /// <param name="context">A pointer to a context.</param>
    /// <returns>
    ///     Returns a pointer to the specified context's device.
    /// </returns>
    public ALDevice GetContextsDevice(ALContext context) => this.alcGetContextsDevice(context);

    /// <summary>
    ///     This strings related to the context.
    /// </summary>
    /// <remarks>
    ///     ALC_DEFAULT_DEVICE_SPECIFIER will return the name of the default output device.
    ///     ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER will return the name of the default capture device.
    ///     ALC_DEVICE_SPECIFIER will return the name of the specified output device if a pointer is supplied, or will return a list of all available devices if a NULL device pointer is supplied. A list is a pointer to a series of strings separated by NULL characters, with the list terminated by two NULL characters. See Enumeration Extension for more details.
    ///     ALC_CAPTURE_DEVICE_SPECIFIER will return the name of the specified capture device if a pointer is supplied, or will return a list of all available devices if a NULL device pointer is supplied.
    ///     ALC_EXTENSIONS returns a list of available context extensions, with each extension separated by a space and the list terminated by a NULL character.
    /// </remarks>
    /// <param name="device">A pointer to the device to be queried.</param>
    /// <param name="param">An attribute to be retrieved: ALC_DEFAULT_DEVICE_SPECIFIER, ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER, ALC_DEVICE_SPECIFIER, ALC_CAPTURE_DEVICE_SPECIFIER, ALC_EXTENSIONS.</param>
    /// <returns>
    ///     A string containing the name of the Device.
    /// </returns>
    public string GetString([In] ALDevice device, AlcGetString param) => this.alcGetString(device, param);

    /// <summary>
    ///     The strings related to the context.
    /// </summary>
    /// <remarks>
    ///     ALC_DEFAULT_DEVICE_SPECIFIER will return the name of the default output device.
    ///     ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER will return the name of the default capture device.
    ///     ALC_DEVICE_SPECIFIER will return the name of the specified output device if a pointer is supplied, or will return a list of all available devices if a NULL device pointer is supplied. A list is a pointer to a series of strings separated by NULL characters, with the list terminated by two NULL characters. See Enumeration Extension for more details.
    ///     ALC_CAPTURE_DEVICE_SPECIFIER will return the name of the specified capture device if a pointer is supplied, or will return a list of all available devices if a NULL device pointer is supplied.
    ///     ALC_EXTENSIONS returns a list of available context extensions, with each extension separated by a space and the list terminated by a NULL character.
    /// </remarks>
    /// <param name="device">A pointer to the device to be queried.</param>
    /// <param name="param">An attribute to be retrieved: ALC_DEFAULT_DEVICE_SPECIFIER, ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER, ALC_DEVICE_SPECIFIER, ALC_CAPTURE_DEVICE_SPECIFIER, ALC_EXTENSIONS.</param>
    /// <returns>
    ///     A string containing the name of the Device.
    /// </returns>
    public unsafe byte* GetStringPtr([In] ALDevice device, AlcGetString param) => this.alcGetStringPtr(device, param);

    /// <summary>
    ///     This function closes a device by name.
    /// </summary>
    /// <param name="device">A pointer to an opened device.</param>
    /// <returns>
    ///     True will be returned on success or False on failure. Closing a device will fail if the device contains any contexts or buffers.
    /// </returns>
    public bool CloseDevice([In] ALDevice device) => this.alcCloseDevice(device);

    /// <summary>
    /// This function destroys a context.
    /// </summary>
    /// <param name="context">A pointer to the new context.</param>
    public void DestroyContext(ALContext context) => this.alcDestroyContext(context);
}
