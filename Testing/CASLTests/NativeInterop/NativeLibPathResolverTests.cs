// <copyright file="NativeLibPathResolverTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.NativeInterop;

using System.IO.Abstractions;
using System.Runtime.InteropServices;
using CASL;
using CASL.NativeInterop;
using Moq;
using Xunit;
using FluentAssertions;

/// <summary>
/// Tests the <see cref="NativeLibPathResolver"/> class.
/// </summary>
public class NativeLibPathResolverTests
{
    private const string WinDirPath = @"C:\Program Files\test-app";
    private const string LinuxDirPath = "/user/bin/test-app";
    private const string MacOSXDirPath = "/Applications/test-app";
    private const string WinExtension = ".dll";
    private const string PosixExtension = ".so"; //MacOSX and Linux systems
    private const char PosixSeparatorChar = '/'; //MacOSX and Linux systems
    private readonly Mock<IPlatform> mockPlatform;
    private readonly Mock<IApplication> mockApp;
    private readonly Mock<IPath> mockPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeLibPathResolverTests"/> class.
    /// </summary>
    public NativeLibPathResolverTests()
    {
        this.mockPlatform = new Mock<IPlatform>();
        this.mockPath = new Mock<IPath>();
        this.mockApp = new Mock<IApplication>();
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
        this.mockPlatform.Setup(m => m.IsWin10Platform()).Returns(isWin10);
        this.mockPlatform.Setup(m => m.GetProcessArchitecture()).Returns(arch);
        this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(extension);

        this.mockPath.Setup(m => m.GetDirectoryName(It.IsAny<string>())).Returns(WinDirPath);
        this.mockPath.Setup(m => m.HasExtension(It.IsAny<string>())).Returns(hasExtension);
        this.mockPath.Setup(m => m.GetFileNameWithoutExtension(libName)).Returns(libName.Split('.')[0]);

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
        this.mockPlatform.Setup(m => m.Is32BitProcess()).Returns(is32BitProcess);
        this.mockPlatform.Setup(m => m.GetProcessArchitecture()).Returns(arch);
        this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(extension);

        this.mockPath.SetupGet(p => p.DirectorySeparatorChar).Returns(separatorChar);
        this.mockPath.Setup(m => m.GetDirectoryName(It.IsAny<string>())).Returns(LinuxDirPath);
        this.mockPath.Setup(m => m.HasExtension(libName)).Returns(true);
        this.mockPath.Setup(m => m.GetFileNameWithoutExtension(libName)).Returns(libName.Split('.')[0]);

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
        this.mockPlatform.Setup(m => m.Is32BitProcess()).Returns(is32BitProcess);
        this.mockPlatform.Setup(m => m.GetProcessArchitecture()).Returns(arch);
        this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(extension);

        this.mockPath.SetupGet(p => p.DirectorySeparatorChar).Returns(separatorChar);
        this.mockPath.Setup(m => m.GetDirectoryName(It.IsAny<string>())).Returns(MacOSXDirPath);
        this.mockPath.Setup(m => m.HasExtension(libName)).Returns(true);
        this.mockPath.Setup(m => m.GetFileNameWithoutExtension(libName)).Returns(libName.Split('.')[0]);

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
    private NativeLibPathResolver CreateResolver()
        => new NativeLibPathResolver(this.mockPlatform.Object, this.mockPath.Object, this.mockApp.Object);

    /// <summary>
    /// Mocks the platform to be Windows.
    /// </summary>
    private void MockWindowsPlatform()
    {
        this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(true);
        this.mockPlatform.Setup(m => m.IsMacOSXPlatform()).Returns(false);
        this.mockPlatform.Setup(m => m.IsLinuxPlatform()).Returns(false);
    }

    /// <summary>
    /// Mocks the platform to be Linux.
    /// </summary>
    private void MockLinuxPlatform()
    {
        this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(false);
        this.mockPlatform.Setup(m => m.IsMacOSXPlatform()).Returns(false);
        this.mockPlatform.Setup(m => m.IsLinuxPlatform()).Returns(true);
    }

    /// <summary>
    /// Mocks the platform to be MacOSX.
    /// </summary>
    private void MockMacOSXPlatform()
    {
        this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(false);
        this.mockPlatform.Setup(m => m.IsMacOSXPlatform()).Returns(true);
        this.mockPlatform.Setup(m => m.IsLinuxPlatform()).Returns(false);
    }
}
