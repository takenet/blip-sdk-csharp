using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Strategies;
using Take.Blip.Builder.Utils.SalesForce;
using Take.Blip.Client.Extensions.AdvancedConfig;

namespace Take.Blip.Builder.Actions.GetLead
{
    public class GetLeadAction : ActionBase<CrmSettings>
    {
        private readonly ISalesForceClient _salesForceClient;
        private readonly ICrmContext _crmContext;
        private readonly IConfigurationExtension _configurationExtension;

        public GetLeadAction(ISalesForceClient salesForceClient, ICrmContext crmContext, IConfigurationExtension configurationExtension) :
            base(nameof(GetLead))
        {
            _salesForceClient = salesForceClient;
            _crmContext = crmContext;
            _configurationExtension = configurationExtension;
        }

        public override async Task ExecuteAsync(IContext context, CrmSettings settings, CancellationToken cancellationToken)
        {
            if (settings.Crm == Crm.SalesForce)
            {
                _crmContext.SetCrm(new SalesForceProcessor(_configurationExtension, _salesForceClient));
            }

            await _crmContext.ExecuteAsync(context, settings, ActionType.GetLead, cancellationToken);
        }
    }
}
