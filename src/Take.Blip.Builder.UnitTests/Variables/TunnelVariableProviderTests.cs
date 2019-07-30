using System.Collections.Generic;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Variables;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Take.Blip.Client.Extensions.Tunnel;
using Takenet.Iris.Messaging.Resources;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Variables
{
    public class TunnelVariableProviderTests : CancellationTokenTestsBase
    {
        public TunnelVariableProviderTests()
        {
            TunnelExtension = Substitute.For<ITunnelExtension>();
            Context = Substitute.For<IContext>();
            BuilderConfiguration = new BuilderConfiguration();
            Message = new Message();
            LazyInput = new LazyInput(
                Message, 
                BuilderConfiguration, 
                Substitute.For<IDocumentSerializer>(),
                Substitute.For<IEnvelopeSerializer>(),
                Substitute.For<IArtificialIntelligenceExtension>(),
                CancellationToken);
            Context.Input.Returns(LazyInput);
            Owner = new Identity("owner", "msging.net");
            Originator = new Node("originator", "msging.net", "instance");
            TunnelIdentity = new Identity(EnvelopeId.NewId(), Take.Blip.Client.Extensions.Tunnel.TunnelExtension.TunnelAddress.Domain);
            ApplicationIdentity = new Identity("application", "msging.net");
            Message.To = ApplicationIdentity.ToNode();
            Tunnel = new Tunnel()
            {
                Owner = Owner,
                Originator = Originator,
                Destination = ApplicationIdentity
            };
        }

        public ITunnelExtension TunnelExtension { get; }
        
        public IContext Context { get; }
        
        public LazyInput LazyInput { get; }

        public BuilderConfiguration BuilderConfiguration { get; }
        
        public Message Message { get; }

        public Identity Owner { get;  }

        public Node Originator { get;  }

        public Identity TunnelIdentity { get; }

        public Tunnel Tunnel { get; }
        
        public Identity ApplicationIdentity { get; }
        
        private TunnelVariableProvider GetTarget()
        {
            return new TunnelVariableProvider(TunnelExtension);
        }

        [Fact]
        public async Task GetFromMessageShouldSucceed()
        {
            // Arrange
            Message.From = TunnelIdentity.ToNode();
            Message.Metadata = new Dictionary<string, string>
            {
                {Take.Blip.Client.Extensions.Tunnel.TunnelExtension.TUNNEL_OWNER_METADATA_KEY, Owner},
                {Take.Blip.Client.Extensions.Tunnel.TunnelExtension.TUNNEL_ORIGINATOR_METADATA_KEY, Originator}
            };
            
            var target = GetTarget();

            // Act
            var actualOriginator = await target.GetVariableAsync("originator", Context, CancellationToken);
            var actualOwner = await target.GetVariableAsync("owner", Context, CancellationToken);
            var actualDestination = await target.GetVariableAsync("destination", Context, CancellationToken);
            var actualIdentity = await target.GetVariableAsync("identity", Context, CancellationToken);
            
            // Assert
            actualOriginator.ShouldBe(Identity.Parse(Originator).ToString());
            actualOwner.ShouldBe(Owner);
            actualDestination.ShouldBe(ApplicationIdentity);
            actualIdentity.ShouldBe(TunnelIdentity);
        }

        [Fact]
        public async Task GetFromIncompleteMetadataShouldReturnNull()
        {
            // Arrange
            Message.From = TunnelIdentity.ToNode();
            Message.Metadata = new Dictionary<string, string>
            {
                {Take.Blip.Client.Extensions.Tunnel.TunnelExtension.TUNNEL_OWNER_METADATA_KEY, Owner},
            };

            var target = GetTarget();

            // Act
            var actualOriginator = await target.GetVariableAsync("originator", Context, CancellationToken);
            var actualOwner = await target.GetVariableAsync("owner", Context, CancellationToken);
            var actualDestination = await target.GetVariableAsync("destination", Context, CancellationToken);
            var actualIdentity = await target.GetVariableAsync("identity", Context, CancellationToken);
            
            // Assert
            actualOriginator.ShouldBeNull();
            actualOwner.ShouldBeNull();
            actualDestination.ShouldBeNull();
            actualIdentity.ShouldBeNull();
        }        
        
        [Fact]
        public async Task GetFromNonTunnelSenderShouldReturnNull()
        {
            // Arrange
            Message.From = new Node(EnvelopeId.NewId(), "not.tunnel.net", "instance");
            Message.Metadata = new Dictionary<string, string>
            {
                {Take.Blip.Client.Extensions.Tunnel.TunnelExtension.TUNNEL_OWNER_METADATA_KEY, Owner},
                {Take.Blip.Client.Extensions.Tunnel.TunnelExtension.TUNNEL_ORIGINATOR_METADATA_KEY, Originator}
            };

            var target = GetTarget();

            // Act
            var actualOriginator = await target.GetVariableAsync("originator", Context, CancellationToken);
            var actualOwner = await target.GetVariableAsync("owner", Context, CancellationToken);
            var actualDestination = await target.GetVariableAsync("destination", Context, CancellationToken);
            var actualIdentity = await target.GetVariableAsync("identity", Context, CancellationToken);
            
            // Assert
            actualOriginator.ShouldBeNull();
            actualOwner.ShouldBeNull();
            actualDestination.ShouldBeNull();
            actualIdentity.ShouldBeNull();
        }    
        
        [Fact]
        public async Task GetFromExtensionShouldSucceed()
        {
            // Arrange
            Message.From = TunnelIdentity.ToNode();
            TunnelExtension.GetTunnelAsync(TunnelIdentity, CancellationToken).Returns(Tunnel);
            
            var target = GetTarget();

            // Act
            var actualOriginator = await target.GetVariableAsync("originator", Context, CancellationToken);
            var actualOwner = await target.GetVariableAsync("owner", Context, CancellationToken);
            var actualDestination = await target.GetVariableAsync("destination", Context, CancellationToken);
            var actualIdentity = await target.GetVariableAsync("identity", Context, CancellationToken);
            
            // Assert
            actualOriginator.ShouldBe(Originator);
            actualOwner.ShouldBe(Owner);
            actualDestination.ShouldBe(ApplicationIdentity);
            actualIdentity.ShouldBe(TunnelIdentity);
        }
        
        [Fact]
        public async Task GetFromExtensionWhenDoesNotExistsShouldReturnNull()
        {
            // Arrange
            Message.From = TunnelIdentity.ToNode();
            TunnelExtension.GetTunnelAsync(TunnelIdentity, CancellationToken)
                .Throws(new LimeException(ReasonCodes.COMMAND_RESOURCE_NOT_FOUND, "Not found"));
            
            var target = GetTarget();

            // Act
            var actualOriginator = await target.GetVariableAsync("originator", Context, CancellationToken);
            var actualOwner = await target.GetVariableAsync("owner", Context, CancellationToken);
            var actualDestination = await target.GetVariableAsync("destination", Context, CancellationToken);
            var actualIdentity = await target.GetVariableAsync("identity", Context, CancellationToken);
            
            // Assert
            actualOriginator.ShouldBeNull();
            actualOwner.ShouldBeNull();
            actualDestination.ShouldBeNull();
            actualIdentity.ShouldBeNull();
        }
    }
}