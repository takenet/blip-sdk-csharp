using System;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Serilog;
using Shouldly;
using Take.Blip.Builder.Variables;
using Take.Blip.Client.Extensions.HelpDesk;
using Takenet.Iris.Messaging.Resources;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Variables
{
    public class TicketVariableProviderTests : ContextTestsBase
    {
        public TicketVariableProviderTests()
        {
            HelpDeskExtension = Substitute.For<IHelpDeskExtension>();
            Logger = Substitute.For<ILogger>();
        }
        
        public IHelpDeskExtension HelpDeskExtension { get; }

        public ILogger Logger { get; }

        private TicketVariableProvider GetTarget()
        {
            return new TicketVariableProvider(HelpDeskExtension, Logger);
        }

        [Fact]
        public async Task GetExistingVariablesShouldSucceed()
        {
            // Arrange
            var ticket = new Ticket()
            {
                Id = Guid.NewGuid()
                    .ToString(),
                OwnerIdentity = "owner@msging.net",
                CustomerIdentity = "customer@msging.net"
            };
            HelpDeskExtension.GetCustomerActiveTicketAsync(UserIdentity, CancellationToken).Returns(ticket);
            var target = GetTarget();
            
            // Act
            var actualTicketId = await target.GetVariableAsync("id", Context, CancellationToken);
            var actualOwnerIdentity = await target.GetVariableAsync("ownerIdentity", Context, CancellationToken);
            var actualCustomerIdentity = await target.GetVariableAsync("customerIdentity", Context, CancellationToken);
            
            // Assert
            actualTicketId.ShouldBe(ticket.Id);
            actualOwnerIdentity.ShouldBe(ticket.OwnerIdentity);
            actualCustomerIdentity.ShouldBe(ticket.CustomerIdentity);
        }
        
        [Fact]
        public async Task GetWithNoTicketShouldReturnNull()
        {
            // Arrange
            Ticket ticket = null;
            HelpDeskExtension.GetCustomerActiveTicketAsync(UserIdentity, CancellationToken)
                .Throws(new LimeException(ReasonCodes.COMMAND_RESOURCE_NOT_FOUND, "Not found"));
            var target = GetTarget();
            
            // Act
            var actual = await target.GetVariableAsync("id", Context, CancellationToken);

            // Assert
            actual.ShouldBeNull();
        }
        
        [Fact]
        public async Task GetNullPropertyShouldReturnNull()
        {
            // Arrange
            var ticket = new Ticket()
            {
                Id = null
            };
            HelpDeskExtension.GetCustomerActiveTicketAsync(UserIdentity, CancellationToken).Returns(ticket);
            var target = GetTarget();
            
            // Act
            var actual = await target.GetVariableAsync("id", Context, CancellationToken);

            // Assert
            actual.ShouldBeNull();
        }
        
        [Fact]
        public async Task GetNonExistingPropertyShouldReturnNull()
        {
            // Arrange
            var ticket = new Ticket()
            {
                Id = Guid.NewGuid()
                    .ToString(),
                OwnerIdentity = "owner@msging.net",
                CustomerIdentity = "customer@msging.net"
            };
            HelpDeskExtension.GetCustomerActiveTicketAsync(UserIdentity, CancellationToken).Returns(ticket);
            var target = GetTarget();
            
            // Act
            var actual = await target.GetVariableAsync("doesNotExists", Context, CancellationToken);

            // Assert
            actual.ShouldBeNull();
        }
    }
}