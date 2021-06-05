// <copyright file="Platform.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Provides information about the current platform.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class Platform : IPlatform
    {
        /// <inheritdoc/>
        public bool IsWinPlatform() => Environment.OSVersion.Platform == PlatformID.Win32NT;

        /// <inheritdoc/>
        public bool IsPosixPlatform() => Environment.OSVersion.Platform == PlatformID.Unix ||
                   Environment.OSVersion.Platform == PlatformID.MacOSX ||
                   (int)Environment.OSVersion.Platform == 128;

        /// <inheritdoc/>
        public bool Is32BitProcess() => !Is64BitProcess();

        /// <inheritdoc/>
        public bool Is64BitProcess() => Environment.Is64BitProcess;

        /// <inheritdoc/>
        public string GetPlatformLibFileExtension()
        {
            if (IsWinPlatform())
            {
                return ".dll";
            }
            else if (IsPosixPlatform())
            {
                return ".so";
            }
            else
            {
                return string.Empty;
            }
        }

        /// <inheritdoc/>
        public IntPtr LoadLibrary(string libPath)
        {
            const int RTLD_NOW = 2;

            return IsWinPlatform() ? NativeMethods.LoadLibrary_WIN(libPath) : NativeMethods.dlopen_POSIX(libPath, RTLD_NOW);
        }

        /// <inheritdoc/>
        public string GetLastSystemError()
        {
            if (IsWinPlatform())
            {
                var errorCode = Marshal.GetLastWin32Error();
                var errorMsg = errorCode == 126 ? "The specified module could not be found." : errorCode.ToString(CultureInfo.InvariantCulture);

                return $"Error Code: {errorMsg}\n\nError Codes: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-";
            }
            else if (IsPosixPlatform())
            {
                return NativeMethods.dlerror_POSIX().ToManagedUTF8String();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
