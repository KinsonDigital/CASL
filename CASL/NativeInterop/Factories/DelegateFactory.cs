// <copyright file="DelegateFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop.Factories
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Creates delegates to native library functions.
    /// </summary>
    public class DelegateFactory : IDelegateFactory
    {
        /// <inheritdoc/>
        public TDelegate CreateDelegate<TDelegate>(nint libraryPtr, string procName)
        {
            if (libraryPtr == 0)
            {
                throw new ArgumentException("The pointer must not be zero.", nameof(libraryPtr));
            }

            var libFunctionPtr = NativeMethods.GetProcAddress_WIN(libraryPtr, procName);

            if (libFunctionPtr == 0)
            {
                throw new Exception($"The address for function '{procName}' could not be created.");
            }

            return Marshal.GetDelegateForFunctionPointer<TDelegate>(libFunctionPtr);
        }
    }
}
