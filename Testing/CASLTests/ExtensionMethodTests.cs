// <copyright file="ExtensionMethodTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests;

using System;
using System.Runtime.InteropServices;
using CASL;
using Shouldly;
using Xunit;

/// <summary>
/// Tests the <see cref="ExtensionMethods"/> class.
/// </summary>
public class ExtensionMethodTests
{
    [Fact]
    public void ToStrings_WithZeroPointer_ReturnsEmptyArray()
    {
        // Arrange
        const IntPtr ptr = 0;

        // Act
        var actual = ptr.ToStrings();

        // Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void ToStrings_WithNullCharSeparatedItems_ReturnsCorrectList()
    {
        // Arrange
        const string strValues = "Item1\0Item2\0";
        var ptr = Marshal.StringToHGlobalAnsi(strValues);

        // Act
        var actual = ptr.ToStrings();

        // Assert
        Assert.Equal(2, actual.Length);
        Assert.Equal("Item1", actual[0]);
        Assert.Equal("Item2", actual[1]);
    }

    [Theory]
    [InlineData("test-value/", "test-value")]
    [InlineData("test-value///", "test-value")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("/", "")]
    [InlineData("///", "")]
    public void TrimAllFromEnd_WhenInvoked_ReturnsCorrectResult(string? value, string expected)
    {
        // Arrange & Act
        var actual = value?.TrimAllFromEnd('/') ?? string.Empty;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MapValue_WhenMappingFloatValueToLongValue_ReturnsCorrectResult()
    {
        // Arrange
        const float value = 123f;

        // Act
        var actual = value.MapValue(0f, 200f, 0L, 1000L);

        // Assert
        actual.ShouldBe(615);
    }

    [Fact]
    public void MapValue_WhenMappingFloatValueToIntValue_ReturnsCorrectResult()
    {
        // Arrange
        const float value = 123f;

        // Act
        var actual = value.MapValue(0f, 200f, 0, 1000);

        // Assert
        actual.ShouldBe(615);
    }

    [Fact]
    public void MapValue_WhenMappingLongValueToFloatValue_ReturnsCorrectResult()
    {
        // Arrange
        const long value = 123L;

        // Act
        var actual = value.MapValue(0L, 200L, 0f, 1000f);

        // Assert
        actual.ShouldBe(615);
    }
}
