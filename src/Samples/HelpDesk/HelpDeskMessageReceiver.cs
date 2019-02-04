using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.HelpDesk;

namespace HelpDesk
{
    public class HelpDeskMessageReceiver : HelpDeskReplyMessageReceiverBase
    {
        public HelpDeskMessageReceiver(ISender sender, IHelpDeskExtension helpDeskExtension)
            : base (sender, helpDeskExtension)
        {
        }

        protected async override Task ReceiveAsync(Message message, Node customerIdentiy, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Send messages from agent to customer
            Console.WriteLine($"< Received message from agent [CUSTOMER: {customerIdentiy}]: {message.Content}");
            await _sender.SendMessageAsync(message.Content, customerIdentiy, cancellationToken);
        }
    }
}
