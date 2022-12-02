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
        
        public IForwardToDesk ForwardToDesk { get; }
        public IStateManager StateManager { get; }
        public ILogger Logger { get; }


        public ForwardToDeskSettings Settings { get; }
        
        private ForwardToDeskAction GetTarget()
        {
            return new ForwardToDeskAction(ForwardToDesk, StateManager, Logger);
        }

        [Fact]
        public async Task ExecuteShouldSendMessage()
        {
            // Arrange
            var target = GetTarget();
            IContext context = Substitute.For<IContext>();
            ForwardToDeskSettings forwardToDeskSettings = new ForwardToDeskSettings();
            ForwardToDesk.GetOrCreateTicketAsync(Context, Settings, CancellationToken).Returns(true);

            // Act
            await target.ExecuteAsync(Context, Settings, CancellationToken);

            // Assert
            _ = ForwardToDesk.Received(1)
                .GetOrCreateTicketAsync(Context, Settings, CancellationToken);
            _ = Logger.Received(0);
            _ = ForwardToDesk.Received(1)
                .SendMessageAsync(Context, Settings, CancellationToken);
        }

        [Fact]
        public async Task ExecuteShouldLogMessage()
        {
            // Arrange
            var target = GetTarget();
            IContext context = Substitute.For<IContext>();
            ForwardToDeskSettings forwardToDeskSettings = new ForwardToDeskSettings();
            ForwardToDesk.GetOrCreateTicketAsync(Context, Settings, CancellationToken).Returns(false);

            // Act
            await target.ExecuteAsync(Context, Settings, CancellationToken);

            // Assert
            _ = ForwardToDesk.Received(1)
                .GetOrCreateTicketAsync(Context, Settings, CancellationToken);
            Logger.Received(1)
                .Information("[Desk-State] Cannot get or create ticket to send message to Desk for UserIdentity {UserIdentity} from OwnerIdentity {OwnerIdentity}.", 
                    Context.UserIdentity, 
                    Context.OwnerIdentity);
            _ = ForwardToDesk.Received(1)
                .SendMessageAsync(Context, Settings, CancellationToken);
        }
    }
}
