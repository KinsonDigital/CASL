// <copyright file="ALDeviceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.OpenAL;

using CASL.OpenAL;
using Shouldly;
using Xunit;

/// <summary>
/// Tests the <see cref="ALDevice"/> struct.
/// </summary>
public class ALDeviceTests
{
    #region Constructor Tests
    [Fact]
    public void Ctr_WhenInvoked_SetHandleProperty()
    {
        // Arrange
        nint handle = 1234;

        // Act
        var device = new ALDevice(handle);

        // Assert
        Assert.Equal(1234, device.Handle);
    }
    #endregion

    #region Operator Tests
    [Fact]
    public void ImplicitCast_WhenInvoked_SuccessfullyCastsContextToPointer()
    {
        // Arrange
        nint handle = 1234;
        var device = new ALDevice(handle);

        // Act
        nint actual = device;

        // Assert
        Assert.Equal(1234, actual);
    }

    [Fact]
    public void EqualsOperator_WithEqualOperands_ReturnsTrue()
    {
        // Arrange
        var left = new ALDevice(1234);
        var right = new ALDevice(1234);

        // Act
        var actual = left == right;

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public void NotEqualsOperator_WithNonEqualOperands_ReturnsFalse()
    {
        // Arrange
        var left = new ALDevice(1234);
        var right = new ALDevice(5678);

        // Act
        var actual = left != right;

        // Assert
        actual.ShouldBeTrue();
    }
    #endregion

    #region Method Tests
    [Fact]
    public void Null_WhenInvoked_ReturnsEmptyDevice()
    {
        // Act
        var actual = ALDevice.Null();

        // Assert
        Assert.Equal(0, actual.Handle);
    }
    #endregion
}
