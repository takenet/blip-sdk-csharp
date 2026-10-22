using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Take.Blip.Builder.Models;
using Xunit;

namespace Take.Blip.Builder.UnitTests
{
    public class StateManagerTests
    {
        private static readonly TimeSpan StateExpiration = TimeSpan.FromSeconds(10);

        private readonly StateManager _target = new StateManager();
        private readonly IContext _context = Substitute.For<IContext>();

        public StateManagerTests()
        {
            _context.Flow = CreateFlow(StateExpiration.ToString());
        }

        private static Flow CreateFlow(string stateExpiration)
        {
            var flow = new Flow { Id = "flow-id" };
            if (stateExpiration != null)
            {
                flow.Configuration = new Dictionary<string, string>
                {
                    { "builder:stateExpiration", stateExpiration }
                };
            }

            return flow;
        }

        [Theory]
        [InlineData("desk:ed0ff839-c619-47a2-9991-83776a4e3e6d")]
        [InlineData("DESK:ed0ff839")]
        public async Task SetStateIdAsyncShouldNotApplyExpirationForDeskState(string stateId)
        {
            // Act
            await _target.SetStateIdAsync(_context, stateId, CancellationToken.None);

            // Assert
            await _context.Received(1).SetVariableAsync(
                "stateId@flow-id",
                stateId,
                Arg.Any<CancellationToken>(),
                default(TimeSpan));
        }

        [Theory]
        [InlineData("onboarding")]
        [InlineData("fallback")]
        [InlineData("subflow:subflowTest")]
        public async Task SetStateIdAsyncShouldApplyExpirationForNonDeskState(string stateId)
        {
            // Act
            await _target.SetStateIdAsync(_context, stateId, CancellationToken.None);

            // Assert
            await _context.Received(1).SetVariableAsync(
                "stateId@flow-id",
                stateId,
                Arg.Any<CancellationToken>(),
                StateExpiration);
        }

        [Fact]
        public async Task SetStateIdAsyncShouldUseDefaultExpirationWhenNotConfigured()
        {
            // Arrange
            _context.Flow = CreateFlow(null);

            // Act
            await _target.SetStateIdAsync(_context, "onboarding", CancellationToken.None);

            // Assert
            await _context.Received(1).SetVariableAsync(
                "stateId@flow-id",
                "onboarding",
                Arg.Any<CancellationToken>(),
                default(TimeSpan));
        }
    }
}
