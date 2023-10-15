// <copyright file="ALContextAttributesTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.OpenAL;

using CASL.OpenAL;
using Xunit;
using FluentAssertions;

/// <summary>
/// Tests the <see cref="ALContextAttributes"/> class.
/// </summary>
public class ALContextAttributesTests
{
    #region Ctor Tests
    [Fact]
    public void Ctor_WhenUsingCtorWithNoParams_DoesNotSetAnyProperties()
    {
        // Act
        var actual = new ALContextAttributes();

        // Assert
        actual.Frequency.Should().BeNull();
        actual.MonoSources.Should().BeNull();
        actual.StereoSources.Should().BeNull();
        actual.Refresh.Should().BeNull();
        actual.Sync.Should().BeNull();
        actual.AdditionalAttributes.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_WhenUsingCtorWithParams_SetsPropValues()
    {
        // Act
        var actual = new ALContextAttributes(11, 22, 33, 44, true);

        // Assert
        actual.Frequency.Should().Be(11);
        actual.MonoSources.Should().Be(22);
        actual.StereoSources.Should().Be(33);
        actual.Refresh.Should().Be(44);
        actual.Sync.Should().BeTrue();
        actual.AdditionalAttributes.Should().BeEmpty();
    }
    #endregion

    #region Method Tests
    [Theory]
    [InlineData(true, (int)AlcContextAttributes.Sync, 1)]
    [InlineData(false, (int)AlcContextAttributes.Sync, 0)]
    [InlineData(null, 0, 0)]
    public void CreateAttributeArray_WhenInvoked_CreatesAttributeArray(bool? sync, int expectedAttrValue, int expectedIntValue)
    {
        // Arrange
        var contextAttributes = new ALContextAttributes(111, 222, 333, 444, sync);

        // Act
        var actual = contextAttributes.CreateAttributeArray();

        // Assert
        actual.Should().ContainInOrder(
            (int)AlcContextAttributes.Frequency,
            111,
            (int)AlcContextAttributes.MonoSources,
            222,
            (int)AlcContextAttributes.StereoSources,
            333,
            (int)AlcContextAttributes.Refresh,
            444,
            expectedAttrValue,
            expectedIntValue,
            // Assert the trailing byte
            0);
    }

    [Fact]
    public void CreateAttributeArray_WithNullMonoSources_CorrectlyCreatesAttributeArray()
    {
        // Arrange
        var contextAttributes = new ALContextAttributes(111, null, 333, 444, true);

        // Act
        var actual = contextAttributes.CreateAttributeArray();

        // Assert
        actual.Should().HaveCount(11);
        actual.Should().ContainInOrder(
            11,
            (int)AlcContextAttributes.Frequency,
            111,
            (int)AlcContextAttributes.StereoSources,
            333,
            (int)AlcContextAttributes.Refresh,
            444,
            (int)AlcContextAttributes.Sync,
            1,
            // Assert missing attribute 'mono sources'
            0,
            0,
            // Assert the trailing byte
            0);
    }

    [Fact]
    public void CreateAttributeArray_WithNullSync_CorrectlyCreatesAttributeArray()
    {
        // Arrange
        var contextAttributes = new ALContextAttributes(111, 222, 333, 444, null);

        // Act
        var actual = contextAttributes.CreateAttributeArray();

        // Assert
        actual.Should().HaveCount(11);
        actual.Should().ContainInOrder(
            (int)AlcContextAttributes.Frequency,
            111,
            (int)AlcContextAttributes.MonoSources,
            222,
            (int)AlcContextAttributes.StereoSources,
            333,
            (int)AlcContextAttributes.Refresh,
            444,
            // Assert values at index 8 and 9 (sync bytes) are zero
            0,
            0,
            // Assert trailing byte is zero
            0);
    }

    [Fact]
    public void CreateAttributeArray_WithAnAdditionalAttribute_CorrectlyCreatesAttributeArray()
    {
        // Arrange
        var contextAttributes = new ALContextAttributes(111, 222, 333, 444, true);
        contextAttributes.AdditionalAttributes = new[] { 555, 666 };

        // Act
        var actual = contextAttributes.CreateAttributeArray();

        // Assert
        actual.Should().HaveCount(13);
        actual.Should().ContainInOrder(
            (int)AlcContextAttributes.Frequency,
            111,
            (int)AlcContextAttributes.MonoSources,
            222,
            (int)AlcContextAttributes.StereoSources,
            333,
            (int)AlcContextAttributes.Refresh,
            444,
            (int)AlcContextAttributes.Sync,
            1,
            // Assert additional fake attributes
            555,
            666,
            // Assert trailing byte
            0);
    }

    [Fact]
    public void ToString_WithAdditionalAttributes_CreatesCorrectString()
    {
        // Arrange
        var expected = string.Empty;

        expected += $"{nameof(AlcContextAttributes.Frequency)}: 111, ";
        expected += $"{nameof(AlcContextAttributes.MonoSources)}: 222, ";
        expected += $"{nameof(AlcContextAttributes.StereoSources)}: N/A, ";
        expected += $"{nameof(AlcContextAttributes.Refresh)}: 444, ";
        expected += $"{nameof(AlcContextAttributes.Sync)}: True, ";
        expected += $"555,";
        expected += $" 666";

        var contextAttributes = new ALContextAttributes(111, 222, null, 444, true);
        contextAttributes.AdditionalAttributes = new[] { 555, 666 };

        // Act
        var actual = contextAttributes.ToString();

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void ToString_WithNoAdditionalAttributes_CreatesCorrectString()
    {
        // Arrange
        var expected = string.Empty;

        expected += $"{nameof(AlcContextAttributes.Frequency)}: 111, ";
        expected += $"{nameof(AlcContextAttributes.MonoSources)}: 222, ";
        expected += $"{nameof(AlcContextAttributes.StereoSources)}: N/A, ";
        expected += $"{nameof(AlcContextAttributes.Refresh)}: 444, ";
        expected += $"{nameof(AlcContextAttributes.Sync)}: True";

        var contextAttributes = new ALContextAttributes(111, 222, null, 444, true);

        // Act
        var actual = contextAttributes.ToString();

        // Assert
        actual.Should().Be(expected);
    }
    #endregion
}
