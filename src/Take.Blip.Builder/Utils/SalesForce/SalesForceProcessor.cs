using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Utils.SalesForce.Models;
using Take.Blip.Client.Extensions.AdvancedConfig;

namespace Take.Blip.Builder.Utils.SalesForce
{
    public class SalesForceProcessor : ICrmProcessor
    {
        private readonly IConfigurationExtension _configurationExtension;
        private readonly ISalesForceClient _salesForceClient;

        public SalesForceProcessor(IConfigurationExtension configurationExtension, ISalesForceClient salesForceClient)
        {
            _configurationExtension = configurationExtension;
            _salesForceClient = salesForceClient;
        }

        public async Task RegisterLead(IContext context, CrmSettings settings, CancellationToken cancellationToken)
        {
            var crmConfig = await _configurationExtension.GetKeyValueAsync<CrmConfig>(
                SalesForceConstants.CONFIG_DOMAIN,
                SalesForceConstants.CRM_CONFIG_KEY,
                cancellationToken
                );

            var salesForceAuth = await _salesForceClient.GetAuthorizationAsync(
                crmConfig.SalesForceConfig,
                context.OwnerIdentity,
                cancellationToken
                );

            var leadResponse = await _salesForceClient.CreateLeadAsync(settings, salesForceAuth);

            await context.SetVariableAsync(
                settings.ReturnValue,
                JsonConvert.SerializeObject(leadResponse),
                cancellationToken
                );
        }
    }
}
