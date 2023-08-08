// <copyright file="ALContextTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.OpenAL;

using CASL.OpenAL;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests the <see cref="ALContext"/> struct.
/// </summary>
public class ALContextTests
{
    #region Constructor Tests
    [Fact]
    public void Ctr_WhenInvoked_SetHandleProperty()
    {
        // Arrange
        nint handle = 1234;

        // Act
        var context = new ALContext(handle);

        // Assert
        context.Handle.Should().Be(1234);
    }
    #endregion

    #region Operator Tests
    [Fact]
    public void ImplicitCast_WhenInvoked_SuccessfullyCastsContextToPointer()
    {
        // Arrange
        nint handle = 1234;
        var context = new ALContext(handle);

        // Act
        nint actual = context;

        // Assert
        actual.Should().Be(1234);
    }

    [Fact]
    public void EqualsOperator_WithEqualOperands_ReturnsTrue()
    {
        // Arrange
        var left = new ALContext(1234);
        var right = new ALContext(1234);

        // Act
        var actual = left == right;

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void NotEqualsOperator_WithNonEqualOperands_ReturnsFalse()
    {
        // Arrange
        var left = new ALContext(1234);
        var right = new ALContext(5678);

        // Act
        var actual = left != right;

        // Assert
        actual.Should().BeTrue();
    }
    #endregion

    #region Method Tests
    [Fact]
    public void Null_WhenInvoked_ReturnsEmptyContext()
    {
        // Act
        var actual = ALContext.Null();

        // Assert
        actual.Handle.Should().Be(0);
    }

    [Fact]
    public void Equals_WithContextParamOverloadAndEqualContext_ReturnsTrue()
    {
        // Arrange
        var contextA = new ALContext(1234);
        var contextB = new ALContext(1234);

        // Act
        var actual = contextA.Equals(contextB);

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithObjectParamOverloadAndNonContextType_ReturnsFalse()
    {
        // Arrange
        var contextA = new ALContext(1234);
        var contextB = new object();

        // Act
        var actual = contextA.Equals(contextB);

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithObjectParamOverloadAndEqualContext_ReturnsTrue()
    {
        // Arrange
        var contextA = new ALContext(1234);
        object contextB = new ALContext(1234);

        // Act
        var actual = contextA.Equals(contextB);

        // Assert
        actual.Should().BeTrue();
    }
    #endregion
}
