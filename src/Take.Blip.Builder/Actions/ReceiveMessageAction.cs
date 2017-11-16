using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Actions
{
    public class ReceiveMessageAction : IAction
    {
        private readonly IFlowMessageManager _flowMessageManager;

        public ReceiveMessageAction(IFlowMessageManager flowMessageManager)
        {
            _flowMessageManager = flowMessageManager;
        }

        public string Name => "ReceiveMessage";

        public async Task ExecuteAsync(IContext context, JObject argument, CancellationToken cancellationToken)
        {
            var message = await _flowMessageManager.DequeueMessageAsync(context.User, cancellationToken);

            // TODO: Validate the message accordingly to the argument
        }
    }
}
