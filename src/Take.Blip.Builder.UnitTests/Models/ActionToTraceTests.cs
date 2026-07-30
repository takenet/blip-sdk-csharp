using Shouldly;
using Take.Blip.Builder.Models;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Models
{
    public class ActionToTraceTests
    {
        [Fact]
        public void ToTrace_ShouldMapIdAndTitleToActionTrace()
        {
            var action = new Action
            {
                Id = "action-abc",
                Title = "My Action",
                Type = "SetVariable",
                Order = 1,
                ContinueOnError = true,
            };

            var trace = action.ToTrace();

            trace.ActionId.ShouldBe("action-abc");
            trace.ActionTitle.ShouldBe("My Action");
            trace.Order.ShouldBe(1);
            trace.Type.ShouldBe("SetVariable");
            trace.ContinueOnError.ShouldBeTrue();
        }

        [Fact]
        public void ToTrace_WhenIdAndTitleAreNull_ShouldReturnNullActionIdAndTitle()
        {
            var action = new Action { Type = "SetVariable" };

            var trace = action.ToTrace();

            trace.ActionId.ShouldBeNull();
            trace.ActionTitle.ShouldBeNull();
        }

        [Fact]
        public void ToTrace_ShouldNotCarrySettings()
        {
            var action = new Action { Id = "x", Type = "SetVariable" };

            var trace = action.ToTrace();

            trace.ParsedSettings.ShouldBeNull();
        }
    }
}
