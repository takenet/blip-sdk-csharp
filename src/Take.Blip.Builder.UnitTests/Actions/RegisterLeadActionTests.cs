using Newtonsoft.Json;
using NSubstitute;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions.CreateLead;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Strategies;
using Take.Blip.Builder.Utils.SalesForce;
using Take.Blip.Builder.Utils.SalesForce.Models;
using Take.Blip.Client.Extensions.AdvancedConfig;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class RegisterLeadActionTests : ActionTestsBase
    {
        private readonly IConfigurationExtension _configurationExtension = Substitute.For<IConfigurationExtension>();
        private readonly ICrmClient _crmClient = Substitute.For<ICrmClient>();

        private readonly ICrmContext _crmContext;
        private readonly RegisterLeadAction _registerLeadAction;

        public RegisterLeadActionTests()
        {
            _crmContext = new CrmContext();
            _registerLeadAction = new RegisterLeadAction(
                _crmContext,
                _configurationExtension,
                _crmClient
                );
        }

        [Fact]
        public async Task TestCreateLeadOnSalesForce_ShouldSuccedAsync()
        {
            // Arrange
            var settings = new CrmSettings()
            {
                Crm = Crm.SalesForce,
                LeadBody = new Dictionary<string, string>()
                {
                    { "FirstName", "Gabriel" },
                    { "LastName", "Rodrigues" },
                    { "Email", "jsdpablo@take.net" },
                    { "Company", "Take" },
                    { "cidade__c", "Bh" },
                    { "Suplemento__c", "Whey" }
                },
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

            var mockedSalesForceResponse = new LeadResponse
            {
                Message = "Deu",
                Succes = true,
                Id = "123"
            };

            // Act
            _configurationExtension.GetKeyValueAsync<CrmConfig>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(configurationResponse);
            _crmClient.GetAuthorizationAsync(
                Arg.Is<CrmConfig>(sc =>
                sc.SalesForceConfig.ClientId == salesForceConfig.ClientId &&
                sc.SalesForceConfig.ClientSecret == salesForceConfig.ClientSecret &&
                sc.SalesForceConfig.RefreshToken == salesForceConfig.RefreshToken),
                Context.OwnerIdentity,
                Arg.Any<CancellationToken>())
                .Returns(mockedSalesForceAuth);

            _crmClient.CreateLeadAsync(
                Arg.Is<CrmSettings>(rl =>
                rl.Crm == settings.Crm &&
                rl.ReturnValue == settings.ReturnValue
                ),
                Arg.Is<AuthorizationResponse>(ar =>
                ar.AccessToken == mockedSalesForceAuth.AccessToken &&
                ar.InstanceUrl == mockedSalesForceAuth.InstanceUrl &&
                ar.TokenType == mockedSalesForceAuth.TokenType
                ))
                .Returns(mockedSalesForceResponse);

            await _registerLeadAction.ExecuteAsync(Context, settings, CancellationToken.None);

            // Assert
            await Context.Received(1).SetVariableAsync(
                settings.ReturnValue,
                JsonConvert.SerializeObject(mockedSalesForceResponse),
                CancellationToken.None
                );
        }
    }
}
