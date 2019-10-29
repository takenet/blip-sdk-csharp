using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lime.Messaging;
using Lime.Messaging.Contents;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using NSubstitute;
using Serilog;
using Shouldly;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Storage;
using Take.Blip.Builder.Storage.Memory;
using Take.Blip.Client;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Take.Blip.Client.Extensions.Contacts;
using Take.Blip.Client.Extensions.HelpDesk;
using Take.Blip.Client.Extensions.Tunnel;
using Xunit;

namespace Take.Blip.Builder.UnitTests
{
    public abstract class ContextBaseTests : CancellationTokenTestsBase
    {
        public ContextBaseTests()
        {
            var documentTypeResolver = new DocumentTypeResolver().WithMessagingDocuments();

            ArtificialIntelligenceExtension = Substitute.For<IArtificialIntelligenceExtension>();
            ContactExtension = Substitute.For<IContactExtension>();
            HelpDeskExtension = Substitute.For<IHelpDeskExtension>();
            TunnelExtension = Substitute.For<ITunnelExtension>();
            Logger = Substitute.For<ILogger>();
            Configuration = Substitute.For<IConfiguration>();
            OwnerCallerContactMap = new OwnerCallerContactMap();
            Sender = Substitute.For<ISender>();
            Flow = new Flow()
            {
                Id = "0",
                Configuration = new Dictionary<string, string>()
            };
            User = "user@msging.net";
            Application = new Application()
            {
                Identifier = "application",
                Domain = "msging.net",
                Instance = "default"
            };
            Input = new LazyInput(
                new Message()
                { 
                    From = User.ToNode(),
                    To = ApplicationIdentity.ToNode(),
                    Content = new PlainText()
                    {
                        Text = "Hello world!"
                    }
                },
                User,
                Flow.BuilderConfiguration,
                new DocumentSerializer(documentTypeResolver),
                new EnvelopeSerializer(documentTypeResolver),
                ArtificialIntelligenceExtension,
                CancellationToken);
            Configuration.ContactCacheExpiration.Returns(TimeSpan.FromMinutes(5));
        }

        public IArtificialIntelligenceExtension ArtificialIntelligenceExtension { get; }

        public IContactExtension ContactExtension { get; }

        public IHelpDeskExtension HelpDeskExtension { get; }
        
        public ITunnelExtension TunnelExtension { get; }
        
        public ISender Sender { get; set; }

        public ILogger Logger { get; }

        public IConfiguration Configuration { get; }

        public IOwnerCallerContactMap OwnerCallerContactMap { get; }

        public Identity User { get; set; }
        
        public Application Application { get; }

        public Identity ApplicationIdentity => Application.Identity;

        public LazyInput Input { get; set; }

        public Flow Flow { get; set; }

        protected abstract ContextBase GetTarget();

        protected abstract void AddVariableValue(string variableName, string variableValue);

        [Fact]
        public async Task GetExistingVariableShouldSucceed()
        {
            // Arrange
            var variableName = "variableName1";
            var variableValue = "value1";
            AddVariableValue(variableName, variableValue);
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync(variableName, CancellationToken);

            // Assert
            actual.ShouldBe(variableValue);
        }

        [Fact]
        public async Task GetExistingVariableWithContextSourceShouldSucceed()
        {
            // Arrange
            var variableName = "variableName1";
            var variableValue = "value1";
            AddVariableValue(variableName, variableValue);
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync($"context.{variableName}", CancellationToken);

            // Assert
            actual.ShouldBe(variableValue);
        }

