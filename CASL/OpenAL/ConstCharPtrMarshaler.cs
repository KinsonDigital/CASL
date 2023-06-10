// <copyright file="ConstCharPtrMarshaler.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.OpenAL;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

/// <summary>
/// Provides a wrapper for handling method calls.
/// </summary>
[ExcludeFromCodeCoverage]
internal class ConstCharPtrMarshaler : ICustomMarshaler
{
    private static readonly ConstCharPtrMarshaler Instance = new ();

    /// <summary>
    /// Returns the instance of the marshaler.
    /// </summary>
    /// <param name="cookie">Can be used by the custom marshaler.</param>
    /// <returns>An instance of the marshaler.</returns>
#pragma warning disable IDE0060 // Remove unused parameter
    public static ICustomMarshaler GetInstance(string cookie) => Instance;
#pragma warning restore IDE0060 // Remove unused parameter

    /// <inheritdoc/>
    public void CleanUpManagedData(object managedObj)
    {
        // Nothing to do.  Let garbage collection take care of this
    }

    /// <inheritdoc/>
    public void CleanUpNativeData(nint nativeData)
    {
        // Nothing to do.  Need to keep data around for use
    }

    /// <inheritdoc/>
    public int GetNativeDataSize()
    {
        unsafe
        {
            return sizeof(nint);
        }
    }

    /// <inheritdoc/>
    public nint MarshalManagedToNative(object managedObj)
        => managedObj switch
        {
            string str => Marshal.StringToHGlobalAnsi(str),
            _ => throw new ArgumentException($"{nameof(ConstCharPtrMarshaler)} only supports marshaling of strings. Got '{managedObj.GetType()}'"),
        };

    /// <inheritdoc/>
    public object MarshalNativeToManaged(nint nativeData) => Marshal.PtrToStringAnsi(nativeData) ?? string.Empty;
}
