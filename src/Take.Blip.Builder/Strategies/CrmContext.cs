using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Strategies
{
    public class CrmContext : ICrmContext
    {
        private ICrmProcessor _crmProcessor;
        public async Task ExecuteAsync(IContext context, CrmSettings settings, CrmActionType actionType, CancellationToken cancellationToken)
        {
            if (actionType.Equals(CrmActionType.GetLead))
            {
                await _crmProcessor.GetLeadAsync(context, settings, cancellationToken);
            }
            else if (actionType.Equals(CrmActionType.CreateLead))
            {
                await _crmProcessor.RegisterLeadAsync(context, settings, cancellationToken);
            }
        }

        public void SetCrm(ICrmProcessor crmProcessor)
        {
            _crmProcessor = crmProcessor;
        }
    }
}
