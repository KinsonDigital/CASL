// <copyright file="OggAudioDecoderTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Data;

using System;
using CASL.Data.Decoders;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests the <see cref="OggAudioDecoder"/> class.
/// </summary>
public class OggAudioDecoderTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullFilePathParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new OggAudioDecoder(null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'filePath')");
    }

    [Fact]
    public void Ctor_WithEmptyFilePathParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new OggAudioDecoder(string.Empty);
        };

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("The value cannot be an empty string. (Parameter 'filePath')");
    }
    #endregion
}
