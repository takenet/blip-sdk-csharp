using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Take.Blip.Builder.Actions.ForwardMessageToDesk;

namespace Take.Blip.Builder.Actions.ForwardToDesk
{
    public class ForwardToDeskAction : ActionBase<ForwardToDeskSettings>
    {
        private readonly IForwardToDesk _forwardToDesk;
        private readonly IStateManager _stateManager;
        private readonly ILogger _logger;

        public ForwardToDeskAction(IForwardToDesk forwardToDesk, IStateManager stateManager, ILogger logger) : base(nameof(ForwardToDesk))
        {
            _forwardToDesk = forwardToDesk;
            _stateManager = stateManager;
            _logger = logger;
        }

        public override async Task ExecuteAsync(IContext context, ForwardToDeskSettings settings, CancellationToken cancellationToken)
        {
            bool helpDeskHasTicket = await _forwardToDesk.GetOrCreateTicketAsync(context, settings, cancellationToken);

            if (!helpDeskHasTicket)
            {
                _logger.Information("[Desk-State] Cannot get or create ticket to send message to Desk for UserIdentity {UserIdentity} from OwnerIdentity {OwnerIdentity}.",
                   context.UserIdentity,
                   context.OwnerIdentity
                );
            }

            await _forwardToDesk.SendMessageAsync(context, settings, cancellationToken);
        }
    }
}
