using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Actions.CreateLead
{
    public class CrmContext : ICrmContext
    {
        private ICrmProcessor _crmProcessor;
        public async Task ExecuteAsync(IContext context, RegisterLeadSettings settings, CancellationToken cancellationToken)
        {
            await _crmProcessor.RegisterLead(context, settings, cancellationToken);
        }

        public void SetCrm(ICrmProcessor crmProcessor)
        {
            _crmProcessor = crmProcessor;
        }
    }
}
