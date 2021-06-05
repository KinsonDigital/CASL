// <copyright file="ExtensionMethodTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests
{
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
            nint ptr = 0;

            // Act
            var actual = ptr.ToStrings();

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public void ToStrings_WithNullCharSeparatedItems_ReturnsCorrectList()
        {
            // Arrange
            var strValues = "Item1\0Item2\0";
            nint ptr = Marshal.StringToHGlobalAnsi(strValues);

            // Act
            var actual = ptr.ToStrings();

            // Assert
            Assert.Equal(2, actual.Length);
            Assert.Equal("Item1", actual[0]);
            Assert.Equal("Item2", actual[1]);
        }
    }
}
