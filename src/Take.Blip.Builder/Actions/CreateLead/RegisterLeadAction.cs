using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Strategies;
using Take.Blip.Builder.Utils.SalesForce;
using Take.Blip.Client.Extensions.AdvancedConfig;

namespace Take.Blip.Builder.Actions.CreateLead
{
    public class RegisterLeadAction : ActionBase<CrmSettings>
    {
        private readonly ICrmContext _crmContext;
        private readonly IConfigurationExtension _configurationExtension;
        private readonly ICrmClient _crmClient;

        public RegisterLeadAction(
            ICrmContext crmContext,
            IConfigurationExtension configurationExtension,
            ICrmClient crmClient
            ) : base(nameof(CreateLead))
        {
            _crmContext = crmContext;
            _configurationExtension = configurationExtension;
            _crmClient = crmClient;
        }

        public override async Task ExecuteAsync(IContext context, CrmSettings settings, CancellationToken cancellationToken)
        {
            if (settings.Crm == Crm.SalesForce)
            {
                _crmContext.SetCrm(new SalesForceProcessor(_configurationExtension, _crmClient));
            }

            await _crmContext.ExecuteAsync(context, settings, ActionType.CreateLead, cancellationToken);
        }
    }
}
