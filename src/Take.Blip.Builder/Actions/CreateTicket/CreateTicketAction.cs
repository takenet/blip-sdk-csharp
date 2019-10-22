using Lime.Protocol;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.HelpDesk;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Builder.Actions.CreateTicket
{
    public class CreateTicketAction : ActionBase<CreateTicketSettings>
    {
        private readonly IHelpDeskExtension _helpDeskExtension;
        private readonly Application _application;

        public CreateTicketAction(IHelpDeskExtension helpDeskExtension, Application application)
            : base(nameof(CreateTicket))
        {
            _helpDeskExtension = helpDeskExtension;
            _application = application;
        }

        public override async Task ExecuteAsync(IContext context, CreateTicketSettings settings, CancellationToken cancellationToken)
        {
            var ticket = new Ticket()
            {
                OwnerIdentity = settings.OwnerIdentity,
                CustomerIdentity = settings.CustomerIdentity,
                RoutingOwnerIdentity = settings.RoutingOwnerIdentity,
                RoutingCustomerIdentity = settings.RoutingCustomerIdentity,
                CustomerInput = settings.CustomerInput,
            };

            if (ticket.OwnerIdentity == null)
            {
                ticket.OwnerIdentity = context.OwnerIdentity;
            }

            if (ticket.CustomerIdentity == null)
            {
                ticket.CustomerIdentity = context.UserIdentity;
            }

            if (context.Flow.BuilderConfiguration.UseTunnelOwnerContext ?? false)
            {
                if (ticket.RoutingOwnerIdentity == null &&
                    ticket.OwnerIdentity != _application.Identity)
                {
                    ticket.RoutingOwnerIdentity = _application.Identity;
                }

                if (ticket.RoutingCustomerIdentity == null)
                {
                    var fromIdentity = context.Input.Message.From.ToIdentity();
                    if (ticket.CustomerIdentity != fromIdentity)
                    {
                        ticket.RoutingCustomerIdentity = fromIdentity;
                    }
                }
            }

            if (ticket.CustomerInput == null)
            {
                ticket.CustomerInput = new DocumentContainer
                {
                    Value = context.Input.Content
                };
            }

            var createdTicket = await _helpDeskExtension.CreateTicketAsync(ticket, cancellationToken);
            context.SetTicket(createdTicket);

            if (!string.IsNullOrWhiteSpace(settings.Variable))
            {
                await context.SetVariableAsync(settings.Variable, createdTicket.Id, cancellationToken);
            }
        }
    }
}