// <copyright file="Enums.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace CASL.NativeInterop;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents the different RTLD (Run-Time Linker Dynamic) modes when loading a native posix library on Ubuntu.
/// </summary>
/// <remarks>
/// <br/>
/// More info can be found at:
/// <para>
///     https://pubs.opengroup.org/onlinepubs/9699919799.
/// </para>
/// </remarks>
[Flags]
[SuppressMessage("csharpsquid", "S2342", Justification = "Used for interop and needs to match original documentation.")]
internal enum UbuntuRTLDMode
{
    /// <summary>
    /// Relocations shall be performed at an implementation-defined time, ranging from the time of the dlopen() call until the first reference
    /// to a given symbol occurs. Specifying RTLD_LAZY should improve performance on implementations supporting dynamic symbol binding since a
    /// process might not reference all of the symbols in an executable object file. And, for systems supporting dynamic symbol resolution for
    /// normal process execution, this behavior mimics the normal handling of process execution.
    /// </summary>
    RTLD_LAZY = 0x00001,

    /// <summary>
    /// All necessary relocations shall be performed when the executable object file is first loaded. This may waste some processing if
    /// relocations are performed for symbols that are never referenced. This behavior may be useful for applications that need to know that
    /// all symbols referenced during execution will be available before dlopen() returns.
    /// </summary>
    RTLD_NOW = 0x00002,

    /// <summary>
    /// The executable object file's symbols shall be made available for relocation processing of any other executable object file. In addition,
    /// symbol lookup using dlopen(NULL,mode) and an associated dlsym() allows executable object files loaded with this mode to be searched.
    /// </summary>
    RTLD_GLOBAL = 0x00100,

    /// <summary>
    /// The executable object file's symbols shall not be made available for relocation processing of any other executable object file.
    /// </summary>
    [SuppressMessage("csharpsquid", "S2346", Justification = "Used for interop and needs to match original documentation.")]
    RTLD_LOCAL = 0x00000,
}

/// <summary>
/// Represents the different RTLD (Run-Time Linker Dynamic) modes when loading a native posix library on Mac.
/// </summary>
/// <remarks>
/// <br/>
/// More info can be found at:
/// <para>
///     https://pubs.opengroup.org/onlinepubs/9699919799.
/// </para>
/// </remarks>
[Flags]
[SuppressMessage("csharpsquid", "S2342", Justification = "Used for interop and needs to match original documentation.")]
internal enum MacRTLDMode
{
    /// <summary>
    /// Relocations shall be performed at an implementation-defined time, ranging from the time of the dlopen() call until the first reference
    /// to a given symbol occurs. Specifying RTLD_LAZY should improve performance on implementations supporting dynamic symbol binding since a
    /// process might not reference all of the symbols in an executable object file. And, for systems supporting dynamic symbol resolution for
    /// normal process execution, this behavior mimics the normal handling of process execution.
    /// </summary>
    RTLD_LAZY = 0x1,

    /// <summary>
    /// All necessary relocations shall be performed when the executable object file is first loaded. This may waste some processing if
    /// relocations are performed for symbols that are never referenced. This behavior may be useful for applications that need to know that
    /// all symbols referenced during execution will be available before dlopen() returns.
    /// </summary>
    RTLD_NOW = 0x2,

    /// <summary>
    /// The executable object file's symbols shall be made available for relocation processing of any other executable object file. In addition,
    /// symbol lookup using dlopen(NULL,mode) and an associated dlsym() allows executable object files loaded with this mode to be searched.
    /// </summary>
    RTLD_GLOBAL = 0x8,

    /// <summary>
    /// The executable object file's symbols shall not be made available for relocation processing of any other executable object file.
    /// </summary>
    [SuppressMessage("csharpsquid", "S2346", Justification = "Used for interop and needs to match original documentation.")]
    RTLD_LOCAL = 0x0,
}
