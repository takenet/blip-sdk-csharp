using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder.Storage;
using Takenet.Elephant;

namespace Take.Blip.Builder.Actions
{
    public class FlowMessageManager : IFlowMessageManager
    {
        private readonly IFlowManager _flowManager;
        private readonly IUserMessageQueueMap _userMessageQueueMap;

        public FlowMessageManager(IFlowManager flowManager, IUserMessageQueueMap userMessageQueueMap)
        {
            _flowManager = flowManager;
            _userMessageQueueMap = userMessageQueueMap;
        }

        public async Task EnqueueMessageAsync(Flow flow, string user, Message message, CancellationToken cancellationToken)
        {
            await _userMessageQueueMap.EnqueueItemAsync(user, message);
            await _flowManager.TryExecuteAsync(flow, user, cancellationToken);
        }

        public async Task<Message> DequeueMessageAsync(string user, CancellationToken cancellationToken)
        {
            var queue = await _userMessageQueueMap.GetValueOrEmptyAsync(user) as IBlockingQueue<Message>;
            if (queue == null) throw new NotSupportedException("The current queue implementation do not support blocking");
            return await queue.DequeueAsync(cancellationToken);
        }
    }
}