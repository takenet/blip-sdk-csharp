using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions.ForwardMessageToDesk;

namespace Take.Blip.Builder.Actions.ForwardToDesk
{
    public class ForwardToDeskAction : ActionBase<ForwardToDeskSettings>
    {
        private readonly IForwardToDesk _forwardToDesk;

        public ForwardToDeskAction(IForwardToDesk forwardToDesk) : base(nameof(ForwardToDesk))
        {
            _forwardToDesk = forwardToDesk;
        }

        public override async Task ExecuteAsync(IContext context, ForwardToDeskSettings settings, CancellationToken cancellationToken)
        {
            await _forwardToDesk.SendMessageAsync(context, settings, cancellationToken);
        }
    }
}
