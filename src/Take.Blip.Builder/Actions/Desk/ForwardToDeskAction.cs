using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions.ForwardMessageToDesk;

namespace Take.Blip.Builder.Actions.ForwardToDesk
{
    public class ForwardToDeskAction : ActionBase<ForwardToDeskSettings>
    {
        private readonly IForwardToDesk _forwardToDesk;
        private readonly IStateManager _stateManager;

        public ForwardToDeskAction(IForwardToDesk forwardToDesk, IStateManager stateManager) : base(nameof(ForwardToDesk))
        {
            _forwardToDesk = forwardToDesk;
            _stateManager = stateManager;
        }

        public override async Task ExecuteAsync(IContext context, ForwardToDeskSettings settings, CancellationToken cancellationToken)
        {
            bool helpDeskHasTicket = await _forwardToDesk.GetOrCreateTicketAsync(context, settings, cancellationToken);

            if (helpDeskHasTicket)
            {
                await _forwardToDesk.SendMessageAsync(context, settings, cancellationToken);
            }
            else
            {
                throw new ValidationException(
                    $"Cannot find ticket to send message to Desk. Error occurred in the '{nameof(ForwardToDesk)}' action");
            }
        }
    }
}
