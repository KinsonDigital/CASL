// <copyright file="OpenALLibraryTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable ConvertToLocalFunction
namespace CASLTests.NativeInterop;

using System;
using System.IO;
using System.IO.Abstractions;
using CASL.DotnetWrappers;
using CASL.Exceptions;
using CASL.NativeInterop;
using Xunit;
using Shouldly;
using Helpers;
using NSubstitute;

/// <summary>
/// Tests the <see cref="OpenALLibrary"/> class.
/// </summary>
public class OpenALLibraryTests
{
    private readonly IPlatform mockPlatform;
    private readonly IDirectory mockDirectory;
    private readonly IFile mockFile;
    private readonly IPath mockPath;
    private readonly IAssembly mockAssembly;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenALLibraryTests"/> class.
    /// </summary>
    public OpenALLibraryTests()
    {
        this.mockPlatform = Substitute.For<IPlatform>();
        this.mockDirectory = Substitute.For<IDirectory>();
        this.mockFile = Substitute.For<IFile>();
        this.mockPath = Substitute.For<IPath>();
        this.mockAssembly = Substitute.For<IAssembly>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullPlatformParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => new OpenALLibrary(
            null!,
            this.mockDirectory,
            this.mockFile,
            this.mockPath,
            this.mockAssembly);

        // Assert
        Should.Throw<ArgumentNullException>(act).WithNullParamMsg("platform");
    }

    [Fact]
    public void Ctor_WithNullDirectoryParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => new OpenALLibrary(
            this.mockPlatform,
            null!,
            this.mockFile,
            this.mockPath,
            this.mockAssembly);

