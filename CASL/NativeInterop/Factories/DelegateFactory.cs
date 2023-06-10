// <copyright file="DelegateFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop.Factories;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

/// <summary>
/// Creates delegates to native library functions.
/// </summary>
[ExcludeFromCodeCoverage]
internal class DelegateFactory : IDelegateFactory
{
    private readonly IPlatform platform;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateFactory"/> class.
    /// </summary>
    /// <param name="platform">Provides platform specific information.</param>
    public DelegateFactory(IPlatform platform) => this.platform = platform;

    /// <inheritdoc/>
    public TDelegate CreateDelegate<TDelegate>(nint libraryPtr, string procName)
    {
        if (libraryPtr == 0)
        {
            throw new ArgumentException("The pointer must not be zero.", nameof(libraryPtr));
        }

        nint libFunctionPtr;

        if (this.platform.IsWinPlatform())
        {
            libFunctionPtr = NativeMethods.GetProcAddress_WIN(libraryPtr, procName);
        }
        else
        {
            libFunctionPtr = NativeMethods.dlsym_POSIX(libraryPtr, procName);
        }

        if (libFunctionPtr == 0)
        {
            throw new Exception($"The address for function '{procName}' could not be created.");
        }

        return Marshal.GetDelegateForFunctionPointer<TDelegate>(libFunctionPtr);
    }
}
