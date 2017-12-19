using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Network;
using NSubstitute;
using Shouldly;
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
            FlowId = "0";
            User = "user@msging.net";
        }

        public IDictionary<string, Document> ValuesDictionary { get; }

        public string FlowId { get; set; }

        public Identity User { get; set; }

        private Context GetTarget()
        {
            return new Context(
                FlowId,
                User,
                new DictionaryContextExtension(ValuesDictionary),
                Substitute.For<IContactExtension>());
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
