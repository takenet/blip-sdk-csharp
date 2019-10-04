using System.Threading.Tasks;
using Lime.Protocol;
using NSubstitute;
using Shouldly;
using Take.Blip.Builder.Models;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.Tunnel;
using Takenet.Iris.Messaging.Resources;
using Xunit;

namespace Take.Blip.Builder.UnitTests
{
    public class UserOwnerResolverTests : CancellationTokenTestsBase
    {
        public UserOwnerResolverTests()
        {
            TunnelExtension = Substitute.For<ITunnelExtension>();
            UserIdentity = new Identity("user", "domain");
            ApplicationIdentity = new Identity("application", "domain");
            Application = new Application()
            {
                Identifier = ApplicationIdentity.Name,
                Domain = ApplicationIdentity.Domain
            };
            Message = new Message()
            {
                From = UserIdentity.ToNode(),
                To = ApplicationIdentity.ToNode()
            };
            BuilderConfiguration = new BuilderConfiguration();
            TunnelIdentity = new Identity(EnvelopeId.NewId(), Take.Blip.Client.Extensions.Tunnel.TunnelExtension.TunnelAddress.Domain);
            TunnelOwner = new Identity("owner", "domain");
            TunnelExtension
                .GetTunnelAsync(TunnelIdentity, CancellationToken)
                .Returns(new Tunnel()
                {
                    Owner = TunnelOwner,
                    Originator = UserIdentity.ToNode(),
                    Destination = ApplicationIdentity
                });
        }
        
        public ITunnelExtension TunnelExtension { get; }
        
        public Application Application { get; set; }
        
        public Identity UserIdentity { get; set; }

        public Identity ApplicationIdentity { get; set; }

        public Message Message { get; set; }

        public BuilderConfiguration BuilderConfiguration { get; }

        public Identity TunnelIdentity { get; }
        
        public Identity TunnelOwner { get; }
        
        
        private UserOwnerResolver GetTarget()
        {
            return new UserOwnerResolver(TunnelExtension, Application);
        }

        [Fact]
        public async Task GetFromMessageAndApplicationShouldSucceed()
        {
            // Arrange
            var target = GetTarget();
            
            // Act
            var actual = await target.GetUserOwnerIdentitiesAsync(Message, BuilderConfiguration, CancellationToken);
            
            // Assert
            actual.ShouldNotBeNull();
            actual.UserIdentity.ShouldBe(UserIdentity);
            actual.OwnerIdentity.ShouldBe(ApplicationIdentity);
        }
        
        [Fact]
        public async Task GetFromMessageAndApplicationShouldSucceedEvenInATunnel()
        {
            // Arrange
            Message.From = TunnelIdentity.ToNode();
            var target = GetTarget();
            
            // Act
            var actual = await target.GetUserOwnerIdentitiesAsync(Message, BuilderConfiguration, CancellationToken);
            
            // Assert
            actual.ShouldNotBeNull();
            actual.UserIdentity.ShouldBe(TunnelIdentity);
            actual.OwnerIdentity.ShouldBe(ApplicationIdentity);
        }
        
        [Fact]
        public async Task GetFromTunnelShouldSucceed()
        {
            // Arrange
            Message.From = TunnelIdentity.ToNode();
            BuilderConfiguration.UseTunnelOwnerContext = true;
            var target = GetTarget();
            
            
            // Act
            var actual = await target.GetUserOwnerIdentitiesAsync(Message, BuilderConfiguration, CancellationToken);
            
            // Assert
            actual.ShouldNotBeNull();
            actual.UserIdentity.ShouldBe(UserIdentity);
            actual.OwnerIdentity.ShouldBe(TunnelOwner);
        }
    }
}
