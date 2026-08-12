using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Blip.Ai.Bot.Monitoring.Logging.Abstractions;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.HelpDesk;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Builder.Actions.CreateTicket
{
    public class CreateTicketAction : ActionBase<CreateTicketSettings>
    {
        private readonly IHelpDeskExtension _helpDeskExtension;
        private readonly Application _application;
        private readonly IBlipLogger _blipMonitoringLogger;

        private static readonly string[] OUTPUT_PARAMETERS_NAME = new string[] { nameof(CreateTicketSettings.Variable).ToCamelCase() };

        public CreateTicketAction(IHelpDeskExtension helpDeskExtension, Application application, IBlipLogger? blipMonitoringLogger = null)
            : base(nameof(CreateTicket), OUTPUT_PARAMETERS_NAME)
        {
            _helpDeskExtension = helpDeskExtension;
            _application = application;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(IContext context, CreateTicketSettings settings, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            try
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

                this.LogExecution(_blipMonitoringLogger, context, new JObject
                {
                    ["customerIdentity"] = ticket.CustomerIdentity?.ToString(),
                    ["outputVariable"] = settings.Variable,
                    ["ticketId"] = createdTicket?.Id,
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                });
            }
            catch (Exception ex)
            {
                this.LogError(_blipMonitoringLogger, context, new JObject
                {
                    ["customerIdentity"] = settings.CustomerIdentity?.ToString(),
                    ["outputVariable"] = settings.Variable,
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }, ex);
                throw;
            }
        }
    }
}
