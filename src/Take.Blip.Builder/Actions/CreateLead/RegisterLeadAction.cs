using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions.CreateLead.SalesForce;
using Take.Blip.Client.Extensions.AdvancedConfig;

namespace Take.Blip.Builder.Actions.CreateLead
{
    class RegisterLeadAction : ActionBase<RegisterLeadSettings>
    {
        private readonly ICrmContext _crmContext;
        private readonly IConfigurationExtension _configurationExtension;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public RegisterLeadAction(
            ICrmContext crmContext,
            ILogger logger,
            IConfigurationExtension configurationExtension,
            IMemoryCache memoryCache
            ) : base(nameof(RegisterLeadAction))
        {
            _crmContext = crmContext;
            _logger = logger;
            _configurationExtension = configurationExtension;
            _memoryCache = memoryCache;
        }

        public override async Task ExecuteAsync(IContext context, RegisterLeadSettings settings, CancellationToken cancellationToken)
        {
            if (settings.Crm == "SalesForce")
            {
                _crmContext.SetCrm(new SalesForceProcessor(_logger, _configurationExtension, _memoryCache));
            }

            await _crmContext.ExecuteAsync(context, settings, cancellationToken);
        }
    }
}
