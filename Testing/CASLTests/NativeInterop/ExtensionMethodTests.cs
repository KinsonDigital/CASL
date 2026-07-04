// <copyright file="ExtensionMethodTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.NativeInterop;

using System;
using System.Runtime.InteropServices;
using CASL.NativeInterop;
using Shouldly;
using Xunit;

public class ExtensionMethodTests
{
    #region Method Tests
    [Fact]
    public void ToManagedUTF8String_WithNullPointer_ReturnsEmptyString()
    {
        // Act
        var actual = IntPtr.Zero.ToManagedUtf8String();

        // Assert
        actual.ShouldBeEmpty();
    }

    [Fact]
    public void ToManagedUTF8String_WhenInvoked_ReturnsCorrectResult()
    {
        // Arrange
        const string testString = "hello world";
        var stringDataPtr = Marshal.StringToHGlobalAnsi(testString);

        // Act
        var actual = stringDataPtr.ToManagedUtf8String();

        // Assert
        actual.ShouldBe("hello world");
    }

    [Fact]
    public void ToReadOnlyCollection_WhenInvoked_ReturnsCorrectResult()
    {
        // Arrange
        var testItems = new[] { "item-1", "item-2" };

        // Act
        var actual = testItems.ToReadOnlyCollection();

        // Assert
        actual.ShouldBe(["item-1", "item-2"], ignoreOrder: true);
    }
    #endregion
}
