// <copyright file="OpenALLibraryTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

#pragma warning disable IDE0002 // The name can be simplified
namespace CASLTests.NativeInterop;

#pragma warning disable IDE0001 // The name can be simplified
using CASL.Exceptions;
using CASL.NativeInterop;
using Moq;
using Xunit;
using FluentAssertions;
using Assert = Helpers.AssertExtensions;
#pragma warning restore IDE0001 // The name can be simplified

/// <summary>
/// Tests the <see cref="OpenALLibrary"/> class.
/// </summary>
public class OpenALLibraryTests
{
    private readonly Mock<IPlatform> mockPlatform;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenALLibraryTests"/> class.
    /// </summary>
    public OpenALLibraryTests() => this.mockPlatform = new Mock<IPlatform>();

    #region Prop Tests
    [Fact]
    public void LibraryName_WhenGettingValueWithWindowsPlatform_ReturnsCorrectResult()
    {
        // Arrange
        MockWindowsPlatform();
        var library = new OpenALLibrary(this.mockPlatform.Object);

        // Act
        var actual = library.LibraryName;

        // Assert
        actual.Should().Be("soft_oal.dll");
    }

    [Fact]
    public void LibraryName_WhenGettingValueWithPosixPlatform_ReturnsCorrectResult()
    {
        // Arrange
        MockPosixPlatform();
        var library = new OpenALLibrary(this.mockPlatform.Object);

        // Act
        var actual = library.LibraryName;

        // Assert
        actual.Should().Be("libopenal.so");
    }

    [Fact]
    public void LibraryName_WithUnknownPlatform_ThrowsException()
    {
        // Arrange
        this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(false);
        this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(false);
        this.mockPlatform.SetupGet(p => p.CurrentOSPlatform).Returns("unknown-platform");

        var library = new OpenALLibrary(this.mockPlatform.Object);

        // Act
        var act = () => library.LibraryName;

        // Assert
        act.Should().Throw<UnknownPlatformException>()
            .WithMessage("The platform 'unknown-platform' is unknown.");
    }
    #endregion

    /// <summary>
    /// Mocks a windows platform.
    /// </summary>
    private void MockWindowsPlatform()
    {
        this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(true);
        this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(false);
    }

    /// <summary>
    /// Mocks a posix platform.
    /// </summary>
    private void MockPosixPlatform()
    {
        this.mockPlatform.Setup(m => m.IsWinPlatform()).Returns(false);
        this.mockPlatform.Setup(m => m.IsPosixPlatform()).Returns(true);
    }
}
