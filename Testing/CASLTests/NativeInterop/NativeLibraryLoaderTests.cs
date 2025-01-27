// <copyright file="NativeLibraryLoaderTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.NativeInterop;

using System;
using System.IO;
using System.IO.Abstractions;
using CASL.DotnetWrappers;
using CASL.Exceptions;
using CASL.NativeInterop;
using FluentAssertions;
using Helpers;
using NSubstitute;
using Xunit;

/// <summary>
/// Tests the <see cref="NativeLibraryLoader"/> class.
/// </summary>
public class NativeLibraryLoaderTests
{
    private readonly IAssembly mockAssembly;
    private readonly IPlatform mockPlatform;
    private readonly IFile mockFile;
    private readonly IPath mockPath;
    private readonly ILibrary mockLibrary;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeLibraryLoaderTests"/> class.
    /// </summary>
    public NativeLibraryLoaderTests()
    {
        this.mockAssembly = Substitute.For<IAssembly>();
        this.mockPlatform = Substitute.For<IPlatform>();
        this.mockFile = Substitute.For<IFile>();

        this.mockPath = Substitute.For<IPath>();
        this.mockPath.DirectorySeparatorChar.Returns('\\');

        this.mockLibrary = Substitute.For<ILibrary>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullAssemblyParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => new NativeLibraryLoader(
            null,
            this.mockPlatform,
            this.mockFile,
            this.mockPath,
            this.mockLibrary);

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("assembly");
    }

    [Fact]
    public void Ctor_WithNullPlatformParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => new NativeLibraryLoader(
            this.mockAssembly,
            null,
            this.mockFile,
            this.mockPath,
            this.mockLibrary);

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("platform");
    }

    [Fact]
    public void Ctor_WithNullFileParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => new NativeLibraryLoader(
            this.mockAssembly,
            this.mockPlatform,
            null,
            this.mockPath,
            this.mockLibrary);

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("file");
    }

    [Fact]
    public void Ctor_WithNullPathParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => new NativeLibraryLoader(
            this.mockAssembly,
            this.mockPlatform,
            this.mockFile,
            null,
            this.mockLibrary);

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("path");
    }

    [Fact]
    public void Ctor_WithNullLibraryParam_ThrowsException()
    {
        // Arrange & Act
        var act = () => new NativeLibraryLoader(
            this.mockAssembly,
            this.mockPlatform,
            this.mockFile,
            this.mockPath,
            null);

        // Assert
        act.Should().ThrowArgNullException().WithNullParamMsg("library");
    }

    [Fact]
    public void Ctor_WhenInvoked_SetsLibraryProperty()
    {
        // Arrange & Act
        this.mockLibrary.GetLibraryName().Returns("test-library");
        var sut = CreateSystemUnderTest();

        // Assert
        sut.LibraryName.Should().Be("test-library");
    }
    #endregion

    #region Method Tests
    [Fact]
    public void LoadLibrary_IfLibraryDoesNotExist_ThrowsException()
    {
        // Arrange
        const string libDirPath = @"C:\lib-dir";
        const string expected = $"Could not find the library 'test-library' in the directory path '{libDirPath}'.";

        this.mockLibrary.GetLibraryName().Returns("test-library");
        this.mockAssembly.Location.Returns(@"C:\lib-dir");
        this.mockFile.Exists(Arg.Any<string>()).Returns(false);

        var sut = CreateSystemUnderTest();

        // Act
        var act = () => sut.LoadLibrary();

        // Assert
        act.Should().Throw<FileNotFoundException>().WithMessage(expected);
    }

    [Fact]
    public void LoadLibrary_WithZeroLibraryPointer_ThrowsException()
    {
        // Arrange
        const string expected = "test-system-error\n\nLibrary path: 'C:\\lib-dir\\test-library'";
        this.mockLibrary.GetLibraryName().Returns("test-library");
        this.mockAssembly.Location.Returns(@"C:\lib-dir");
        this.mockFile.Exists(Arg.Any<string>()).Returns(true);
        this.mockPlatform.LoadLibrary(Arg.Any<string>()).Returns(IntPtr.Zero);
        this.mockPlatform.GetLastSystemError().Returns("test-system-error");

        var sut = CreateSystemUnderTest();

        // Act
        var act = () => sut.LoadLibrary();

        // Assert
        act.Should().Throw<LoadLibraryException>().WithMessage(expected);
    }

    [Fact]
    public void LoadLibrary_WithValidLibraryPointer_ReturnsPointer()
    {
        // Arrange
        this.mockLibrary.GetLibraryName().Returns("test-library");
        this.mockAssembly.Location.Returns(@"C:\lib-dir");
        this.mockFile.Exists(Arg.Any<string>()).Returns(true);
        this.mockPlatform.LoadLibrary(Arg.Any<string>()).Returns(new IntPtr(123));

        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.LoadLibrary();

        // Assert
        actual.Should().Be(123);
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="NativeLibraryLoader"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private NativeLibraryLoader CreateSystemUnderTest()
        => new (
            this.mockAssembly,
            this.mockPlatform,
            this.mockFile,
            this.mockPath,
            this.mockLibrary);
}
