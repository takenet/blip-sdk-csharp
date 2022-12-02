using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Take.Blip.Builder.Actions.ForwardMessageToDesk;
using Take.Blip.Builder.Actions.LeavingFromDesk;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class LeavingFromDeskActionTests : ActionTestsBase
    {
        public LeavingFromDeskActionTests()
        {
            LeavingFromDesk = Substitute.For<ILeavingFromDesk>();
            Settings = new LeavingFromDeskSettings();
        }
        
        public ILeavingFromDesk LeavingFromDesk { get; }

        public LeavingFromDeskSettings Settings { get; }
        
        private LeavingFromDeskAction GetTarget()
        {
            return new LeavingFromDeskAction(LeavingFromDesk);
        }

        [Fact]
        public async Task ExecuteShouldSendMessage()
        {
            // Arrange
            var target = GetTarget();
            LeavingFromDeskSettings leavingFromDeskSettings = new LeavingFromDeskSettings();

            // Act
            await target.ExecuteAsync(Context, Settings, CancellationToken);

            // Assert
            _ = LeavingFromDesk.Received(1)
                .CloseOpenedTicketsAsync(Context, Settings, CancellationToken);
        }
    }
}
