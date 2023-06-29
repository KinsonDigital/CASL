// <copyright file="NativeMethods.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop;

using System.Runtime.InteropServices;

/// <summary>
/// Provides access to interop with native windows and posix platform related functions.
/// </summary>
internal static partial class NativeMethods
{
    private const string WinLibName = "kernel32.dll";
    private const string PosixLibName = "libdl.so.2";

    /// <summary>
    /// Loads the specified module into the address space of the calling process. The specified module may cause other modules to be loaded.
    /// </summary>
    /// <param name="lpFileName">
    ///     The name of the module. This can be either a library module (a .dll file) or an executable module (an .exe file).
    ///     The name specified is the file name of the module and is not related to the name stored in the library module itself,
    ///     as specified by the LIBRARY keyword in the module-definition (.def) file.
    ///     <para>
    ///         If the string specifies a full path, the function searches only that path for the module.
    ///     </para>
    ///     <para>
    ///         If the string specifies a relative path or a module name without a path, the function uses a standard search strategy
    ///         to find the module; for more information, see the Remarks.
    ///     </para>
    ///     <para>
    ///         If the function cannot find the module, the function fails. When specifying a path, be sure to use backslashes (\),
    ///         not forward slashes (/). For more information about paths, see Naming a File or Directory.
    ///     </para>
    ///     <para>
    ///         If the string specifies a module name without a path and the file name extension is omitted, the function appends the
    ///         default library extension .dll to the module name. To prevent the function from appending .dll to the module name,
    ///         include a trailing point character (.) in the module name string.
    ///     </para>
    /// </param>
    /// <returns>
    ///     If the function succeeds, the return value is a handle to the module.
    ///     If the function fails, the return value is a null pointer.
    /// </returns>
    [LibraryImport(WinLibName, EntryPoint = "LoadLibraryW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint LoadLibrary_WIN(string lpFileName);

    /// <summary>
    /// Retrieves the address of an exported function (also known as a procedure) or variable from the specified dynamic-link library (DLL).
    /// </summary>
    /// <param name="hModule">
    /// A handle to the DLL module that contains the function or variable. The <see cref="LoadLibrary_WIN"/>, LoadLibraryEx, LoadPackagedLibrary,
    /// or GetModuleHandle function returns this handle.
    /// </param>
    /// <param name="procname">
    /// The function or variable name, or the function's ordinal value. If this parameter is an ordinal value, it must be in the low-order word;
    /// the high-order word must be zero.
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is the address of the exported function or variable.
    /// If the function fails, the return value is NULL.
    /// <br/>
    /// <br/>
    /// To get extended error information, go here:
    /// <para>
    ///     https://learn.microsoft.com/en-us/windows/win32/api/errhandlingapi/nf-errhandlingapi-getlasterror
    /// </para>
    /// </returns>
    /// <remarks>
    /// <br/>
    /// <br/>
    /// More information can be found at:
    /// <para>
    ///     https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-getprocaddress
    /// </para>
    /// </remarks>
    [LibraryImport(WinLibName, EntryPoint = "GetProcAddress", SetLastError = true, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller))]
    public static partial nint GetProcAddress_WIN(nint hModule, string procname);

    /*Useful Links:
     * https://linux.die.net/man/3/dlopen
     * https://linux.die.net/man/3/dlerror
     * https://linux.die.net/man/3/dlsym
    */

    /*NOTE:
     * The dll calls below all use Ansi.  This Displays the warning CA2101.  This warning has to do with a security vulnerability
     * dealing with characters chosen for the entry point strings on different platforms.  No testing has been done to see if these
     * interop functions work using the CharSet.Unicode character set and this needs to be done.
     *
     * Refer to link for more info: https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2101?view=vs-2019
     */

    /// <summary>
    /// Loads a shared library at runtime, allowing access to code and data defined in the library.
    /// </summary>
    /// <param name="fileName">The filename or path of the shared library to load.</param>
    /// <param name="flags">Flags specifying the behavior of the loading process.</param>
    /// <returns>A handle to the loaded library, or null if an error occurs.</returns>
    /// <remarks>
    /// The library can be specified by its filename or by using a system search algorithm.
    /// Use the dlsym function to retrieve the address of symbols (functions or variables) defined in the library.
    /// Call dlclose to unload the library and free associated resources.
    /// Use dlerror to retrieve error messages if any errors occur during the loading process.
    /// <br/>
    /// <br/>
    /// More information can be found at:
    /// <para>
    ///     https://pubs.opengroup.org/onlinepubs/9699919799/functions/dlopen.html
    /// </para>
    /// </remarks>
    [LibraryImport(PosixLibName, EntryPoint = "dlopen", SetLastError = true, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller))]
    public static partial nint dlopen_POSIX(string fileName, UbuntuRTLDMode flags);

    /// <summary>
    /// Retrieves the most recent error message that occurred during dynamic loading operations.
    /// </summary>
    /// <returns>A string describing the error, or null if no error occurred.</returns>
    /// <remarks>
    /// <br/>
    /// <br/>
    /// More information can be found at:
    /// <para>
    ///     https://pubs.opengroup.org/onlinepubs/9699919799/functions/dlerror.html
    /// </para>
    /// </remarks>
    [LibraryImport(PosixLibName, EntryPoint = "dlerror", SetLastError = true)]
    public static partial nint dlerror_POSIX();

    /// <summary>
    /// Retrieves the address of a symbol by its name from a loaded library.
    /// </summary>
    /// <param name="handle">The handle to the loaded library.</param>
    /// <param name="symbol">The name of the symbol to retrieve.</param>
    /// <returns>The address of the symbol, or null if the symbol is not found.</returns>
    /// <remarks>
    /// Use the dlopen function to load a library before calling this function.
    /// Symbols can include functions, variables, or special symbols defined in the library.
    /// <br/>
    /// <br/>
    /// More information can be found at:
    /// <para>
    ///     https://pubs.opengroup.org/onlinepubs/9699919799/functions/dlsym.html
    /// </para>
    /// </remarks>
    [LibraryImport(PosixLibName, EntryPoint = "dlsym", SetLastError = true, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller))]
    public static partial nint dlsym_POSIX(nint handle, string symbol);
}
