﻿// <copyright file="ExtensionMethodTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests;

using System;
using System.Runtime.InteropServices;
using CASL;
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
    [InlineData(@"C:\dir-1\dir-2", "C:/dir-1/dir-2")]
    [InlineData(@"C:\dir-1\dir-2\", "C:/dir-1/dir-2/")]
    public void ToCrossPlatPath_WhenInvoked_ReturnsCorrectResult(string path, string expected)
    {
        // Act
        var actual = path.ToCrossPlatPath();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("test-value/", "test-value")]
    [InlineData("test-value///", "test-value")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("/", "")]
    [InlineData("///", "")]
    public void TrimAllFromEnd_WhenInvoked_ReturnsCorrectResult(string value, string expected)
    {
        // Arrange & Act
        var actual = value.TrimAllFromEnd('/');

        // Assert
        Assert.Equal(expected, actual);
    }
}
