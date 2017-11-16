using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Takenet.Elephant;

namespace Take.Blip.Builder.Actions
{
    public class ReceiveMessageAction : IAction
    {
        private readonly IQueue<Message> _messageQueue;

        public ReceiveMessageAction(IQueue<Message> messageQueue)
        {
            _messageQueue = messageQueue;
        }

        public string Name => "ReceiveMessage";

        public async Task<bool> ExecuteAsync(IContext context, JObject argument, CancellationToken cancellationToken)
        {
            var message = await _messageQueue.DequeueOrDefaultAsync();
            if (message == null) return false; // Suspend the flow execution

            return true;
        }
    }

    public interface IFlowMessageManager
    {
        Task EnqueueMessageAsync(Flow flow, string user, Message message, CancellationToken cancellationToken);

        Task<Message> DequeueMessageAsync(string user, CancellationToken cancellationToken);
    }


    public class FlowMessageManager
    {
        private readonly IFlowManager _flowManager;
        private readonly IQueueMap<string, Message> _messageQueueMap;

        public FlowMessageManager(IFlowManager flowManager, IQueueMap<string, Message> messageQueueMap)
        {
            _flowManager = flowManager;
            _messageQueueMap = messageQueueMap;
        }

        public async Task EnqueueMessageAsync(Flow flow, string user, Message message, CancellationToken cancellationToken)
        {
            await _messageQueueMap.EnqueueItemAsync(user, message);
            
            await _flowManager.ExecuteAsync(flow, user, cancellationToken);
        }


        public async Task<Message> DequeueMessageAsync(string user, CancellationToken cancellationToken)
        {
            var queue = await _messageQueueMap.GetValueOrEmptyAsync(user);
            
        }
    }
}
