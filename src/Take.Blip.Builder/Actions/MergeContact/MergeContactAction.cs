using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Newtonsoft.Json.Linq;
using Serilog;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;
using Take.Blip.Client.Extensions.Contacts;

namespace Take.Blip.Builder.Actions.MergeContact
{
    public class MergeContactAction : IAction
    {
        private readonly IContactExtension _contactExtension;
        private readonly ILogger _logger;
        private readonly IBlipLogger _blipMonitoringLogger;

        public MergeContactAction(IContactExtension contactExtension, ILogger logger, IBlipLogger? blipMonitoringLogger = null)
        {
            _contactExtension = contactExtension;
            _logger = logger;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public string Type => nameof(MergeContact);

        public string[]? OutputVariables => null;

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            try
            {
                var contact = settings.ToObject<Contact>(LimeSerializerContainer.Serializer);
                contact.Identity = contact.Identity;

                _logger.Information("Trying to merge contact values ({Settings}) for UserIdentity {UserIdentity}",
                   settings,
                   context.UserIdentity);

                await _contactExtension.MergeAsync(context.UserIdentity, contact, cancellationToken);
                context.RemoveContact();

                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "MergeContact",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["success"] = true,
                    },
                    FlowVersion = context.Flow?.Version,
                    Channel = context.Input.Message?.From?.Domain,
                    IdMessage = context.Input.Message?.Id,
                    From = context.UserIdentity?.ToString(),
                    To = context.OwnerIdentity?.ToString(),
                    OriginalFrom = context.Input.Message?.From,
                    OriginalTo = context.Input.Message?.To,
                });
            }
            catch (Exception ex)
            {
                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "MergeContact",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["success"] = false,
                        ["error"] = ex.ToString(),
                    },
                    FlowVersion = context.Flow?.Version,
                    Channel = context.Input.Message?.From?.Domain,
                    IdMessage = context.Input.Message?.Id,
                    From = context.UserIdentity?.ToString(),
                    To = context.OwnerIdentity?.ToString(),
                    OriginalFrom = context.Input.Message?.From,
                    OriginalTo = context.Input.Message?.To,
                });
                throw;
            }
        }
    }
}
