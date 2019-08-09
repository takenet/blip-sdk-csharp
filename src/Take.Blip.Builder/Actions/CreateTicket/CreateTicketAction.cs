using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.HelpDesk;

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
            if (settings.OwnerIdentity == null)
            {
                settings.OwnerIdentity = context.OwnerIdentity;
            }
            
            if (settings.CustomerIdentity == null)
            {
                settings.CustomerIdentity = context.UserIdentity;
            }

            if (context.Flow.BuilderConfiguration.UseTunnelOwnerContext ?? false)
            {
                if (settings.RoutingOwnerIdentity == null)
                {
                    settings.RoutingOwnerIdentity = _application.Identity;
                }

                if (settings.RoutingCustomerIdentity == null)
                {
                    settings.RoutingCustomerIdentity = context.Input.Message.From.ToIdentity();
                }
            }

            var ticket = await _helpDeskExtension.CreateTicketAsync(settings, cancellationToken);
            context.SetTicket(ticket);

            if (!string.IsNullOrWhiteSpace(settings.Variable))
            {
                await context.SetVariableAsync(settings.Variable, ticket.Id, cancellationToken);
            }
        }
    }
}