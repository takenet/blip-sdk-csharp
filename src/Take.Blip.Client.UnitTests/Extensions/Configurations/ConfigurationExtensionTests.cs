using Lime.Protocol;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Utils.SalesForce.Models;
using Take.Blip.Client.Extensions.AdvancedConfig;
using Xunit;

namespace Take.Blip.Client.UnitTests.Extensions.Configurations
{
    public class ConfigurationExtensionTests : TestsBase
    {
        public ISender _sender = Substitute.For<ISender>();
        private readonly ConfigurationExtension _configurationExtension;
        private const string CONFIGURATION_LIME_URI = "lime://postmaster@portal.blip.ai/configuration";

        public ConfigurationExtensionTests()
        {
            _configurationExtension = new ConfigurationExtension(_sender);
        }

        [Fact]
        public async Task TestGetDomain_ShouldSuccedAsync()
        {
            //Arrange
            var domain = "postmaster@portal.blip.ai";
            var mockedResponse = new Command()
            {
                Method = CommandMethod.Get,
                Uri = new LimeUri(CONFIGURATION_LIME_URI),
                Resource = new JsonDocument(new Dictionary<string, object>()
                {
                    { "Plugins", "Broadcast" }
                }, MediaType.ApplicationJson)
            };
            var requestBodyCommand = new Command
            {
                Method = CommandMethod.Get,
                Uri = new LimeUri(CONFIGURATION_LIME_URI)
            };

            //Act
            _sender.ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>()).Returns(mockedResponse);
            var response = await _configurationExtension.GetDomainAsync(domain, CancellationToken.None);

            //Assert
            await _sender.Received(1).ProcessCommandAsync(Arg.Is<Command>(
                c =>
                c.Uri.Path == requestBodyCommand.Uri.Path &&
                c.Method.ToString() == requestBodyCommand.Method.ToString()),
                CancellationToken.None);

        }

        [Fact]
        public async Task TestGetDomain_ShouldFailAsync()
        {
            //Arrange
            var domain = string.Empty;

            //Act
            Func<Task<Document>> functionCall = () => _configurationExtension.GetDomainAsync(domain, CancellationToken.None);

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(functionCall);
        }

        [Theory]
        [InlineData("", "key")]
        [InlineData("domain", "")]
        public async Task TestGetKeyValue_ShouldFailAsync(string domain, string key)
        {
            //Act
            Func<Task<string>> functionCall = () => _configurationExtension.GetKeyValueAsync(domain, key, CancellationToken.None);

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(functionCall);
        }

        [Theory]
        [InlineData("postmaster@portal.blip.ai", "Crm")]
        [InlineData("postmaster@desk.msging.net", "Desk")]
        public async Task TestGetKeyValue_ShouldSuccedlAsync(string domain, string key)
        {
            //Arrange
            var mockedResponse = new Command()
            {
                Method = CommandMethod.Get,
                Uri = new LimeUri($"lime://{domain}/configuration"),
                Resource = new JsonDocument(new Dictionary<string, object>()
                {
                    { key, "value" }
                }, MediaType.ApplicationJson)
            };

            var expectedResponse = "value";

            //Act
            _sender.ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>()).Returns(mockedResponse);
            var response = await _configurationExtension.GetKeyValueAsync(domain, key, CancellationToken.None);

            //Assert
            Assert.Equal(expectedResponse, response);
        }

        [Theory]
        [InlineData("postmaster@portal.blip.ai", "Crm")]
        [InlineData("postmaster@desk.msging.net", "Desk")]
        public async Task TestGetTypedKeyValue_ShouldSuccedlAsync(string domain, string key)
        {
            //Arrange
            var expectedResponse = new CrmConfig()
            {
                SalesForceConfig = new SalesForceConfig()
                {
                    ClientId = "123",
                    ClientSecret = "321",
                    RefreshToken = "456"
                }
            };

            var mockedResponse = new Command()
            {
                Method = CommandMethod.Get,
                Uri = new LimeUri($"lime://{domain}/configuration"),
                Resource = new JsonDocument(new Dictionary<string, object>()
                {
                    { key, JsonConvert.SerializeObject(expectedResponse) }
                }, MediaType.ApplicationJson)
            };

            //Act
            _sender.ProcessCommandAsync(Arg.Any<Command>(), Arg.Any<CancellationToken>()).Returns(mockedResponse);
            var response = await _configurationExtension.GetKeyValueAsync<CrmConfig>(domain, key, CancellationToken.None);

            //Assert
            Assert.Equal(expectedResponse, response);
        }


        [Theory]
        [InlineData("postmaster@portal.blip.ai", "Crm", "Salesforce")]
        [InlineData("postmaster@desk.msging.net", "Desk", "Blipdesk")]
        public async Task TestSetKeyValue_ShouldSuccedlAsync(string domain, string key, string value)
        {
            //Arrange
            var resource = new JsonDocument(new Dictionary<string, object>()
                {
                    { key, value }
                }, MediaType.ApplicationJson);
            var mockedRequestCommand = new Command()
            {
                Method = CommandMethod.Set,
                Uri = new LimeUri($"lime://{domain}/configuration"),
                Resource = resource
            };

            //Act
            await _configurationExtension.SetConfigAsync(
                domain,
                resource,
                CancellationToken.None
                );

            //Assert
            await _sender.Received(1).ProcessCommandAsync(Arg.Is<Command>(
                c =>
                c.Method == mockedRequestCommand.Method &&
                c.Uri.Path == mockedRequestCommand.Uri.Path &&
                (c.Resource as JsonDocument)[key] == resource[key]
                ),
                Arg.Any<CancellationToken>()
                );
        }
    }
}
