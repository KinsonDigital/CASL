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
using Xunit;
using FluentAssertions;
using NSubstitute;

#pragma warning restore IDE0001 // Name can be simplified

/// <summary>
/// Tests the <see cref="NativeDependencyManager"/> class.
/// </summary>
public class NativeDependencyManagerTests
{
    private readonly IFile mockFile;
    private readonly IPath mockPath;
    private readonly IFilePathResolver mockPathResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeDependencyManagerTests"/> class.
    /// </summary>
    public NativeDependencyManagerTests()
    {
        this.mockFile = Substitute.For<IFile>();
        this.mockPath = Substitute.For<IPath>();
        this.mockPathResolver = Substitute.For<IFilePathResolver>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WhenInvokedWithNullFile_ThrowsException()
    {
        // Act
        var act = () => new OpenALDependencyManager(
                null,
                this.mockPath,
                this.mockPathResolver);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'file')");
    }

    [Fact]
    public void Ctor_WhenInvokedWithNullPath_ThrowsException()
    {
        // Act
        var act = () => new OpenALDependencyManager(
                this.mockFile,
                null,
                this.mockPathResolver);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'path')");
    }

    [Fact]
    public void Ctor_WhenInvokedWithNullPathResolver_ThrowsException()
    {
        // Act
        var act = () => new OpenALDependencyManager(
                this.mockFile,
                this.mockPath,
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

        this.mockPathResolver.GetDirPath().Returns(dirPath);
        this.mockPath.GetFileNameWithoutExtension(libNameWithExtension).Returns(libNameWithoutExtension);

        var manager = CreateManager();

        // Act
        manager.NativeLibraries = new ReadOnlyCollection<string>(new List<string> { libNameWithExtension });
        var actual = manager.NativeLibraries;

        // Assert
        actual.Should().HaveCount(1);
        actual[0].Should().Be(libNameWithoutExtension);
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

        this.mockPathResolver.GetDirPath().Returns(dirPath);

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

        this.mockFile.Exists($"{srcDirPath}/lib.dll").Returns(false);
        this.mockPathResolver.GetDirPath().Returns(srcDirPath);

        this.mockPath.GetExtension("lib.dll").Returns(".dll");
        this.mockPath.GetFileNameWithoutExtension("lib.dll").Returns("lib");

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
        const string assemblyDirPath = @"C:\test-dir\";
        const string srcDirPath = $@"{assemblyDirPath}runtimes\win-x64\native\";

        this.mockFile.Exists(Arg.Any<string>()).Returns(true);
        this.mockPathResolver.GetDirPath().Returns(srcDirPath);

        this.mockPath.GetExtension("lib.dll").Returns(".dll");
        this.mockPath.GetFileNameWithoutExtension("lib.dll").Returns("lib");

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
        => new (this.mockFile,
            this.mockPath,
            this.mockPathResolver);
}
