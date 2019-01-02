using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.HelpDesk;
using Takenet.Iris.Messaging.Resources;

namespace HelpDesk
{
    public class MessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        private readonly IHelpDeskExtension _helpDeskExtension;
        private static readonly IDictionary<Node, string> _states = new ConcurrentDictionary<Node, string>();

        public MessageReceiver(ISender sender, IHelpDeskExtension helpDeskExtension)
        {
            _sender = sender;
            _helpDeskExtension = helpDeskExtension;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            // Redirect customer from human to robot.
            if (message.Content.GetType().Equals(typeof(Redirect))){

                var redirect = message.Content as Redirect;
                var ticket = redirect.Context.Value as Ticket;
                var fromIdentity = Uri.UnescapeDataString(message.From.Name);

                _states.Remove(fromIdentity.ToString());

                Console.WriteLine($"< Ticket [ID:{ticket.Id}]: Finished");
                return;
            }

            // Customer is talking with a human
            if(_states.ContainsKey(message.From) && _states[message.From].Equals("human"))
            {
                // Fowarding message to an agent
                await _helpDeskExtension.ForwardMessageToAgentAsync(message, cancellationToken);
                return;
            }

            // Customer is talking with the bot
            var content = message.Content as PlainText;
            Console.WriteLine($"< Received message [FROM:{message.From}]: {message.Content}");

            if (content.Text.ToLowerInvariant().Equals("atendimento"))
            {
                _states.Add(message.From.ToIdentity().ToString(), "human");
                
                // Create a new ticket
                var ticket = await _helpDeskExtension.CreateTicketAsync(message.From.ToIdentity(), message.Content, cancellationToken);

                await _helpDeskExtension.ForwardMessageToAgentAsync(message, cancellationToken);
            }
            else
            {
                await _sender.SendMessageAsync(message.Content, message.From, cancellationToken);
            }
        }
    }
}
