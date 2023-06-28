// <copyright file="NativeMethods.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop;

using System.Runtime.InteropServices;

/// <summary>
/// Provides access to interop with native windows and posix platform related functions.
/// </summary>
internal static class NativeMethods
{
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
    [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
    public static extern nint LoadLibrary_WIN(string lpFileName);

    [DllImport("kernel32.dll", EntryPoint = "GetProcAddress", SetLastError = true, CharSet = CharSet.Ansi)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "Must be set to ANSI to properly get function pointer.")]
    public static extern nint GetProcAddress_WIN(nint hModule, string procname);

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
    [DllImport("libdl.so.2", EntryPoint = "dlopen", SetLastError = true, CharSet = CharSet.Ansi)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "Possibly will not work.  Testing needs to be done.")]
    public static extern nint dlopen_POSIX(string fileName, int flags);

    [DllImport("libdl.so.2", EntryPoint = "dlerror", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern nint dlerror_POSIX();

    [DllImport("libdl.so.2", EntryPoint = "dlsym", SetLastError = true, CharSet = CharSet.Ansi)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "Possibly will not work.  Testing needs to be done.")]
    public static extern nint dlsym_POSIX(nint handle, string symbol);
}
