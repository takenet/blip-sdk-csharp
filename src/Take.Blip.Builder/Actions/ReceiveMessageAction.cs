using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lime.Protocol;

namespace Take.Blip.Builder.Actions
{
    public class ReceiveMessageAction : IAction
    {
        private readonly BufferBlock<Message> _messageBufferBlock;

        public ReceiveMessageAction(BufferBlock<Message> messageBufferBlock)
        {
            _messageBufferBlock = messageBufferBlock;
        } 

        public Task<bool> CanExecuteAsync(IContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(_messageBufferBlock.Count == 0);
        }

        public Task ExecuteAsync(IContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
