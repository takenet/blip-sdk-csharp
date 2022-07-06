﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions.GetLead;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Strategies;
using Take.Blip.Builder.Utils.SalesForce;
using Take.Blip.Builder.Utils.SalesForce.Models;
using Take.Blip.Client.Extensions.AdvancedConfig;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class GetLeadActionTests : ActionTestsBase
    {
        private readonly IConfigurationExtension _configurationExtension = Substitute.For<IConfigurationExtension>();
        private readonly ICrmClient _salesForceClient = Substitute.For<ICrmClient>();

        private readonly ICrmContext _crmContext;
        private readonly GetLeadAction _getLeadAction;

        public GetLeadActionTests()
        {
            _crmContext = new CrmContext();
            _getLeadAction = new GetLeadAction(
                _salesForceClient,
                _crmContext,
                _configurationExtension
                );
        }

        [Fact]
        public async Task TestGetLeadOnSalesForce_ShouldSuccedAsync()
        {
            // Arrange
            var settings = new CrmSettings()
            {
                Crm = Crm.SalesForce,
                LeadId = "1234",
                ReturnValue = "sfReturn"
            };

            var configurationResponse = new CrmConfig()
            {
                SalesForceConfig = new SalesForceConfig()
                {
                    ClientId = "clientId",
                    ClientSecret = "clientSecret",
                    RefreshToken = "refreshToken"
                }
            };
            var salesForceConfig = configurationResponse.SalesForceConfig;

            var mockedSalesForceAuth = new AuthorizationResponse
            {
                AccessToken = "123",
                InstanceUrl = "salesforce.com",
                TokenType = "Bearer"
            };

            var mockedSalesForceResponse = new JObject
            {
                { "Id", "1234" },
                { "Email", "io@take.net" },
                { "FirstName", "John" },
                { "LastName", "Doe" },
                { "Company", "Take" }
            };

            // Act
            _configurationExtension.GetKeyValueAsync<CrmConfig>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(configurationResponse);
            _salesForceClient.GetAuthorizationAsync(
                Arg.Is<CrmConfig>(sc =>
                sc.SalesForceConfig.ClientId == salesForceConfig.ClientId &&
                sc.SalesForceConfig.ClientSecret == salesForceConfig.ClientSecret &&
                sc.SalesForceConfig.RefreshToken == salesForceConfig.RefreshToken),
                Context.OwnerIdentity,
                Arg.Any<CancellationToken>())
                .Returns(mockedSalesForceAuth);

            _salesForceClient.GetLeadAsync(
                Arg.Is<CrmSettings>(rl =>
                rl.Crm == settings.Crm &&
                rl.ReturnValue == settings.ReturnValue
                ),
                Arg.Is<AuthorizationResponse>(ar =>
                ar.AccessToken == mockedSalesForceAuth.AccessToken &&
                ar.InstanceUrl == mockedSalesForceAuth.InstanceUrl &&
                ar.TokenType == mockedSalesForceAuth.TokenType
                ),
                Arg.Any<CancellationToken>()
                )
                .Returns(mockedSalesForceResponse);

            await _getLeadAction.ExecuteAsync(Context, settings, CancellationToken.None);

            // Assert
            await Context.Received(1).SetVariableAsync(
                settings.ReturnValue,
                JsonConvert.SerializeObject(mockedSalesForceResponse),
                CancellationToken.None
                );
        }

        [Fact]
        public async Task TestGetLeadActionTests_ShouldFailAsync()
        {
            // Arrange
            var settings = new CrmSettings()
            {
                Crm = Crm.SalesForce,
                LeadId = "1234",
                ReturnValue = "sfReturn"
            };

            var configurationResponse = new CrmConfig()
            {
                SalesForceConfig = new SalesForceConfig()
                {
                    ClientId = "clientId",
                    ClientSecret = "clientSecret",
                    RefreshToken = "refreshToken"
                }
            };
            var salesForceConfig = configurationResponse.SalesForceConfig;

            var mockedSalesForceAuth = new AuthorizationResponse
            {
                AccessToken = "123",
                InstanceUrl = "salesforce.com",
                TokenType = "Bearer"
            };

            var mockedSalesForceResponse = new JObject
            {
                { "errorCode",  "NOT_FOUND" },
                { "message", "Provided external ID field does not exist or is not accessible" }
            };

            // Act
            _configurationExtension.GetKeyValueAsync<CrmConfig>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(configurationResponse);
            _salesForceClient.GetAuthorizationAsync(
                Arg.Is<CrmConfig>(sc =>
                sc.SalesForceConfig.ClientId == salesForceConfig.ClientId &&
                sc.SalesForceConfig.ClientSecret == salesForceConfig.ClientSecret &&
                sc.SalesForceConfig.RefreshToken == salesForceConfig.RefreshToken),
                Context.OwnerIdentity,
                Arg.Any<CancellationToken>())
                .Returns(mockedSalesForceAuth);

            _salesForceClient.GetLeadAsync(
                Arg.Is<CrmSettings>(rl =>
                rl.Crm == settings.Crm &&
                rl.ReturnValue == settings.ReturnValue
                ),
                Arg.Is<AuthorizationResponse>(ar =>
                ar.AccessToken == mockedSalesForceAuth.AccessToken &&
                ar.InstanceUrl == mockedSalesForceAuth.InstanceUrl &&
                ar.TokenType == mockedSalesForceAuth.TokenType
                ),
                Arg.Any<CancellationToken>()
                )
                .Returns(mockedSalesForceResponse);

            await _getLeadAction.ExecuteAsync(Context, settings, CancellationToken.None);

            // Assert
            await Context.Received(1).SetVariableAsync(
                settings.ReturnValue,
                JsonConvert.SerializeObject(mockedSalesForceResponse),
                CancellationToken.None
                );

        }
    }
}