        // Assert
        Should.Throw<ArgumentNullException>(act).WithNullParamMsg("directory");
    }

    [Fact]
    public void Ctor_WithNullFileParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => new OpenALLibrary(
            this.mockPlatform,
            this.mockDirectory,
            null!,
            this.mockPath,
            this.mockAssembly);

        // Assert
        Should.Throw<ArgumentNullException>(act).WithNullParamMsg("file");
    }

    [Fact]
    public void Ctor_WithNullPathParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => new OpenALLibrary(
            this.mockPlatform,
            this.mockDirectory,
            this.mockFile,
            null!,
            this.mockAssembly);

        // Assert
        Should.Throw<ArgumentNullException>(act).WithNullParamMsg("path");
    }

    [Fact]
    public void Ctor_WithNullAssemblyParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => new OpenALLibrary(
            this.mockPlatform,
            this.mockDirectory,
            this.mockFile,
            this.mockPath,
            null!);

        // Assert
        Should.Throw<ArgumentNullException>(act).WithNullParamMsg("assembly");
    }

    [Fact]
    public void Ctor_WithUnknownPlatform_ThrowsException()
    {
        // Arrange
        this.mockPlatform.IsWinPlatform().Returns(false);
        this.mockPlatform.IsPosixPlatform().Returns(false);
        this.mockPlatform.CurrentOSPlatform.Returns("xyz");

        // Act
        var act = CreateSystemUnderTest;

        // Assert
        Should.Throw<UnknownPlatformException>(act)
            .Message.ShouldBe("The platform 'xyz' is unknown or not supported.");
    }

    [Fact]
    public void Ctor_WhenPlatformDirPathDoesNotExist_ThrowsException()
    {
        // Arrange
        MockWindowsPlatform();
        this.mockAssembly.Location.Returns(@"C:\app-dir");
        this.mockFile.Exists(Arg.Any<string>()).Returns(false);
        this.mockDirectory.Exists(Arg.Any<string>()).Returns(false);

        // Act
        var act = CreateSystemUnderTest;

        // Assert
        Should.Throw<DirectoryNotFoundException>(act)
            .Message.ShouldBe(@"The directory 'C:\app-dir\runtimes\win-x64\native' does not exist.");
    }

    [Fact]
    public void Ctor_WhenFullPlatformLibFilePathDoesNotExist_ThrowsException()
    {
        // Arrange
        const string expected = @"The library 'soft_oal.dll' does not exist" +
                                @" in the platform directory 'C:\app-dir\runtimes\win-x64\native'.";
        MockWindowsPlatform();
        this.mockAssembly.Location.Returns(@"C:\app-dir");
        this.mockFile.Exists(Arg.Any<string>()).Returns(false);
        this.mockDirectory.Exists(Arg.Any<string>()).Returns(true);

        // Act
        var act = CreateSystemUnderTest;

        // Assert
        Should.Throw<FileNotFoundException>(act)
            .Message.ShouldBe(expected);
    }

    [Fact]
    public void Ctor_WithWindowsPlatform_ProcessesLibFile()
    {
        // Arrange
        MockWindowsPlatform();
        this.mockFile.Exists(@"C:\app-dir\soft_oal.dll").Returns(false);
        this.mockDirectory.Exists(Arg.Any<string>()).Returns(true);
        this.mockFile.Exists(@"C:\app-dir\runtimes\win-x64\native\soft_oal.dll").Returns(true);

        // Act
        CreateSystemUnderTest();

        // Assert
        this.mockFile.Received(1)
            .Copy(@"C:\app-dir\runtimes\win-x64\native\soft_oal.dll", @"C:\app-dir\soft_oal.dll");
    }

    [Fact]
    public void Ctor_WithLinuxPlatforms_ProcessesLibFile()
    {
        // Arrange
        MockLinuxPlatform();
        this.mockFile.Exists("/app-dir/libopenal.so.1.24.2").Returns(false);
        this.mockDirectory.Exists(Arg.Any<string>()).Returns(true);
        this.mockFile.Exists("/app-dir/runtimes/linux-x64/native/libopenal.so.1.24.2").Returns(true);

        // Act
        CreateSystemUnderTest();

        // Assert
        this.mockFile.Received(1)
            .Copy("/app-dir/runtimes/linux-x64/native/libopenal.so.1.24.2", "/app-dir/libopenal.so.1.24.2");
    }
    #endregion

    #region Method Tests
    [Fact]
    public void GetLibraryName_WhenGettingValueWithWinPlatform_ReturnsCorrectResult()
    {
        // Arrange
        MockWindowsPlatform();
        this.mockFile.Exists(Arg.Any<string>()).Returns(true);
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.GetLibraryName();

        // Assert
        actual.ShouldBe("soft_oal.dll");
    }

    [Fact]
    public void GetLibraryName_WhenGettingValueWithMacOSPlatform_ReturnsCorrectResult()
    {
        // Arrange
        MockMacOSPlatform();
        this.mockFile.Exists(Arg.Any<string>()).Returns(true);
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.GetLibraryName();

        // Assert
        actual.ShouldBe("OpenAL");
    }

    [Fact]
    public void GetLibraryName_WhenGettingValueWithPosixPlatform_ReturnsCorrectResult()
    {
        // Arrange
        MockLinuxPlatform();
        this.mockFile.Exists(Arg.Any<string>()).Returns(true);
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.GetLibraryName();

        // Assert
        actual.ShouldBe("libopenal.so.1.24.2");
    }

    [Fact]
    public void GetLibraryPath_WhenGettingValueWithMacosPlatform_ReturnsCorrectResult()
    {
        // Arrange
        MockMacOSPlatform();
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.GetLibraryPath();

        // Assert
        actual.ShouldBe("/System/Library/Frameworks/OpenAL.framework/OpenAL");
    }

    [Fact]
    public void GetLibraryPath_WhenGettingValueWithNonMacosPlatform_ReturnsCorrectResult()
    {
        // Arrange
        MockWindowsPlatform();
        this.mockDirectory.Exists(Arg.Any<string>()).Returns(true);
        this.mockFile.Exists(Arg.Any<string>()).Returns(true);
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.GetLibraryPath();

        // Assert
        actual.ShouldBe(@"C:\app-dir\soft_oal.dll");
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="OpenALLibrary"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private OpenALLibrary CreateSystemUnderTest()
        => new (this.mockPlatform,
            this.mockDirectory,
            this.mockFile,
            this.mockPath,
            this.mockAssembly);

    /// <summary>
    /// Mocks a windows platform.
    /// </summary>
    private void MockWindowsPlatform()
    {
        this.mockPlatform.IsWinPlatform().Returns(true);
        this.mockPlatform.IsPosixPlatform().Returns(false);
        this.mockPlatform.IsLinuxPlatform().Returns(false);
        this.mockPath.DirectorySeparatorChar.Returns('\\');
        this.mockAssembly.Location.Returns(@"C:\app-dir");
    }

    /// <summary>
    /// Mocks a linux platform.
    /// </summary>
    private void MockLinuxPlatform()
    {
        this.mockPlatform.IsWinPlatform().Returns(false);
        this.mockPlatform.IsPosixPlatform().Returns(true);
        this.mockPlatform.IsLinuxPlatform().Returns(true);
        this.mockPath.DirectorySeparatorChar.Returns('/');
        this.mockAssembly.Location.Returns("/app-dir");
    }

    /// <summary>
    /// Mocks the macOS platform.
    /// </summary>
    private void MockMacOSPlatform()
    {
        this.mockPlatform.IsWinPlatform().Returns(false);
        this.mockPlatform.IsPosixPlatform().Returns(true);
        this.mockPlatform.IsLinuxPlatform().Returns(false);
        this.mockPlatform.IsMacOSXPlatform().Returns(true);
        this.mockPath.DirectorySeparatorChar.Returns('/');
        this.mockAssembly.Location.Returns("/app-dir");
    }
}
