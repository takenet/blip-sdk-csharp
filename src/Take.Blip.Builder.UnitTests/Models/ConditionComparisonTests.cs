using Shouldly;
using Take.Blip.Builder.Models;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Models
{
    public class ConditionComparisonTests
    {
        [Fact]
        public void EqualsComparisonWithEqualsValuesShouldSucceed()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Value 1";
            var target = ConditionComparison.Equals.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void EqualsComparisonWithEqualsValuesWithDifferentCasingShouldSucceed()
        {
            // Arrange
            var value1 = "ValUe 1";
            var value2 = "value 1";
            var target = ConditionComparison.Equals.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void EqualsComparisonWithEqualsValuesWithEmojiShouldSucceed()
        {
            // Arrange
            var value1 = "Value 😱🤢➡";
            var value2 = "Value 😱🤢➡";
            var target = ConditionComparison.Equals.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }
    
        [Fact]
        public void EqualsComparisonWithEqualsValuesWithAccentShouldSucceed()
        {
            // Arrange
            var value1 = "Valuê 1";
            var value2 = "Válue 1";
            var target = ConditionComparison.Equals.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }
        [Fact]
        public void EqualsComparisonWithUnequalsValuesShouldFail()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Value X";
            var target = ConditionComparison.Equals.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeFalse();
        }

        [Fact]
        public void NotEqualsComparisonWithEqualsValuesShouldFail()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Value 1";
            var target = ConditionComparison.NotEquals.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeFalse();
        }

        [Fact]
        public void NotEqualsComparisonWithEqualsValuesWithDifferentCasingShouldFail()
        {
            // Arrange
            var value1 = "ValUe 1";
            var value2 = "value 1";
            var target = ConditionComparison.NotEquals.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeFalse();
        }

        [Fact]
        public void NotEqualsComparisonWithEqualsValuesWithAccentShouldFail()
        {
            // Arrange
            var value1 = "Valuê 1";
            var value2 = "Válue 1";
            var target = ConditionComparison.NotEquals.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeFalse();
        }
        [Fact]
        public void NotEqualsComparisonWithUnequalsValuesShouldSucceed()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Value X";
            var target = ConditionComparison.NotEquals.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void StartsWithComparisonWithEqualsValuesShouldSucceed()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Value 1";
            var target = ConditionComparison.StartsWith.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void StartsWithComparisonWithEqualsValuesWithDifferentCasingShouldSucceed()
        {
            // Arrange
            var value1 = "ValUe 1";
            var value2 = "value 1";
            var target = ConditionComparison.StartsWith.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void StartsWithComparisonWithPartialValuesShouldSucceed()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Valu";
            var target = ConditionComparison.StartsWith.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void StartsWithComparisonWithInvalidValuesShouldFail()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Balue";
            var target = ConditionComparison.StartsWith.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeFalse();
        }


        [Fact]
        public void GreaterThanWithComparisonWithBiggerValuesShouldSucceed()
        {
            // Arrange
            var value1 = "12345";
            var value2 = "1234";
            var target = ConditionComparison.GreaterThan.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void GreaterThanWithComparisonWithEqualsValuesShouldFail()
        {
            // Arrange
            var value1 = "12345";
            var value2 = "12345";
            var target = ConditionComparison.GreaterThan.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeFalse();
        }

        [Fact]
        public void GreaterThanWithComparisonWithSmallerValuesShouldFail()
        {
            // Arrange
            var value1 = "1234";
            var value2 = "12345";
            var target = ConditionComparison.GreaterThan.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeFalse();
        }

        [Fact]
        public void GreaterThanWithComparisonWithInvalidValuesShouldFail()
        {
            // Arrange
            var value1 = "not a number";
            var value2 = "also not a number";
            var target = ConditionComparison.GreaterThan.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeFalse();
        }

        [Fact]
        public void ApproximateToComparisonWithApproximateToValuesShouldSucceed()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Value 1";
            var target = ConditionComparison.ApproximateTo.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void ApproximateToComparisonWithApproximateToValuesWithDifferentCasingShouldSucceed()
        {
            // Arrange
            var value1 = "ValUe 1";
            var value2 = "value 1";
            var target = ConditionComparison.ApproximateTo.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void ApproximateToComparisonWithApproximateToValuesWithAccentShouldSucceed()
        {
            // Arrange
            var value1 = "Valuê 1";
            var value2 = "Válue 1";
            var target = ConditionComparison.ApproximateTo.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void ApproximateToComparisonWithSlightlyUnequalsValuesShouldSucceed()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Vilue X";
            var target = ConditionComparison.ApproximateTo.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void ApproximateToComparisonWithCompletelyUnequalsValuesShouldFail()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Hello world";
            var target = ConditionComparison.ApproximateTo.ToBinaryDelegate();

            // Act
            target(value1, value2).ShouldBeFalse();
        }

        [Fact]
        public void ExistsComparisonWithExistingValueShouldSucceed()
        {
            // Arrange
            var value = "Value";
            var target = ConditionComparison.Exists.ToUnaryDelegate();

            // Act
            target(value).ShouldBeTrue();
        }

        [Fact]
        public void ExistsComparisonWithEmptyStringShouldFail()
        {
            // Arrange
            var value = "";
            var target = ConditionComparison.Exists.ToUnaryDelegate();

            // Act
            target(value).ShouldBeFalse();
        }

        [Fact]
        public void ExistsComparisonNonExistingValueShouldFail()
        {
            // Arrange
            string value = null;
            var target = ConditionComparison.Exists.ToUnaryDelegate();

            // Act
            target(value).ShouldBeFalse();
        }

        [Fact]
        public void NotExistsComparisonWithExistingValueShouldFail()
        {
            // Arrange
            var value = "Value";
            var target = ConditionComparison.NotExists.ToUnaryDelegate();

            // Act
            target(value).ShouldBeFalse();
        }

        [Fact]
        public void NotExistsComparisonWithEmptyStringShouldSucceed()
        {
            // Arrange
            var value = "";
            var target = ConditionComparison.NotExists.ToUnaryDelegate();

            // Act
            target(value).ShouldBeTrue();
        }

        [Fact]
        public void NotExistsComparisonNonExistingValueShouldSucceed()
        {
            // Arrange
            string value = null;
            var target = ConditionComparison.NotExists.ToUnaryDelegate();

            // Act
            target(value).ShouldBeTrue();
        }
    }
}
