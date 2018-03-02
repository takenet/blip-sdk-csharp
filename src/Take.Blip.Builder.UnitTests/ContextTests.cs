using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using NSubstitute;
using Shouldly;
using SimpleInjector;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Variables;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Take.Blip.Client.Extensions.Contacts;
using Take.Blip.Client.Extensions.Context;
using Xunit;

namespace Take.Blip.Builder.UnitTests
{
    public class ContextTests : CancellationTokenTestsBase
    {
        public ContextTests()
        {
            ValuesDictionary = new Dictionary<string, Document>(StringComparer.InvariantCultureIgnoreCase);
            ArtificialIntelligenceExtension = Substitute.For<IArtificialIntelligenceExtension>();
            ContextExtension = new DictionaryContextExtension(ValuesDictionary);
            ContactExtension = Substitute.For<IContactExtension>();
            Sender = Substitute.For<ISender>();
            Flow = new Flow()
            {
                Id = "0",
                Configuration = new Dictionary<string, string>()
            };
            User = "user@msging.net";
            Input = new LazyInput(new PlainText()
                {
                    Text = "Hello world!"
                },
                Flow.Configuration,
                new DocumentSerializer(),
                ArtificialIntelligenceExtension,
                CancellationToken);
        }

        public IDictionary<string, Document> ValuesDictionary { get; }

        public IArtificialIntelligenceExtension ArtificialIntelligenceExtension { get; }

        public IContactExtension ContactExtension { get; }

        public IContextExtension ContextExtension { get; set; }

        public ISender Sender { get; set; }

        public Identity User { get; set; }

        public LazyInput Input { get; set; }

        public Flow Flow { get; set; }
        
        private Context GetTarget()
        {
            var container = new Container();
            container.RegisterBuilder();
            container.RegisterSingleton(ContactExtension);
            container.RegisterSingleton(ContextExtension);
            container.RegisterSingleton(Sender);

            return new Context(
                User,
                Input,
                Flow,                
                ContextExtension,
                container.GetAllInstances<IVariableProvider>());
        }

        [Fact]
        public async Task GetExistingVariableShouldSucceed()
        {
            // Arrange
            var variableName = "variableName1";
            var variableValue = "value1";
            ValuesDictionary.Add(variableName, new PlainText() { Text = variableValue });
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
            ValuesDictionary.Add(variableName, new PlainText() { Text = variableValue });
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
            ValuesDictionary.Add(variableName, new PlainText() { Text = variableValue });
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
            ValuesDictionary.Add(variableName, new PlainText() { Text = variableValue });
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
            ValuesDictionary.Add(variableName, new PlainText() { Text = variableValue });
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
            ValuesDictionary.Add(variableName, new PlainText() { Text = "{\"plan\": \"Premium\",\"details\": {\"address\": \"Rua X\"}}"});
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
            ValuesDictionary.Add(variableName, new PlainText() { Text = "{\"plan\": \"Premium\",\"details\": {\"address\": \"Rua X\"}}" });
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
            ValuesDictionary.Add(variableName, new PlainText() { Text = "{\"plan\": \"Premium\",\"details\": {\"address\": \"Rua X\"}}" });
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
            ValuesDictionary.Add(variableName, new PlainText() { Text = "{\"plan\": \"Premium\",\"details\": {\"address\": \"Rua X\"}}" });
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
            Sender
                .ProcessCommandAsync(
                    Arg.Is<Command>(c => c.Method == CommandMethod.Get && c.Uri.ToString().Equals("/buckets/id1")), CancellationToken)
                .Returns(new Command()
                {
                    Status = CommandStatus.Success,
                    Resource = document
                });

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
            Sender
                .ProcessCommandAsync(
                    Arg.Is<Command>(c => c.Method == CommandMethod.Get && c.Uri.ToString().Equals("/buckets/id1")), CancellationToken)
                .Returns(new Command()
                {
                    Status = CommandStatus.Success,
                    Resource = document
                });

            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("bucket.id1@key2", CancellationToken);

            // Assert
            actual.ShouldBe("value2");
        }


        private class DictionaryContextExtension : IContextExtension
        {            
            public DictionaryContextExtension(IDictionary<string, Document> valuesDictionary)
            {
                ValuesDictionary = valuesDictionary;
            }

            public IDictionary<string, Document> ValuesDictionary { get; }

            public async Task<T> GetVariableAsync<T>(Identity identity, string variableName, CancellationToken cancellationToken) where T : Document
            {
                if (!ValuesDictionary.TryGetValue(variableName, out var variableValue))
                {
                    throw new LimeException(ReasonCodes.COMMAND_RESOURCE_NOT_FOUND, "Not found");
                }

                return (T)variableValue;
            }

            public Task SetVariableAsync<T>(Identity identity, string variableName, T document, CancellationToken cancellationToken,
                TimeSpan expiration = default(TimeSpan)) where T : Document
            {
                throw new NotImplementedException();
            }

            public Task SetGlobalVariableAsync<T>(string variableName, T document, CancellationToken cancellationToken,
                TimeSpan expiration = default(TimeSpan)) where T : Document
            {
                throw new NotImplementedException();
            }

            public Task DeleteVariableAsync(Identity identity, string variableName, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task DeleteGlobalVariableAsync(string variableName, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<DocumentCollection> GetVariablesAsync(Identity identity, int skip = 0, int take = 100,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                throw new NotImplementedException();
            }

            public Task<DocumentCollection> GetIdentitiesAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default(CancellationToken))
            {
                throw new NotImplementedException();
            }
        }
    }
}
