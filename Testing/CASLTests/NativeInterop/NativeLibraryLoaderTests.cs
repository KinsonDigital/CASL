// <copyright file="NativeLibraryLoaderTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

#pragma warning disable IDE0002 // Name can be simplified
namespace CASLTests.NativeInterop;

#pragma warning disable IDE0001 // Name can be simplified
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using CASL.Exceptions;
using CASL.NativeInterop;
using FluentAssertions;
using Moq;
using Xunit;
#pragma warning restore IDE0001 // Name can be simplified

/// <summary>
/// Provides tests for the <see cref="NativeLibraryLoader"/> class.
/// </summary>
public class NativeLibraryLoaderTests
{
    private const string WinDirPath = @"C:\Program Files\test-app";
    private const string CrossPlatWinDirPath = @"C:/Program Files/test-app";
    private const string LinuxDirPath = "/user/bin/test-app";
    private const string WinExtension = ".dll";
    private const string PosixExtenstion = ".so";
    private const string LibNameWithoutExt = "test-lib";
    private const string WinLibNameWithExt = LibNameWithoutExt + WinExtension;
    private const string PosixLibNameWithExt = LibNameWithoutExt + PosixExtenstion;
    private const char PosixSeparatorChar = '/'; //MacOSX and Linux systems
    private readonly Mock<IDependencyManager> mockDependencyManager;
    private readonly Mock<IPlatform> mockPlatform;
    private readonly Mock<IDirectory> mockDirectory;
    private readonly Mock<IFile> mockFile;
    private readonly Mock<IPath> mockPath;
    private readonly Mock<ILibrary> mockLibrary;
    private string? libPath;
    private ReadOnlyCollection<string>? libDirPaths;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeLibraryLoaderTests"/> class.
    /// </summary>
    public NativeLibraryLoaderTests()
    {
        this.mockDependencyManager = new Mock<IDependencyManager>();
        this.mockPlatform = new Mock<IPlatform>();
        this.mockDirectory = new Mock<IDirectory>();
        this.mockFile = new Mock<IFile>();
        this.mockPath = new Mock<IPath>();
        this.mockLibrary = new Mock<ILibrary>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullDependencyManager_ThrowsException()
    {
        // Act
        var act = () => new NativeLibraryLoader(
                null,
                this.mockPlatform.Object,
                this.mockDirectory.Object,
                this.mockFile.Object,
                this.mockPath.Object,
                this.mockLibrary.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'dependencyManager')");
    }

    [Fact]
    public void Ctor_WithNullPlatform_ThrowsException()
    {
        // Act
        var act = () => new NativeLibraryLoader(
                this.mockDependencyManager.Object,
                null,
                this.mockDirectory.Object,
                this.mockFile.Object,
                this.mockPath.Object,
                this.mockLibrary.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'platform')");
    }

    [Fact]
    public void Ctor_WithNullDirectoryObject_ThrowsException()
    {
        // Act
        var act = () => new NativeLibraryLoader(
                this.mockDependencyManager.Object,
                this.mockPlatform.Object,
                null,
                this.mockFile.Object,
                this.mockPath.Object,
                this.mockLibrary.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'directory')");
    }

    [Fact]
    public void Ctor_WithNullFileObject_ThrowsException()
    {
        // Act
        var act = () => new NativeLibraryLoader(
                this.mockDependencyManager.Object,
                this.mockPlatform.Object,
                this.mockDirectory.Object,
                null,
                this.mockPath.Object,
                this.mockLibrary.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'file')");
    }

    [Fact]
    public void Ctor_WithNullPath_ThrowsException()
    {
        // Act
        var act = () => new NativeLibraryLoader(
                this.mockDependencyManager.Object,
                this.mockPlatform.Object,
                this.mockDirectory.Object,
                this.mockFile.Object,
                null,
                this.mockLibrary.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'path')");
    }

    [Fact]
    public void Ctor_WithNullLibrary_ThrowsException()
    {
        // Act
        var act = () => new NativeLibraryLoader(
                this.mockDependencyManager.Object,
                this.mockPlatform.Object,
                this.mockDirectory.Object,
                this.mockFile.Object,
                this.mockPath.Object,
                null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'library')");
    }

    [Fact]
    public void Ctor_WhenLibraryDoesNotExist_ThrowsException()
    {
        // Arrange
        MockPlatformAsWindows();
        this.mockLibrary.SetupGet(p => p.LibraryName).Returns(string.Empty);

        // Act
        var act = CreateLoader;

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null or empty. (Parameter 'libraryName')");
    }

    [Theory]
    [InlineData(LibNameWithoutExt + ".txt", WinExtension)]
    public void Ctor_WhenUsingLibraryNameWithIncorrectLibraryExtension_FixesExtension(
        string libName,
        string extension)
    {
        //Arrange
        this.mockLibrary.SetupGet(p => p.LibraryName).Returns(libName);
        this.mockPath.Setup(m => m.GetFileNameWithoutExtension(libName)).Returns(LibNameWithoutExt);
        this.mockPath.Setup(m => m.HasExtension(It.IsAny<string>()))
            .Returns<string>(path => path.Contains('.'));
        this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(extension);

        //Act
        var loader = CreateLoader();

        //Assert
        loader.LibraryName.Should().Be(WinLibNameWithExt);
    }
    #endregion

    #region Method Tests
    [Theory]
    [InlineData(WinDirPath, CrossPlatWinDirPath, WinLibNameWithExt, WinExtension)]
    [InlineData(LinuxDirPath, LinuxDirPath, PosixLibNameWithExt, PosixExtenstion)]
    public void LoadLibrary_WhenLibraryDoesNotLoad_ThrowsException(
        string dirPath,
        string expectedDirPath,
        string libName,
        string extension)
    {
        //Arrange
        const string systemError = "Could not load the library";

        var expectedPath = $"{expectedDirPath}{PosixSeparatorChar}{libName}";
        this.mockFile.Setup(m => m.Exists(It.IsAny<string?>())).Returns(true);

        this.mockDependencyManager.SetupGet(p => p.NativeLibDirPath).Returns(dirPath);

        this.mockLibrary.SetupGet(p => p.LibraryName).Returns(libName);

        this.mockPath.Setup(m => m.GetFileNameWithoutExtension(libName)).Returns(LibNameWithoutExt);
        this.mockPath.Setup(m => m.HasExtension(It.IsAny<string>()))
            .Returns<string>(path => path.Contains('.'));

        this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(extension);
        this.mockPlatform.Setup(m => m.GetLastSystemError()).Returns(systemError);

        var loader = CreateLoader();

        // Act
        var act = loader.LoadLibrary;

        // Assert
        act.Should().Throw<LoadLibraryException>()
            .WithMessage($"{systemError}\n\nLibrary Path: '{expectedPath}'");
    }

    [Fact]
    public void LoadLibrary_WhenInvoked_ReturnsLibraryPointer()
    {
        // Arrange
        const IntPtr expected = 1234;
        const string libFilePath = $"{CrossPlatWinDirPath}/{WinLibNameWithExt}";
        this.mockFile.Setup(m => m.Exists(libFilePath)).Returns(true);
        this.mockDependencyManager.SetupGet(p => p.NativeLibDirPath).Returns(WinDirPath);
        this.mockLibrary.SetupGet(p => p.LibraryName).Returns(WinLibNameWithExt);

        this.mockPath.Setup(m => m.GetFileNameWithoutExtension(WinLibNameWithExt)).Returns(LibNameWithoutExt);
        this.mockPath.Setup(m => m.HasExtension(It.IsAny<string>()))
            .Returns<string>(path => path.Contains('.'));

        this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(WinExtension);
        this.mockPlatform.Setup(m => m.LoadLibrary(libFilePath)).Returns(expected);

        var loader = CreateLoader();

        // Act
        var actual = loader.LoadLibrary();

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void LoadLibrary_WhenLibraryFileDoesNotExist_ThrowsException()
    {
        nint expected = 1234;
        var libFilePath = $"{CrossPlatWinDirPath}/{WinLibNameWithExt}";
        this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(false);
        this.mockDependencyManager.SetupGet(p => p.NativeLibDirPath).Returns(WinDirPath);
        this.mockLibrary.SetupGet(p => p.LibraryName).Returns(WinLibNameWithExt);

        this.mockPath.Setup(m => m.GetFileNameWithoutExtension(WinLibNameWithExt)).Returns(LibNameWithoutExt);
        this.mockPath.Setup(m => m.HasExtension(It.IsAny<string>()))
            .Returns<string>(path => path.Contains('.'));

        this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(WinExtension);
        this.mockPlatform.Setup(m => m.LoadLibrary(libFilePath)).Returns(expected);

        var loader = CreateLoader();

        // Act
        var act = loader.LoadLibrary;

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage($"Could not find the library '{WinLibNameWithExt}' in directory path '{CrossPlatWinDirPath}'");
    }
    #endregion

    /// <summary>
    /// Mocks a windows platform.
    /// </summary>
    private void MockPlatformAsWindows()
    {
        this.libDirPaths = new ReadOnlyCollection<string>(new[] { $@"C:\test-dir\" });
        this.libPath = $"{this.libDirPaths[0]}{WinLibNameWithExt}";

        this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(true);
        this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(false);
        this.mockPlatform.Setup(m => m.GetPlatformLibFileExtension()).Returns(".dll");
        this.mockPlatform.Setup(m => m.Is32BitProcess()).Returns(false);
        this.mockPlatform.Setup(m => m.Is64BitProcess()).Returns(true);
        this.mockPlatform.Setup(m => m.LoadLibrary(this.libPath)).Returns(new nint(1234));
        this.mockPlatform.Setup(m => m.GetLastSystemError()).Returns("Could not load module.");

        this.mockDirectory.Setup(m => m.Exists(this.libDirPaths[0])).Returns(true);

        this.mockFile.Setup(m => m.Exists(this.libPath)).Returns(true);

        this.mockPath.Setup(m => m.HasExtension(WinLibNameWithExt)).Returns(true);
        this.mockPath.Setup(m => m.GetFileNameWithoutExtension(It.IsAny<string>()))
            .Returns(WinLibNameWithExt.Replace(".dll", string.Empty));

        this.mockLibrary.SetupGet(p => p.LibraryName).Returns(WinLibNameWithExt);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="NativeLibraryLoader"/> class for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private NativeLibraryLoader CreateLoader()
        => new (this.mockDependencyManager.Object,
            this.mockPlatform.Object,
            this.mockDirectory.Object,
            this.mockFile.Object,
            this.mockPath.Object,
            this.mockLibrary.Object);
}
