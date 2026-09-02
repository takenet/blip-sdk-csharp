using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Take.Blip.Builder.Models;
using Xunit;

namespace Take.Blip.Builder.UnitTests
{
    public class StateManagerTests
    {
        private readonly IContext _context;
        private readonly StateManager _target;
        private readonly Flow _flow;

        public StateManagerTests()
        {
            _flow = new Flow
            {
                Id = Guid.NewGuid().ToString(),
                BuilderConfiguration = new BuilderConfiguration
                {
                    StateExpiration = TimeSpan.FromHours(24)
                }
            };

            _context = Substitute.For<IContext>();
            _context.Flow.Returns(_flow);

            _target = new StateManager();
        }

        [Fact]
        public async Task RenewStateExpirationAsync_WhenStateExists_ShouldRenewWithSameValueAndConfiguredExpiration()
        {
            // Arrange
            const string stateId = "subflow:currentState";
            var stateKey = $"stateId@{_flow.Id}";
            _context
                .GetContextVariableAsync(stateKey, Arg.Any<CancellationToken>())
                .Returns(stateId);

            // Act
            await _target.RenewStateExpirationAsync(_context, CancellationToken.None);

            // Assert
            await _context
                .Received(1)
                .SetVariableAsync(stateKey, stateId, Arg.Any<CancellationToken>(), _flow.BuilderConfiguration.StateExpiration.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task RenewStateExpirationAsync_WhenStateDoesNotExist_ShouldNotSetVariable(string storedStateId)
        {
            // Arrange
            var stateKey = $"stateId@{_flow.Id}";
            _context
                .GetContextVariableAsync(stateKey, Arg.Any<CancellationToken>())
                .Returns(storedStateId);

            // Act
            await _target.RenewStateExpirationAsync(_context, CancellationToken.None);

            // Assert
            await _context
                .DidNotReceive()
                .SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<TimeSpan>());
        }
    }
}
