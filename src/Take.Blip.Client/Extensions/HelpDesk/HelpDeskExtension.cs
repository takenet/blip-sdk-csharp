using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using System.Web;
using System;
using Lime.Messaging.Contents;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Client.Extensions.HelpDesk
{
    public class HelpDeskExtension : ExtensionBase, IHelpDeskExtension
    {
        private readonly ISender _sender;
        private const string DESK_DOMAIN = "desk.msging.net";
        private const string ID_PREFIX = "fwd";

        public HelpDeskExtension(ISender sender)
            : base(sender)
        {
            _sender = sender;
        }

        public async Task ForwardMessageToAgentAsync(Message message, CancellationToken cancellationToken)
        {
            var customerName = Uri.EscapeDataString(message.From.ToIdentity().ToString());
            var deskNode = new Identity(customerName, DESK_DOMAIN).ToNode();

            var fwMessage = new Message
            {
                Id = $"{ID_PREFIX}:{EnvelopeReceiverContext<Message>.Envelope?.Id ?? EnvelopeId.NewId()}",
                To = deskNode,
                Content = message.Content
            };

            await _sender.SendMessageAsync(fwMessage, cancellationToken);
        }

        public bool IsFromAgent(Message message)
        {
            return message.From.Domain.Equals(DESK_DOMAIN);
        }

        public async Task<Ticket> CreateTicketAsync(Identity customerIdentity, Document context, CancellationToken cancellationToken)
        {
            var newTicketCommand = new Command
            {
                Id = EnvelopeId.NewId(),
                Method = CommandMethod.Set,
                Uri = new LimeUri($"/tickets/{Uri.EscapeDataString(customerIdentity.ToString())}"),
                To = new Node("postmaster", DESK_DOMAIN, null),
                Resource = context
            };

            var result = await _sender.ProcessCommandAsync(newTicketCommand, cancellationToken);
            EnsureSuccess(result);
            return result.Resource as Ticket;
        }

        public async Task CloseTicketAsUser(string ticketId, CancellationToken cancellationToken)
        {
            var newTicketCommand = new Command
            {
                Id = EnvelopeId.NewId(),
                Method = CommandMethod.Set,
                Uri = new LimeUri($"/tickets/change-status"),
                To = new Node("postmaster", DESK_DOMAIN, null),
                Resource = new Ticket
                {
                    Id = ticketId,
                    Status = TicketStatusEnum.ClosedClient
                }
            };

            var result = await _sender.ProcessCommandAsync(newTicketCommand, cancellationToken);
            EnsureSuccess(result);
        }
    }
}
