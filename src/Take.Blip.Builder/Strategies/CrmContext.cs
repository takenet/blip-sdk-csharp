using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Strategies
{
    public class CrmContext : ICrmContext
    {
        private ICrmProcessor _crmProcessor;
        public async Task ExecuteAsync(IContext context, CrmSettings settings, CancellationToken cancellationToken)
        {
            await _crmProcessor.RegisterLead(context, settings, cancellationToken);
        }

        public void SetCrm(ICrmProcessor crmProcessor)
        {
            _crmProcessor = crmProcessor;
        }
    }
}
