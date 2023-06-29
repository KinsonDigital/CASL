// <copyright file="Platform.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASL.NativeInterop;

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
    public string CurrentOSPlatform
        => $"{RuntimeInformation.OSDescription} - {GetProcessArchitecture().ToString().ToLower()}";

    /// <inheritdoc/>
    public bool IsWinPlatform() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <inheritdoc/>
    public bool IsWin10Platform() => IsWinPlatform() && Environment.OSVersion.Version.Major == 10;

    /// <inheritdoc/>
    public bool IsMacOSXPlatform() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    /// <inheritdoc/>
    public bool IsLinuxPlatform() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <inheritdoc/>
    public bool IsPosixPlatform() => IsUnixPlatform() || IsMacOSXPlatform() ||
                                     (int)Environment.OSVersion.Platform == 128;

    /// <inheritdoc/>
    public bool IsUnixPlatform() => Environment.OSVersion.Platform == PlatformID.Unix;

    /// <inheritdoc/>
    public bool Is32BitProcess() => !Is64BitProcess();

    /// <inheritdoc/>
    public bool Is64BitProcess() => Environment.Is64BitProcess;

    /// <inheritdoc/>
    public bool Is32BitOS()
    {
        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X86:
            case Architecture.Arm:
                return true;
            case Architecture.X64:
            case Architecture.Arm64:
                return false;
            default:
                throw new Exception("Do not know if OS is 32 bit.");
        }
    }

    /// <inheritdoc/>
    public bool Is64BitOS()
    {
        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X86:
            case Architecture.Arm:
                return false;
            case Architecture.X64:
            case Architecture.Arm64:
                return true;
            default:
                throw new Exception("Do not know if OS is 64 bit.");
        }
    }

    /// <inheritdoc/>
    public Architecture GetProcessArchitecture() => RuntimeInformation.ProcessArchitecture;

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
        try
        {
            if (IsWinPlatform())
            {
                return NativeMethods.LoadLibrary_WIN(libPath);
            }
            else if (IsPosixPlatform())
            {
                return NativeMethods.dlopen_POSIX(libPath, UbuntuRTLDMode.RTLD_NOW);
            }
            else
            {
                var ex = new Exception("Platform must be a windows or posix platform.")
                {
                    HResult = 9753,
                };

                throw ex;
            }
        }
        catch (Exception ex)
        {
            if (ex.HResult != 9753)
            {
                var systemErrorMsg = GetLastSystemError();

                throw new Exception(systemErrorMsg);
            }

            throw;
        }
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
            return NativeMethods.dlerror_POSIX().ToManagedUtf8String();
        }
        else
        {
            return string.Empty;
        }
    }
}
