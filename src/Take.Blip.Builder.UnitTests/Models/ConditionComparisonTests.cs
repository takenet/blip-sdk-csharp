using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Take.Blip.Builder.Models;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Models
{
    public class ConditionComparisonTests
    {
        [Fact]
        public void EqualsComparisonWithEqualsValuesShouldSuccceed()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Value 1";
            var target = ConditionComparison.Equals.ToDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void EqualsComparisonWithEqualsValuesWithDifferentCasingShouldSuccceed()
        {
            // Arrange
            var value1 = "ValUe 1";
            var value2 = "value 1";
            var target = ConditionComparison.Equals.ToDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void EqualsComparisonWithEqualsValuesWithEmojiShouldSuccceed()
        {
            // Arrange
            var value1 = "Value 😱🤢➡";
            var value2 = "Value 😱🤢➡";
            var target = ConditionComparison.Equals.ToDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }
    
        [Fact]
        public void EqualsComparisonWithEqualsValuesWithAccentShouldSuccceed()
        {
            // Arrange
            var value1 = "Valuê 1";
            var value2 = "Válue 1";
            var target = ConditionComparison.Equals.ToDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }
        [Fact]
        public void EqualsComparisonWithUnequalsValuesShouldFail()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Value X";
            var target = ConditionComparison.Equals.ToDelegate();

            // Act
            target(value1, value2).ShouldBeFalse();
        }

        [Fact]
        public void NotEqualsComparisonWithEqualsValuesShouldFail()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Value 1";
            var target = ConditionComparison.NotEquals.ToDelegate();

            // Act
            target(value1, value2).ShouldBeFalse();
        }

        [Fact]
        public void NotEqualsComparisonWithEqualsValuesWithDifferentCasingShouldFail()
        {
            // Arrange
            var value1 = "ValUe 1";
            var value2 = "value 1";
            var target = ConditionComparison.NotEquals.ToDelegate();

            // Act
            target(value1, value2).ShouldBeFalse();
        }

        [Fact]
        public void NotEqualsComparisonWithEqualsValuesWithAccentShouldFail()
        {
            // Arrange
            var value1 = "Valuê 1";
            var value2 = "Válue 1";
            var target = ConditionComparison.NotEquals.ToDelegate();

            // Act
            target(value1, value2).ShouldBeFalse();
        }
        [Fact]
        public void NotEqualsComparisonWithUnequalsValuesShouldSucceed()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Value X";
            var target = ConditionComparison.NotEquals.ToDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void StartsWithComparisonWithEqualsValuesShouldSuccceed()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Value 1";
            var target = ConditionComparison.StartsWith.ToDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void StartsWithComparisonWithEqualsValuesWithDifferentCasingShouldSuccceed()
        {
            // Arrange
            var value1 = "ValUe 1";
            var value2 = "value 1";
            var target = ConditionComparison.StartsWith.ToDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void StartsWithComparisonWithPartialValuesShouldSuccceed()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Valu";
            var target = ConditionComparison.StartsWith.ToDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void StartsWithComparisonWithInvalidValuesShouldFail()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Balue";
            var target = ConditionComparison.StartsWith.ToDelegate();

            // Act
            target(value1, value2).ShouldBeFalse();
        }


        [Fact]
        public void ApproximateToComparisonWithApproximateToValuesShouldSuccceed()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Value 1";
            var target = ConditionComparison.ApproximateTo.ToDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void ApproximateToComparisonWithApproximateToValuesWithDifferentCasingShouldSuccceed()
        {
            // Arrange
            var value1 = "ValUe 1";
            var value2 = "value 1";
            var target = ConditionComparison.ApproximateTo.ToDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void ApproximateToComparisonWithApproximateToValuesWithAccentShouldSuccceed()
        {
            // Arrange
            var value1 = "Valuê 1";
            var value2 = "Válue 1";
            var target = ConditionComparison.ApproximateTo.ToDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void ApproximateToComparisonWithSlightlyUnequalsValuesShouldSucceed()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Vilue X";
            var target = ConditionComparison.ApproximateTo.ToDelegate();

            // Act
            target(value1, value2).ShouldBeTrue();
        }

        [Fact]
        public void ApproximateToComparisonWithCompletelyUnequalsValuesShouldFail()
        {
            // Arrange
            var value1 = "Value 1";
            var value2 = "Hello world";
            var target = ConditionComparison.ApproximateTo.ToDelegate();

            // Act
            target(value1, value2).ShouldBeFalse();
        }
    }
}
