using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Util;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions
{
    public class SendMessageAction : IAction
    {
        private readonly ISender _sender;
        private readonly Message _message;

        public SendMessageAction(ISender sender, Message message)
        {
            _sender = sender;
            _message = message;
        }

        public Task<bool> CanExecuteAsync(IContext context, CancellationToken cancellationToken)
        {
            return TaskUtil.TrueCompletedTask;
        }

        public Task ExecuteAsync(IContext context, CancellationToken cancellationToken)
        {
            var executionMessage = _message.ShallowCopy();
            executionMessage.To = context.User;
            return _sender.SendMessageAsync(executionMessage, cancellationToken);
        }
    }
}
