using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;
using Takenet.Iris.Messaging.Contents;

namespace MessageTypes
{
    public class OptionResourceMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionResourceMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {

            Document document;
            var openWith = new Dictionary<string, string>();//using System.Collections.Generic
            openWith.Add("name", message.From.Name);//checar mais tarde <<

            if (message.Content.ToString().Equals("res1"))
                document = GetResource();
            else
                document = GetResourceMessageReplace(openWith);

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }

        public Resource GetResource()
        {
            var document = new Resource
            {
                Key = "welcome-message" //recurso previamente adicionado com extensão 'recursos' ou através do portal
            };

            return document;
        }

         public Resource GetResourceMessageReplace(Dictionary<string,string> openWith)
        {
            var document = new Resource
            {
                Key = "welcome-message",
                Variables = openWith
            };

            return document;
        }
    }
}
