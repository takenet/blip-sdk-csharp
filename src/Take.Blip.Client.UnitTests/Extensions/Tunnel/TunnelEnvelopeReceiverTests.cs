using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using NSubstitute;
using Take.Blip.Client.Extensions.Tunnel;

using Xunit;
#pragma warning disable 4014

namespace Take.Blip.Client.UnitTests.Extensions.Tunnel
{
    public class TunnelEnvelopeReceiverTests : TestsBase
    {
        public ISender Sender { get; } = Substitute.For<ISender>();
       
        public TunnelEnvelopeReceiver<Message> GetMessageTarget() => new TunnelEnvelopeReceiver<Message>(Sender.SendMessageAsync);

        [Fact]
        public async Task ReceiveValidTunnelEnvelopeShouldForwardToOriginator()
        {
            // Arrange
            var message = new Message
            {
                From = "children-bot@tunnel.msging.net/originator%40domain.local%2Finstance",
                To = "master-bot@msging.net",
                Content = "Hello"
            };

            var target = GetMessageTarget();

            // Act
            await target.ReceiveAsync(message, CancellationToken);

            // Assert
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.To == "originator@domain.local/instance"
                        && m.From == null
                        && m.Content == message.Content),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ReceiveInvalidTunnelEnvelopeShouldThrowArgumentException()
        {
            // Arrange
            var message = new Message
            {
                From = "originator@domain.local/instance",
                To = "master-bot@msging.net",
                Content = "Hello"
            };

            var target = GetMessageTarget();

            // Act
            Assert.ThrowsAsync<ArgumentException>(async () => await target.ReceiveAsync(message, CancellationToken));
        }
    }
}
