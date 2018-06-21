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
                Values = new[] { "value" }
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
                ex.Message.ShouldBe("The condition values should be provided if comparison is not Exists or NotExists");
            }
        }

        [Fact]
        public void ValidateContextSourceWithoutVariableNameShouldFail()
        {
            // Arrange
            var condition = new Condition
            {
                Source = ValueSource.Context,
                Values = new[] { "value" }
            };

            // Act
            try
            {
                condition.Validate();
                throw new Exception("No validation exception thrown");
            }
            catch (ValidationException ex)
            {
                ex.Message.ShouldBe("The variable name should be provided if the comparsion source is context");
            }
        }

        [Fact]
        public void ValidateEntitySourceWithoutEntityNameShouldFail()
        {
            // Arrange
            var condition = new Condition
            {
                Source = ValueSource.Entity,
                Values = new[] { "value" }
            };

            // Act
            try
            {
                condition.Validate();
                throw new Exception("No validation exception thrown");
            }
            catch (ValidationException ex)
            {
                ex.Message.ShouldBe("The entity name should be provided if the comparsion source is entity");
            }
        }
    }
}
