using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions.ForwardMessageToDesk;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.HelpDesk;

namespace Take.Blip.Builder.Actions.LeavingFromDesk
{
    public class LeavingFromDeskAction : ActionBase<LeavingFromDeskSettings>
    {
        private readonly ILeavingFromDesk _leavingToDesk;

        public LeavingFromDeskAction(ILeavingFromDesk leavingToDesk) : base(nameof(LeavingFromDesk))
        {
            _leavingToDesk = leavingToDesk;
        }

        public override async Task ExecuteAsync(IContext context, LeavingFromDeskSettings settings, CancellationToken cancellationToken)
        {
            await _leavingToDesk.CloseOpenedTicketsAsync(context, settings, cancellationToken);
        }
    }
}
