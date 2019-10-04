using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using NSubstitute;
using Shouldly;
using Take.Blip.Builder.Variables;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Variables
{
    public class CalendarVariableProviderTests : ContextTestsBase
    {
        public CalendarVariableProviderTests()
        {
            Identity = new Identity("name", "domain.com");
            Context.UserIdentity.Returns(Identity);
        }

        public CalendarVariableProvider GetTarget()
        {
            return new CalendarVariableProvider();
        }

        public Identity Identity { get; }
        [Fact]
        public async Task GetDateShouldReturnTodayDate()
        {
            // Arrange
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("date", Context, CancellationToken);

            // Assert
            actual.ShouldBe(DateTime.UtcNow.Date.ToString("yyyy-MM-dd"));
        }

        [Fact]
        public async Task GetTodayDateShouldReturnTodayDate()
        {
            // Arrange
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("today.date", Context, CancellationToken);

            // Assert
            actual.ShouldBe(DateTime.UtcNow.Date.ToString("yyyy-MM-dd"));
        }

        [Fact]
        public async Task GetTomorrowDateShouldReturnTomorrowDate()
        {
            // Arrange
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("tomorrow.date", Context, CancellationToken);

            // Assert
            actual.ShouldBe(DateTime.UtcNow.Date.AddDays(1).ToString("yyyy-MM-dd"));
        }

        [Fact]
        public async Task GetYesterdayDateShouldReturnYesterdayDate()
        {
            // Arrange
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("yesterday.date", Context, CancellationToken);

            // Assert
            actual.ShouldBe(DateTime.UtcNow.Date.AddDays(-1).ToString("yyyy-MM-dd"));
        }

        [Fact]
        public async Task GetTodayDatePlus5DaysShouldReturnTheCorrectDate()
        {
            // Arrange
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("plus5days.date", Context, CancellationToken);

            // Assert
            actual.ShouldBe(DateTime.UtcNow.Date.AddDays(5).ToString("yyyy-MM-dd"));
        }

        [Fact]
        public async Task GetTodayDateMinus5DaysShouldReturnTheCorrectDate()
        {
            // Arrange
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("minus5days.date", Context, CancellationToken);

            // Assert
            actual.ShouldBe(DateTime.UtcNow.Date.AddDays(-5).ToString("yyyy-MM-dd"));
        }

        [Fact]
        public async Task GetTomorrowDatePlus5DaysShouldReturnTheCorrectDate()
        {
            // Arrange
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("tomorrow.plus5days.date", Context, CancellationToken);

            // Assert
            actual.ShouldBe(DateTime.UtcNow.Date.AddDays(6).ToString("yyyy-MM-dd"));
        }
    }
}
