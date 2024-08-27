// <copyright file="NativeLibPathResolverTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.NativeInterop;

using System.IO.Abstractions;
using System.Runtime.InteropServices;
using CASL;
using CASL.NativeInterop;
using Xunit;
using FluentAssertions;
using NSubstitute;

/// <summary>
/// Tests the <see cref="NativeLibPathResolver"/> class.
/// </summary>
public class NativeLibPathResolverTests
{
    private const string WinDirPath = @"C:\Program Files\test-app";
    private const string LinuxDirPath = "/user/bin/test-app";
    private const string MacOSXDirPath = "/Applications/test-app";
    private const string WinExtension = ".dll";
    private const string PosixExtension = ".so"; //macOSX and Linux systems
    private const char PosixSeparatorChar = '/'; //macOSX and Linux systems
    private readonly IPlatform mockPlatform;
    private readonly IApplication mockApp;
    private readonly IPath mockPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeLibPathResolverTests"/> class.
    /// </summary>
    public NativeLibPathResolverTests()
    {
        this.mockPlatform = Substitute.For<IPlatform>();
        this.mockPath = Substitute.For<IPath>();
        this.mockApp = Substitute.For<IApplication>();
    }

    #region Method Tests
    [Theory]
    [InlineData(WinExtension, true, Architecture.X64, false, "win-x64")]
    [InlineData("", false, Architecture.X64, false, "win-x64")]
    [InlineData(WinExtension, true, Architecture.X86, false, "win-x86")]
    public void GetPath_WhenWindows_ReturnsCorrectPath(
        string extension,
        bool hasExtension,
        Architecture arch,
        bool isWin10,
        string platform)
    {
        // Arrange
        var libName = $"test-lib{extension}";
        var expected = @$"C:/Program Files/test-app/runtimes/{platform}/native/test-lib{extension}";

        MockWindowsPlatform();
        this.mockPlatform.IsWin10Platform().Returns(isWin10);
        this.mockPlatform.GetProcessArchitecture().Returns(arch);
        this.mockPlatform.GetPlatformLibFileExtension().Returns(extension);

        this.mockPath.GetDirectoryName(Arg.Any<string>()).Returns(WinDirPath);
        this.mockPath.HasExtension(Arg.Any<string>()).Returns(hasExtension);
        this.mockPath.GetFileNameWithoutExtension(libName).Returns(libName.Split('.')[0]);

        var resolver = CreateResolver();

        // Act
        var actual = resolver.GetFilePath(libName);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(PosixExtension, PosixSeparatorChar, true, Architecture.X86, "osx")]
    [InlineData(PosixExtension, PosixSeparatorChar, false, Architecture.X64, "osx-x64")]
    public void GetPath_WhenMacOSX_ReturnsCorrectPath(
        string extension,
        char separatorChar,
        bool is32BitProcess,
        Architecture arch,
        string platform)
    {
        // Arrange
        var libName = $"test-lib{extension}";
        var expected = $@"{LinuxDirPath}{separatorChar}runtimes{separatorChar}{platform}{separatorChar}native{separatorChar}test-lib{extension}";

        MockMacOSXPlatform();
        this.mockPlatform.Is32BitProcess().Returns(is32BitProcess);
        this.mockPlatform.GetProcessArchitecture().Returns(arch);
        this.mockPlatform.GetPlatformLibFileExtension().Returns(extension);

        this.mockPath.DirectorySeparatorChar.Returns(separatorChar);
        this.mockPath.GetDirectoryName(Arg.Any<string>()).Returns(LinuxDirPath);
        this.mockPath.HasExtension(libName).Returns(true);
        this.mockPath.GetFileNameWithoutExtension(libName).Returns(libName.Split('.')[0]);

        var resolver = CreateResolver();

        // Act
        var actual = resolver.GetFilePath(libName);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(PosixExtension, PosixSeparatorChar, false, Architecture.X64, "linux-x64")]
    public void GetPath_WhenLinuxOS_ReturnsCorrectPath(
        string extension,
        char separatorChar,
        bool is32BitProcess,
        Architecture arch,
        string platform)
    {
        // Arrange
        var libName = $"test-lib{extension}";
        var expected = $@"{MacOSXDirPath}{separatorChar}runtimes{separatorChar}{platform}{separatorChar}native{separatorChar}test-lib{extension}";

        MockLinuxPlatform();
        this.mockPlatform.Is32BitProcess().Returns(is32BitProcess);
        this.mockPlatform.GetProcessArchitecture().Returns(arch);
        this.mockPlatform.GetPlatformLibFileExtension().Returns(extension);

        this.mockPath.DirectorySeparatorChar.Returns(separatorChar);
        this.mockPath.GetDirectoryName(Arg.Any<string>()).Returns(MacOSXDirPath);
        this.mockPath.HasExtension(libName).Returns(true);
        this.mockPath.GetFileNameWithoutExtension(libName).Returns(libName.Split('.')[0]);

        var resolver = CreateResolver();

        // Act
        var actual = resolver.GetFilePath(libName);

        // Assert
        actual.Should().Be(expected);
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="NativeLibPathResolver"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private NativeLibPathResolver CreateResolver() => new (this.mockPlatform, this.mockPath, this.mockApp);

    /// <summary>
    /// Mocks the platform to be Windows.
    /// </summary>
    private void MockWindowsPlatform()
    {
        this.mockPlatform.IsWinPlatform().Returns(true);
        this.mockPlatform.IsMacOSXPlatform().Returns(false);
        this.mockPlatform.IsLinuxPlatform().Returns(false);
    }

    /// <summary>
    /// Mocks the platform to be Linux.
    /// </summary>
    private void MockLinuxPlatform()
    {
        this.mockPlatform.IsWinPlatform().Returns(false);
        this.mockPlatform.IsMacOSXPlatform().Returns(false);
        this.mockPlatform.IsLinuxPlatform().Returns(true);
    }

    /// <summary>
    /// Mocks the platform to be macOSX.
    /// </summary>
    private void MockMacOSXPlatform()
    {
        this.mockPlatform.IsWinPlatform().Returns(false);
        this.mockPlatform.IsMacOSXPlatform().Returns(true);
        this.mockPlatform.IsLinuxPlatform().Returns(false);
    }
}
