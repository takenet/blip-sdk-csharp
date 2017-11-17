using System;
using System.ComponentModel.DataAnnotations;
using Shouldly;
using Take.Blip.Builder.Models;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Models
{
    public class StateTests
    {
        [Fact]
        public void ValidateValidStateShouldSucceed()
        {
            // Arrange
            var state = new State
            {
                Id = "0"
            };

            // Act 
            state.Validate();
        }

        [Fact]
        public void ValidateWithoutIdShouldFail()
        {
            // Arrange
            var state = new State
            {
                Root = true,
                Input = new Input()
            };

            // Act
            try
            {
                state.Validate();
                throw new Exception("No validation exception thrown");
            }
            catch (ValidationException ex)
            {
                ex.Message.ShouldBe("The state id is required");
            }
        }
    }
}
