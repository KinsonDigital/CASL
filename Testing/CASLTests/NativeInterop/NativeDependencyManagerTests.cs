// <copyright file="NativeDependencyManagerTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

#pragma warning disable IDE0002 // Name can be simplified
namespace CASLTests.NativeInterop;

#pragma warning disable IDE0001 // Name can be simplified
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CASL.NativeInterop;
using Moq;
using Xunit;
using Assert = Helpers.AssertExtensions;
using FluentAssertions;
#pragma warning restore IDE0001 // Name can be simplified

/// <summary>
/// Tests the <see cref="NativeDependencyManager"/> class.
/// </summary>
public class NativeDependencyManagerTests
{
    private readonly Mock<IFile> mockFile;
    private readonly Mock<IPath> mockPath;
    private readonly Mock<IFilePathResolver> mockPathResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeDependencyManagerTests"/> class.
    /// </summary>
    public NativeDependencyManagerTests()
    {
        this.mockFile = new Mock<IFile>();
        this.mockPath = new Mock<IPath>();
        this.mockPathResolver = new Mock<IFilePathResolver>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WhenInvokedWithNullFile_ThrowsException()
    {
        // Act
        var act = () => new OpenALDependencyManager(
                null,
                this.mockPath.Object,
                this.mockPathResolver.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'file')");
    }

    [Fact]
    public void Ctor_WhenInvokedWithNullPath_ThrowsException()
    {
        // Act
        var act = () => new OpenALDependencyManager(
                this.mockFile.Object,
                null,
                this.mockPathResolver.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'path')");
    }

    [Fact]
    public void Ctor_WhenInvokedWithNullPathResolver_ThrowsException()
    {
        // Act
        var act = () => new OpenALDependencyManager(
                this.mockFile.Object,
                this.mockPath.Object,
                null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'nativeLibPathResolver')");
    }
    #endregion

    #region Prop Tests
    [Fact]
    public void NativeLibraries_WhenSettingValue_ReturnsCorrectResult()
    {
        // Arrange
        const string dirPath = "C:/test-dir";
        const string libNameWithExtension = "test-native-lib.dll";
        const string libNameWithoutExtension = "test-native-lib";

        this.mockPathResolver.Setup(m => m.GetDirPath()).Returns(dirPath);
        this.mockPath.Setup(m => m.GetFileNameWithoutExtension(libNameWithExtension))
            .Returns(libNameWithoutExtension);

        var manager = CreateManager();

        // Act
        manager.NativeLibraries = new ReadOnlyCollection<string>(new List<string> { libNameWithExtension });
        var actual = manager.NativeLibraries;

        // Assert
        actual.Should().HaveCount(1);
        actual.First().Should().Be(libNameWithoutExtension);
    }

    [Theory]
    [InlineData(@"C:\test-dir")]
    [InlineData(@"C:\test-dir\")]
    [InlineData(@"C:\test-dir\\")]
    [InlineData("C:/test-dir")]
    [InlineData("C:/test-dir/")]
    [InlineData("C:/test-dir//")]
    public void NativeLibDirPath_WhenGettingValue_ReturnsCorrectResult(string dirPath)
    {
        // Arrange
        const string expected = "C:/test-dir";

        this.mockPathResolver.Setup(m => m.GetDirPath()).Returns(dirPath);

        var sut = CreateManager();

        // Act
        var actual = sut.NativeLibDirPath;

        // Assert
        actual.Should().Be(expected);
    }
    #endregion

    #region Method Tests
    [Fact]
    public void VerifyDependencies_WhenLibrarySrcDoesNotExist_ThrowsException()
    {
        // Arrange
        const string assemblyDirPath = @"C:/test-dir";
        const string srcDirPath = $@"{assemblyDirPath}/runtimes/win-x64/native";

        this.mockFile.Setup(m => m.Exists($"{srcDirPath}/lib.dll")).Returns(false);
        this.mockPathResolver.Setup(m => m.GetDirPath()).Returns(srcDirPath);

        this.mockPath.Setup(m => m.GetExtension("lib.dll")).Returns(".dll");
        this.mockPath.Setup(m => m.GetFileNameWithoutExtension("lib.dll")).Returns("lib");

        var manager = CreateManager();
        manager.NativeLibraries = new ReadOnlyCollection<string>(new[] { "lib.dll" }.ToList());

        // Act
        var act = manager.VerifyDependencies;

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage($"The native dependency library '{srcDirPath}/lib.dll' does not exist.");
    }

    [Fact]
    public void VerifyDependencies_WhenNativeLibExists_DoesNotThrowException()
    {
        // Arrange
        var assemblyDirPath = @"C:\test-dir\";
        var srcDirPath = $@"{assemblyDirPath}runtimes\win-x64\native\";

        this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(true);
        this.mockPathResolver.Setup(m => m.GetDirPath()).Returns(srcDirPath);

        this.mockPath.Setup(m => m.GetExtension("lib.dll")).Returns(".dll");
        this.mockPath.Setup(m => m.GetFileNameWithoutExtension("lib.dll")).Returns("lib");

        var manager = CreateManager();
        manager.NativeLibraries = new ReadOnlyCollection<string>(new[] { "lib.dll" }.ToList());

        // Act
        var act = manager.VerifyDependencies;

        // Assert
        act.Should().NotThrow<FileNotFoundException>();
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="OpenALDependencyManager"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private OpenALDependencyManager CreateManager()
        => new (this.mockFile.Object,
            this.mockPath.Object,
            this.mockPathResolver.Object);
}
