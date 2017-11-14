using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lime.Protocol;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Actions
{
    public class ReceiveMessageAction : IAction
    {
        private readonly BufferBlock<Message> _messageBufferBlock;

        public ReceiveMessageAction(BufferBlock<Message> messageBufferBlock)
        {
            _messageBufferBlock = messageBufferBlock;
        }

        public string Name => "ReceiveMessage";

        public async Task<bool> ExecuteAsync(IContext context, JObject argument, CancellationToken cancellationToken)
        {
            if (!_messageBufferBlock.TryReceive(m => true, out Message message))
            {
                return false;
            }

            return true;
        }
    }
}
