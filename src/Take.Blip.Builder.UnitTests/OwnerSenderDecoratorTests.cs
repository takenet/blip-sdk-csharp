using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using NSubstitute;
using Take.Blip.Client;
using Take.Blip.Client.Activation;
using Xunit;

namespace Take.Blip.Builder.UnitTests
{
    public class OwnerSenderDecoratorTests
    {
        private readonly ISender _sender;
        private readonly Application _applicationIdentity;
        private readonly OwnerSenderDecorator _ownerSenderDecorator;

        public OwnerSenderDecoratorTests()
        {
            _sender = Substitute.For<ISender>();
            _applicationIdentity = new Application { Identifier = "identity", Domain = "limeprotocol.org" };

            OwnerContext.Create(new Identity("owner", "limeprotocol.org"));

            _ownerSenderDecorator = new OwnerSenderDecorator(
                _sender,
                _applicationIdentity);
        }

        [Fact]
        public async Task ProcessCommandShouldSetFromAsOwner()
        {
            // Arrange
            var command = new Command("id")
            {
                Uri = new LimeUri("/uri"),
                To = "postmaster@limeprotocol.org"
            };

            // Act
            await _ownerSenderDecorator.ProcessCommandAsync(command, CancellationToken.None);

            // Assert
            await _sender
                .Received(1)
                .ProcessCommandAsync(
                    Arg.Is<Command>(c => c.From.Name == "owner" && c.From.Domain == "limeprotocol.org"),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessCommandShouldIgnoreOwner()
        {
            // Arrange
            var command = new Command("id")
            {
                Uri = new LimeUri("/uri"),
                To = "postmaster@limeprotocol.org",
                Metadata = new Dictionary<string, string>
                {
                    { $"builder.{Constants.IGNORE_OWNER_CONTEXT}", "True" }
                }
            };

            // Act
            await _ownerSenderDecorator.ProcessCommandAsync(command, CancellationToken.None);

            // Assert
            await _sender
                .Received(1)
                .ProcessCommandAsync(
                    Arg.Is<Command>(c => c.From == null),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessCommandShouldNotIgnoreOwner()
        {
            // Arrange
            var command = new Command("id")
            {
                Uri = new LimeUri("/uri"),
                To = "postmaster@limeprotocol.org",
                Metadata = new Dictionary<string, string>
                {
                    { $"builder.{Constants.IGNORE_OWNER_CONTEXT}", "False" }
                }
            };

            // Act
            await _ownerSenderDecorator.ProcessCommandAsync(command, CancellationToken.None);

            // Assert
            await _sender
                .Received(1)
                .ProcessCommandAsync(
                    Arg.Is<Command>(c => c.From.Name == "owner" && c.From.Domain == "limeprotocol.org"),
                    Arg.Any<CancellationToken>());
        }
    }
}
