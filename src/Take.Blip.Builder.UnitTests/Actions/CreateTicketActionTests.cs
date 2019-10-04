using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Take.Blip.Builder.Actions.CreateTicket;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.HelpDesk;
using Takenet.Iris.Messaging.Resources;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class CreateTicketActionTests : ActionTestsBase
    {
        public CreateTicketActionTests()
        {
            HelpDeskExtension = Substitute.For<IHelpDeskExtension>();
            Application = new Application()
            {
                Identifier = OwnerIdentity.Name,
                Domain = OwnerIdentity.Domain
            };
            Settings = new CreateTicketSettings();
        }
        
        public Application Application { get; }

        public IHelpDeskExtension HelpDeskExtension { get; }

        public CreateTicketSettings Settings { get; }
        
        private CreateTicketAction GetTarget()
        {
            return new CreateTicketAction(HelpDeskExtension, Application);
        }

        [Fact]
        public async Task ExecuteShouldCallExtension()
        {
            // Arrange
            var target = GetTarget();
            
            // Act
            await target.ExecuteAsync(Context, Settings, CancellationToken);
            
            // Assert
            HelpDeskExtension.Received(1)
                .CreateTicketAsync(Arg.Is<Ticket>(t => t.CustomerIdentity == UserIdentity && t.OwnerIdentity == OwnerIdentity), CancellationToken);
        }
        
        [Fact]
        public async Task ExecuteWithVariableShouldSetOnContext()
        {
            // Arrange
            var ticketId = Guid.NewGuid().ToString();
            var ticket = new Ticket()
            {
                Id = ticketId
            };
            Settings.Variable = "myTicketId";
            HelpDeskExtension.CreateTicketAsync(Arg.Any<Ticket>(), Arg.Any<CancellationToken>()).Returns(ticket);
            var target = GetTarget();
            
            // Act
            await target.ExecuteAsync(Context, Settings, CancellationToken);
            
            // Assert
            HelpDeskExtension.Received(1)
                .CreateTicketAsync(Arg.Is<Ticket>(t => t.CustomerIdentity == UserIdentity && t.OwnerIdentity == OwnerIdentity), CancellationToken);
            Context.SetVariableAsync(Settings.Variable, ticketId, CancellationToken, default);
        }
    }
}