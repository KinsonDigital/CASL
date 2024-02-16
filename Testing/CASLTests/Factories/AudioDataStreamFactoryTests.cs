// <copyright file="AudioDataStreamFactoryTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Factories;

using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CASL.Factories;
using FluentAssertions;
using NSubstitute;
using Xunit;

/// <summary>
/// Tests the <see cref="AudioDataStreamFactory"/> class.
/// </summary>
public class AudioDataStreamFactoryTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullOrEmptyFileParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new AudioDataStreamFactory(null);
        };

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'file')");
    }
    #endregion

    #region Method Tests
    [Theory]
    [InlineData("", "The value cannot be an empty string. (Parameter 'filePath')")]
    [InlineData(null, "Value cannot be null. (Parameter 'filePath')")]
    public void CreateMp3AudioStream_WithNullOrEmptyParam_ThrowsException(string? filePath, string expected)
    {
        // Arrange
        var sut = new AudioDataStreamFactory(Substitute.For<IFile>());

        // Act
        var act = () =>
        {
            sut.CreateMp3AudioStream(filePath);
        };

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage(expected);
    }

    [Fact]
    public void CreateMp3AudioStream_WhenFileDoesNotExist_ThrowsException()
    {
        // Arrange
        var mockFile = Substitute.For<IFile>();

        var sut = new AudioDataStreamFactory(mockFile);

        // Act
        var act = () => sut.CreateMp3AudioStream("test-file");

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage("The MP3 audio file does not exist.")
            .Subject.First().FileName.Should().Be("test-file");
    }

    [Fact]
    public void CreateMp3AudioStream_WhenInvoked_CreatesStream()
    {
        // Arrange
        var mockFile = Substitute.For<IFile>();
        mockFile.Exists(Arg.Any<string>()).Returns(true);
        var sut = new AudioDataStreamFactory(mockFile);

        // Act
        var actual = sut.CreateMp3AudioStream("test-file");

        // Assert
        actual.Should().NotBeNull();
    }

    [Theory]
    [InlineData("", "The value cannot be an empty string. (Parameter 'filePath')")]
    [InlineData(null, "Value cannot be null. (Parameter 'filePath')")]
    public void CreateOggAudioStream_WithNullOrEmptyParam_ThrowsException(string? filePath, string expected)
    {
        // Arrange
        var sut = new AudioDataStreamFactory(Substitute.For<IFile>());

        // Act
        var act = () =>
        {
            sut.CreateOggAudioStream(filePath);
        };

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage(expected);
    }

    [Fact]
    public void CreateOggAudioStream_WhenFileDoesNotExist_ThrowsException()
    {
        // Arrange
        var mockFile = Substitute.For<IFile>();

        var sut = new AudioDataStreamFactory(mockFile);

        // Act
        var act = () => sut.CreateOggAudioStream("test-file");

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage("The OGG audio file does not exist.")
            .Subject.First().FileName.Should().Be("test-file");
    }

    [Fact]
    public void CreateOggAudioStream_WhenInvoked_CreatesStream()
    {
        // Arrange
        var mockFile = Substitute.For<IFile>();
        mockFile.Exists(Arg.Any<string>()).Returns(true);
        var sut = new AudioDataStreamFactory(mockFile);

        // Act
        var actual = sut.CreateOggAudioStream("test-file");

        // Assert
        actual.Should().NotBeNull();
    }
    #endregion
}
