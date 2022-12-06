using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Serilog;
using Take.Blip.Builder.Actions.ForwardMessageToDesk;
using Take.Blip.Builder.Actions.ForwardToDesk;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ForwardToDeskActionTests : ActionTestsBase
    {
        public ForwardToDeskActionTests()
        {
            ForwardToDesk = Substitute.For<IForwardToDesk>();
            StateManager =Substitute.For<IStateManager>();
            Logger = Substitute.For<ILogger>();
            Settings = new ForwardToDeskSettings();
        }
        
        private IForwardToDesk ForwardToDesk { get; }
        private IStateManager StateManager { get; }
        private ILogger Logger { get; }


        public ForwardToDeskSettings Settings { get; }
        
        private ForwardToDeskAction GetTarget()
        {
            return new ForwardToDeskAction(ForwardToDesk);
        }

        [Fact]
        public async Task ExecuteShouldSendMessage()
        {
            // Arrange
            var target = GetTarget();
            ForwardToDesk.GetOrCreateTicketAsync(Context, Settings, CancellationToken).Returns(true);

            // Act
            await target.ExecuteAsync(Context, Settings, CancellationToken);

            // Assert
            _ = ForwardToDesk.Received(1)
                .SendMessageAsync(Context, Settings, CancellationToken);
        }
    }
}
