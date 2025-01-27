// <copyright file="OpenALLibraryTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

#pragma warning disable IDE0002 // The name can be simplified
namespace CASLTests.NativeInterop;

#pragma warning disable IDE0001 // The name can be simplified
using System.IO;
using System.IO.Abstractions;
using CASL;
using CASL.Exceptions;
using CASL.NativeInterop;
using Xunit;
using FluentAssertions;
using NSubstitute;

#pragma warning restore IDE0001 // The name can be simplified

/// <summary>
/// Tests the <see cref="OpenALLibrary"/> class.
/// </summary>
public class OpenALLibraryTests
{
    private readonly IPlatform mockPlatform;
    private readonly IDirectory mockDirectory;
    private readonly IFile mockFile;
    private readonly IPath mockPath;
    private readonly IApplication mockApplication;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenALLibraryTests"/> class.
    /// </summary>
    public OpenALLibraryTests()
    {
        this.mockPlatform = Substitute.For<IPlatform>();
        this.mockDirectory = Substitute.For<IDirectory>();
        this.mockFile = Substitute.For<IFile>();
        this.mockPath = Substitute.For<IPath>();
        this.mockApplication = Substitute.For<IApplication>();
    }


    // TODO: Add ctor tests for null params

    #region Constructor Tests
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
        act.Should().Throw<UnknownPlatformException>()
            .WithMessage("The platform 'xyz' is unknown or not supported.");
    }

    [Fact]
    public void Ctor_WhenPlatformDirPathDoesNotExist_ThrowsException()
    {
        // Arrange
        MockWindowsPlatform();
        this.mockApplication.Location.Returns(@"C:\app-dir");
        this.mockFile.Exists(Arg.Any<string>()).Returns(false);
        this.mockDirectory.Exists(Arg.Any<string>()).Returns(false);

        // Act
        var act = CreateSystemUnderTest;

        // Assert
        act.Should().Throw<DirectoryNotFoundException>()
            .WithMessage(@"The directory 'C:\app-dir\runtimes\win-x64\native' does not exist.");
    }

    [Fact]
    public void Ctor_WhenFullPlatformLibFilePathDoesNotExist_ThrowsException()
    {
        // Arrange
        const string expected = @"The library 'soft_oal.dll' does not exist" +
                                @" in the platform directory 'C:\app-dir\runtimes\win-x64\native'.";
        MockWindowsPlatform();
        this.mockApplication.Location.Returns(@"C:\app-dir");
        this.mockFile.Exists(Arg.Any<string>()).Returns(false);
        this.mockDirectory.Exists(Arg.Any<string>()).Returns(true);

        // Act
        var act = CreateSystemUnderTest;

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage(expected);
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
    public void Ctor_WithPosixPlatforms_ProcessesLibFile()
    {
        // Arrange
        MockPosixPlatform();
        this.mockFile.Exists("/app-dir/libopenal.so").Returns(false);
        this.mockDirectory.Exists(Arg.Any<string>()).Returns(true);
        this.mockFile.Exists("/app-dir/runtimes/linux-x64/native/libopenal.so").Returns(true);

        // Act
        CreateSystemUnderTest();

        // Assert
        this.mockFile.Received(1)
            .Copy("/app-dir/runtimes/linux-x64/native/libopenal.so", "/app-dir/libopenal.so");
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
        actual.Should().Be("soft_oal.dll");
    }

    [Fact]
    public void GetLibraryName_WhenGettingValueWithPosixPlatform_ReturnsCorrectResult()
    {
        // Arrange
        MockPosixPlatform();
        this.mockFile.Exists(Arg.Any<string>()).Returns(true);
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.GetLibraryName();

        // Assert
        actual.Should().Be("libopenal.so");
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
            this.mockApplication);

    /// <summary>
    /// Mocks a windows platform.
    /// </summary>
    private void MockWindowsPlatform()
    {
        this.mockPlatform.IsWinPlatform().Returns(true);
        this.mockPlatform.IsPosixPlatform().Returns(false);
        this.mockPath.DirectorySeparatorChar.Returns('\\');
        this.mockApplication.Location.Returns(@"C:\app-dir");
    }

    /// <summary>
    /// Mocks a posix platform.
    /// </summary>
    private void MockPosixPlatform()
    {
        this.mockPlatform.IsWinPlatform().Returns(false);
        this.mockPlatform.IsPosixPlatform().Returns(true);
        this.mockPath.DirectorySeparatorChar.Returns('/');
        this.mockApplication.Location.Returns("/app-dir");
    }
}
