using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using NSubstitute;
using Serilog;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Variables;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.Contacts;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Variables
{
    public class ContactVariableProviderTests : ContextTestsBase
    {

        public ContactVariableProviderTests()
        {
            ContactExtension = Substitute.For<IContactExtension>();
            Logger = Substitute.For<ILogger>();
            Configuration = Substitute.For<IConfiguration>();
            Application = Substitute.For<Application>();
            InputContext = new Dictionary<string, object>();
            Context.InputContext.Returns(InputContext);
            Contact = new Contact()
            {
                Identity = "john@domain.com",
                Name = "John Doe",
                Address = "184 Alpha Avenue"
            };
            ContactExtension.GetAsync(Contact.Identity, CancellationToken).Returns(Contact);
            Context.UserIdentity.Returns(Contact.Identity);
            Context.OwnerIdentity.Returns(new Identity("application", "domain.com"));
            Application.Identifier = "application";
            Application.Domain = "domain.com";
            DocumentSerializer = new DocumentSerializer(new DocumentTypeResolver());
        }

        public IContactExtension ContactExtension { get; }
        
        public ILogger Logger { get; }
        
        public IConfiguration Configuration { get; }
        
        public Application Application { get; }
        
        public IDictionary<string, object> InputContext { get; }

        public Contact Contact { get; }

        public IDocumentSerializer DocumentSerializer { get; }

        public ContactVariableProvider GetTarget()
        {
            return new ContactVariableProvider(ContactExtension, DocumentSerializer, Logger);
        }

        [Fact]
        public async Task GetSerializedContact()
        {
            // Arrange
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("serialized", Context, CancellationToken);

            // Asset
            actual.ShouldBe(DocumentSerializer.Serialize(Contact));
        }

        [Fact]
        public async Task GetNameWhenContactExistsShouldReturnName()
        {
            // Arrange
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("name", Context, CancellationToken);

            // Asset
            actual.ShouldBe(Contact.Name);
        }
        
        [Fact]
        public async Task GetExtrasPropertyShouldReturnsValue()
        {
            // Arrange
            Contact.Extras = new Dictionary<string, string>()
            {
                ["extra1"] = "value1"
            };
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("extras.extra1", Context, CancellationToken);

            // Asset
            actual.ShouldBe(Contact.Extras["extra1"]);
        }

        [Fact]
        public async Task GetNameWhenContactDoesNotExistsShouldReturnNull()
        {
            // Arrange
            var target = GetTarget();

            Contact nullContact = null;
            ContactExtension.GetAsync(Arg.Any<Identity>(), Arg.Any<CancellationToken>()).Returns(nullContact);

            // Act
            var actual = await target.GetVariableAsync("name", Context, CancellationToken);

            // Asset
            actual.ShouldBeNull();
        }
    }
}