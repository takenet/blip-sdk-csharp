using System;
using System.ComponentModel.DataAnnotations;
using Shouldly;
using Take.Blip.Builder.Models;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Models
{
    public class ConditionTests
    {
        [Fact]
        public void ValidateValidConditionShouldSucceed()
        {
            // Arrange
            var condition = new Condition
            {
                Source = ValueSource.Context,
                Variable = "variable",
                Value = "value"
            };

            // Act
            condition.Validate();
        }

        [Fact]
        public void ValidateWithoutValueShouldFail()
        {
            // Arrange
            var condition = new Condition
            {
                Source = ValueSource.Context,
                Variable = "variable"
            };

            // Act
            try
            {
                condition.Validate();
                throw new Exception("No validation exception thrown");
            }
            catch (ValidationException ex)
            {
                ex.Message.ShouldBe("The condition value is required");
            }
        }


        [Fact]
        public void ValidateContextSourceWithoutVariableNameShouldFail()
        {
            // Arrange
            var condition = new Condition
            {
                Source = ValueSource.Context,
                Value = "value"
            };

            // Act
            try
            {
                condition.Validate();
                throw new Exception("No validation exception thrown");
            }
            catch (ValidationException ex)
            {
                ex.Message.ShouldBe("The variable should be provided if the comparsion source is context");
            }
        }
    }
}
