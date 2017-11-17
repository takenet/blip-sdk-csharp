﻿using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions
{
    public class SendMessageAction : IAction
    {
        private readonly ISender _sender;

        public SendMessageAction(ISender sender)
        {
            _sender = sender;
        }

        public string Name => "SendMessage";

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            var message = settings.ToObject<Message>();
            message.To = context.User.ToNode();
            await _sender.SendMessageAsync(message, cancellationToken);
        }
    }
}