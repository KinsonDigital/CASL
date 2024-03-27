// <copyright file="Mp3AudioDecoderTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.Data;

using System;
using CASL.Data.Decoders;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests the <see cref="Mp3AudioDecoder"/> class.
/// </summary>
public class Mp3AudioDecoderTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullFilePathParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new Mp3AudioDecoder(null);
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
            _ = new Mp3AudioDecoder(string.Empty);
        };

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("The value cannot be an empty string. (Parameter 'filePath')");
    }
    #endregion
}
