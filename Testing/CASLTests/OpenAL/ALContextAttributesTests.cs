// <copyright file="ALContextAttributesTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CASLTests.OpenAL
{
    using CASL.OpenAL;
    using Xunit;

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
            Assert.Null(actual.Frequency);
            Assert.Null(actual.MonoSources);
            Assert.Null(actual.StereoSources);
            Assert.Null(actual.Refresh);
            Assert.Null(actual.Sync);
            Assert.Empty(actual.AdditionalAttributes);
        }

        [Fact]
        public void Ctor_WhenUsingCtorWithParams_SetsPropValues()
        {
            // Act
            var actual = new ALContextAttributes(11, 22, 33, 44, true);

            // Assert
            Assert.Equal(11, actual.Frequency);
            Assert.Equal(22, actual.MonoSources);
            Assert.Equal(33, actual.StereoSources);
            Assert.Equal(44, actual.Refresh);
            Assert.True(actual.Sync);
            Assert.Empty(actual.AdditionalAttributes);
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
            Assert.Equal(11, actual.Length);
            Assert.Equal((int)AlcContextAttributes.Frequency, actual[0]);
            Assert.Equal(111, actual[1]);

            Assert.Equal((int)AlcContextAttributes.MonoSources, actual[2]);
            Assert.Equal(222, actual[3]);

            Assert.Equal((int)AlcContextAttributes.StereoSources, actual[4]);
            Assert.Equal(333, actual[5]);

            Assert.Equal((int)AlcContextAttributes.Refresh, actual[6]);
            Assert.Equal(444, actual[7]);

            Assert.Equal(expectedAttrValue, actual[8]);
            Assert.Equal(expectedIntValue, actual[9]);

            // Assert the trailing byte
            Assert.Equal(0, actual[10]);
        }

        [Fact]
        public void CreateAttributeArray_WithNullMonoSources_CorrectlyCreatesAttributeArray()
        {
            // Arrange
            var contextAttributes = new ALContextAttributes(111, null, 333, 444, true);

            // Act
            var actual = contextAttributes.CreateAttributeArray();

            // Assert
            Assert.Equal(11, actual.Length);
            Assert.Equal((int)AlcContextAttributes.Frequency, actual[0]);
            Assert.Equal(111, actual[1]);

            Assert.Equal((int)AlcContextAttributes.StereoSources, actual[2]);
            Assert.Equal(333, actual[3]);

            Assert.Equal((int)AlcContextAttributes.Refresh, actual[4]);
            Assert.Equal(444, actual[5]);

            Assert.Equal((int)AlcContextAttributes.Sync, actual[6]);
            Assert.Equal(1, actual[7]);

            // Assert missing attribute 'mono sources'
            Assert.Equal(0, actual[8]);
            Assert.Equal(0, actual[9]);

            // Assert the trailing byte
            Assert.Equal(0, actual[10]);
        }

        [Fact]
        public void CreateAttributeArray_WithNullSync_CorrectlyCreatesAttributeArray()
        {
            // Arrange
            var contextAttributes = new ALContextAttributes(111, 222, 333, 444, null);

            // Act
            var actual = contextAttributes.CreateAttributeArray();

            // Assert
            Assert.Equal(11, actual.Length);
            Assert.Equal((int)AlcContextAttributes.Frequency, actual[0]);
            Assert.Equal(111, actual[1]);

            Assert.Equal((int)AlcContextAttributes.MonoSources, actual[2]);
            Assert.Equal(222, actual[3]);

            Assert.Equal((int)AlcContextAttributes.StereoSources, actual[4]);
            Assert.Equal(333, actual[5]);

            Assert.Equal((int)AlcContextAttributes.Refresh, actual[6]);
            Assert.Equal(444, actual[7]);

            // Assert that the sync attribute is zero
            Assert.Equal(0, actual[8]);
            Assert.Equal(0, actual[9]);

            // Assert the trailing byte
            Assert.Equal(0, actual[10]);
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
            Assert.Equal(13, actual.Length);
            Assert.Equal((int)AlcContextAttributes.Frequency, actual[0]);
            Assert.Equal(111, actual[1]);

            Assert.Equal((int)AlcContextAttributes.MonoSources, actual[2]);
            Assert.Equal(222, actual[3]);

            Assert.Equal((int)AlcContextAttributes.StereoSources, actual[4]);
            Assert.Equal(333, actual[5]);

            Assert.Equal((int)AlcContextAttributes.Refresh, actual[6]);
            Assert.Equal(444, actual[7]);

            Assert.Equal((int)AlcContextAttributes.Sync, actual[8]);
            Assert.Equal(1, actual[9]);

            // Assert additional fake attributes
            Assert.Equal(555, actual[10]);
            Assert.Equal(666, actual[11]);

            // Assert the trailing byte
            Assert.Equal(0, actual[12]);
        }

        [Fact]
        public void ToString_WithAdditionalAttributes_CreatesCorrectString()
        {
            // Arrange
            var expected = "";

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
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToString_WithNoAdditionalAttributes_CreatesCorrectString()
        {
            // Arrange
            var expected = "";

            expected += $"{nameof(AlcContextAttributes.Frequency)}: 111, ";
            expected += $"{nameof(AlcContextAttributes.MonoSources)}: 222, ";
            expected += $"{nameof(AlcContextAttributes.StereoSources)}: N/A, ";
            expected += $"{nameof(AlcContextAttributes.Refresh)}: 444, ";
            expected += $"{nameof(AlcContextAttributes.Sync)}: True";

            var contextAttributes = new ALContextAttributes(111, 222, null, 444, true);

            // Act
            var actual = contextAttributes.ToString();

            // Assert
            Assert.Equal(expected, actual);
        }
        #endregion
    }
}