        [Fact]
        public async Task GetExistingVariableWithLeadingDotShouldFail()
        {
            // Arrange
            var variableName = "variableName1";
            var variableValue = "value1";
            AddVariableValue(variableName, variableValue);
            var target = GetTarget();

            // Act
            await target.GetVariableAsync($".{variableName}", CancellationToken).ShouldThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetExistingVariableWithTrailingDotShouldFail()
        {
            // Arrange
            var variableName = "variableName1";
            var variableValue = "value1";
            AddVariableValue(variableName, variableValue);
            var target = GetTarget();

            // Act
            await target.GetVariableAsync($"{variableName}.", CancellationToken).ShouldThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetNonExistingVariableShouldReturnNull()
        {
            // Arrange
            var variableName = "variableName1";
            var variableValue = "value1";
            AddVariableValue(variableName, variableValue);
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("variableName2", CancellationToken);

            // Assert
            actual.ShouldBeNull();
        }

        [Fact]
        public async Task GetVariableWithJsonPropertyShouldSucceed()
        {
            // Arrange
            var variableName = "variableName1";
            AddVariableValue(variableName, "{\"plan\": \"Premium\",\"details\": {\"address\": \"Rua X\"}}");
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("variableName1@plan", CancellationToken);

            // Assert
            actual.ShouldBe("Premium");
        }

        [Fact]
        public async Task GetVariableWithInvalidJsonPropertyShouldSucceed()
        {
            // Arrange
            var variableName = "variableName1";
            AddVariableValue(variableName, "{\"plan\": \"Premium\",\"details\": {\"address\": \"Rua X\"}}");
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("variableName1@none", CancellationToken);

            // Assert
            actual.ShouldBeNull();
        }

        [Fact]
        public async Task GetVariableWithJsonPropertyWithTwoLevelsShouldSucceed()
        {
            // Arrange
            var variableName = "variableName1";
            AddVariableValue(variableName, "{\"plan\": \"Premium\",\"details\": {\"address\": \"Rua X\"}}");
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("variableName1@details.address", CancellationToken);

            // Assert
            actual.ShouldBe("Rua X");
        }

        [Fact]
        public async Task GetVariableWithJsonInvalidPropertyWithTwoLevelsShouldReturnNull()
        {
            // Arrange
            var variableName = "variableName1";
            AddVariableValue(variableName, "{\"plan\": \"Premium\",\"details\": {\"address\": \"Rua X\"}}");
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("variableName1@details.none", CancellationToken);

            // Assert
            actual.ShouldBeNull();
        }

        [Fact]
        public async Task GetContactVariableShouldSucceed()
        {
            // Arrange
            var contact = new Contact
            {
                Name = "John da Silva"
            };

            ContactExtension.GetAsync(User, CancellationToken).Returns(contact);
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("contact.name", CancellationToken);

            // Assert
            actual.ShouldBe(contact.Name);
        }

        [Fact]
        public async Task GetContactExtrasVariableShouldSucceed()
        {
            // Arrange
            var contact = new Contact
            {
                Name = "John da Silva",
                Extras = new Dictionary<string, string>()
                {
                    { "property1", "value 1" }
                }
            };

            ContactExtension.GetAsync(User, CancellationToken).Returns(contact);
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("contact.extras.property1", CancellationToken);

            // Assert
            actual.ShouldBe(contact.Extras["property1"]);
        }

        [Fact]
        public async Task GetInvalidContactVariableShouldReturnNull()
        {
            // Arrange
            var contact = new Contact
            {
                Name = "John da Silva"
            };

            ContactExtension.GetAsync(User, CancellationToken).Returns(contact);
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("contact.invalid", CancellationToken);

            // Assert
            actual.ShouldBeNull();
        }

        [Fact]
        public async Task GetCalendarVariableShouldSucceed()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("calendar.hour", CancellationToken);

            // Assert
            actual.ShouldBe(now.Hour.ToString());
        }

        [Fact]
        public async Task GetInvalidCalendarVariableShouldReturnNull()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("calendar.dimenson", CancellationToken);

            // Assert
            actual.ShouldBeNull();
        }

        [Fact]
        public async Task GetRandomGuidVariableShouldSucceed()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("random.guid", CancellationToken);

            // Assert
            actual.ShouldNotBeNull();
            Guid.TryParse(actual, out _).ShouldBeTrue();
        }

        [Fact]
        public async Task GetRandomIntegerVariableShouldSucceed()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("random.integer", CancellationToken);

            // Assert
            actual.ShouldNotBeNull();
            int.TryParse(actual, out _).ShouldBeTrue();
        }

        [Fact]
        public async Task GetRandomStringVariableShouldSucceed()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("random.string", CancellationToken);

            // Assert
            actual.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetBucketTextVariableShouldSucceed()
        {
            // Arrange
            var document = new PlainText()
            {
                Text = "my value"
            };

            SetupGetCommandResult("/buckets/id1", document);

            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("bucket.id1", CancellationToken);

            // Assert
            actual.ShouldBe(document.Text);
        }

        [Fact]
        public async Task GetBucketJsonVariablePropertyShouldSucceed()
        {
            // Arrange
            var document = new JsonDocument()
            {
                {"key1", "value1"},
                {"key2", "value2"}
            };

            SetupGetCommandResult("/buckets/id1", document);

            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("bucket.id1@key2", CancellationToken);

            // Assert
            actual.ShouldBe("value2");
        }
        [Fact]
        public async Task GetResourceTextVariableShouldSucceed()
        {
            // Arrange
            var document = new PlainText()
            {
                Text = "my value"
            };
            
            SetupGetCommandResult("/resources/id1", document);
            
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("resource.id1", CancellationToken);

            // Assert
            actual.ShouldBe(document.Text);
        }

        [Fact]
        public async Task GetResourceJsonVariablePropertyShouldSucceed()
        {
            // Arrange
            var document = new JsonDocument()
            {
                {"key1", "value1"},
                {"key2", "value2"}
            };

            SetupGetCommandResult("/resources/id1", document);

            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("resource.id1@key2", CancellationToken);

            // Assert
            actual.ShouldBe("value2");
        }

        private void SetupGetCommandResult(string uri, Document resource)
        {
            Sender
                .ProcessCommandAsync(
                    Arg.Is<Command>(c => c.Method == CommandMethod.Get && c.Uri.ToString().Equals(uri)), CancellationToken)
                .Returns(new Command()
                {
                    Status = CommandStatus.Success,
                    Resource = resource
                });
        }
    }
}