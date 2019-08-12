using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using System.Web;
using System;
using Lime.Messaging.Contents;
using Takenet.Iris.Messaging.Resources;
using System.Linq;

namespace Take.Blip.Client.Extensions.HelpDesk
{
    public class HelpDeskExtension : ExtensionBase, IHelpDeskExtension
    {
        public const string DEFAULT_DESK_DOMAIN = "desk." + Constants.DEFAULT_DOMAIN;
        public const string ID_PREFIX = "fwd";
        
        private readonly ISender _sender;
        private readonly Node _deskNode;

        public HelpDeskExtension(ISender sender)
            : base(sender)
        {
            _sender = sender;
            _deskNode = new Node("postmaster", DEFAULT_DESK_DOMAIN, null);
        }

        public async Task ForwardMessageToAgentAsync(Message message, CancellationToken cancellationToken)
        {
            var customerName = Uri.EscapeDataString(message.From.ToIdentity().ToString());
            var deskNode = new Identity(customerName, DEFAULT_DESK_DOMAIN).ToNode();

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
            return message.From.Domain.Equals(DEFAULT_DESK_DOMAIN);
        }

        public async Task<Ticket> CreateTicketAsync(Identity customerIdentity, Document context, CancellationToken cancellationToken)
        {
            var newTicketCommand = new Command
            {
                Id = EnvelopeId.NewId(),
                To = _deskNode,
                Method = CommandMethod.Set,
                Uri = new LimeUri($"/tickets/{Uri.EscapeDataString(customerIdentity.ToString())}"),
                Resource = context
            };

            var result = await _sender.ProcessCommandAsync(newTicketCommand, cancellationToken);
            EnsureSuccess(result);
            return (Ticket)result.Resource;
        }

        public async Task<Ticket> CreateTicketAsync(Ticket ticket, CancellationToken cancellationToken)
        {
            var newTicketCommand = new Command
            {
                Id = EnvelopeId.NewId(),
                To = _deskNode,
                Method = CommandMethod.Set,
                Uri = new LimeUri("/tickets"),
                Resource = ticket
            };

            var result = await _sender.ProcessCommandAsync(newTicketCommand, cancellationToken);
            EnsureSuccess(result);
            return (Ticket)result.Resource;
        }

        public async Task CloseTicketAsUser(string ticketId, CancellationToken cancellationToken)
        {
            var newTicketCommand = new Command
            {
                Id = EnvelopeId.NewId(),
                To = _deskNode,
                Method = CommandMethod.Set,
                Uri = new LimeUri($"/tickets/change-status"),
                Resource = new Ticket
                {
                    Id = ticketId,
                    Status = TicketStatusEnum.ClosedClient
                }
            };

            var result = await _sender.ProcessCommandAsync(newTicketCommand, cancellationToken);
            EnsureSuccess(result);
        }

        public async Task<Ticket> GetUserOpenTicketsAsync(Identity customerIdentity, CancellationToken cancellationToken)
        {
            var openTicketCommand = new Command
            {
                Id = EnvelopeId.NewId(),
                To = _deskNode,
                Method = CommandMethod.Get,
                Uri = new LimeUri($"/tickets?$filter={Uri.EscapeDataString($"customerIdentity eq '{customerIdentity}' and status eq 'Open'")}"),
            };
            var result = await _sender.ProcessCommandAsync(openTicketCommand, cancellationToken);
            EnsureSuccess(result);

            var ticketCollection = result.Resource as DocumentCollection;
            return ticketCollection?.Items?.FirstOrDefault() as Ticket;
        }

        public async Task<Ticket> GetCustomerActiveTicketAsync(Identity customerIdentity, CancellationToken cancellationToken)
        {
            var openTicketCommand = new Command
            {
                Id = EnvelopeId.NewId(),
                To = _deskNode,
                Method = CommandMethod.Get,
                Uri = new LimeUri($"/tickets?$filter={Uri.EscapeDataString($"customerIdentity eq '{customerIdentity}' and (status eq 'Open' or status eq 'Waiting' or status eq 'Assigned')")}"),
            };
            var result = await _sender.ProcessCommandAsync(openTicketCommand, cancellationToken);
            EnsureSuccess(result);

            var ticketCollection = result.Resource as DocumentCollection;
            return ticketCollection?.Items?.FirstOrDefault() as Ticket;        
        }
    }
}
