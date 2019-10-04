using System;
using System.Threading.Tasks;
using Lime.Protocol;
using NSubstitute;
using Take.Blip.Builder.Actions.ForwardMessageToDesk;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.HelpDesk;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ForwardMessageToDeskActionTests : ActionTestsBase
    {
        public ForwardMessageToDeskActionTests()
        {
            Sender = Substitute.For<ISender>();
            Settings = new ForwardMessageToDeskSettings();
        }
        
        public ISender Sender { get; }

        public ForwardMessageToDeskSettings Settings { get; }
        
        private ForwardMessageToDeskAction GetTarget()
        {
            return new ForwardMessageToDeskAction(Sender);
        }

        [Fact]
        public async Task ExecuteShouldSendMessage()
        {
            // Arrange
            var target = GetTarget();
            
            // Act
            await target.ExecuteAsync(Context, Settings, CancellationToken);
            
            // Assert
            Sender.Received(1)
                .SendMessageAsync(Arg.Is<Message>(m =>
                    m.To.Name == Uri.EscapeDataString(From) &&
                    m.To.Domain == HelpDeskExtension.DEFAULT_DESK_DOMAIN &&
                    m.Content == Message.Content), CancellationToken);
        }
        
        [Fact]
        public async Task ExecuteWithTicketShouldSendMessageWithMetadata()
        {
            // Arrange
            var ticketId = Guid.NewGuid().ToString();
            Settings.TicketId = ticketId;
            var target = GetTarget();
            
            // Act
            await target.ExecuteAsync(Context, Settings, CancellationToken);
            
            // Assert
            Sender.Received(1)
                .SendMessageAsync(Arg.Is<Message>(m =>
                    m.To.Name == Uri.EscapeDataString(From) &&
                    m.To.Domain == HelpDeskExtension.DEFAULT_DESK_DOMAIN &&
                    m.Content == Message.Content &&
                    m.Metadata["desk.ticketId"] == ticketId), 
                    CancellationToken);
        }
    }
}