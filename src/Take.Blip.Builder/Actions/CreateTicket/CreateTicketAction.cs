using System;
using System.Threading;
using System.Threading.Tasks;
using Esprima;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;
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

                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "CreateTicket",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["customerIdentity"] = ticket.CustomerIdentity?.ToString(),
                        ["outputVariable"] = settings.Variable,
                        ["ticketId"] = createdTicket?.Id,
                        ["success"] = true,
                    },
                    FlowVersion = context.Flow?.Version,
                    Channel = context.Input.Message?.From?.Domain,
                    IdMessage = context.Input.Message?.Id,
                    From = context.UserIdentity?.ToString(),
                    To = context.OwnerIdentity?.ToString(),
                    OriginalFrom = context.Input.Message?.From,
                    OriginalTo = context.Input.Message?.To
                });
            }
            catch (Exception ex)
            {
                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "CreateTicket",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["customerIdentity"] = settings.CustomerIdentity?.ToString(),
                        ["outputVariable"] = settings.Variable,
                        ["success"] = false,
                        ["error"] = ex.ToString(),
                    },
                    FlowVersion = context.Flow?.Version,
                    Channel = context.Input.Message?.From?.Domain,
                    IdMessage = context.Input.Message?.Id,
                    From = context.UserIdentity?.ToString(),
                    To = context.OwnerIdentity?.ToString(),
                    OriginalFrom = context.Input.Message?.From,
                    OriginalTo = context.Input.Message?.To
                });
                throw;
            }
        }
    }
}
