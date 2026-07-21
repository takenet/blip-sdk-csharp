using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;

namespace Take.Blip.Builder.Actions.Redirect
{
    public class RedirectAction : IAction
    {
        private readonly IRedirectManager _redirectManager;
        private readonly ILogger _logger;
        private readonly IBlipLogger _blipMonitoringLogger;
        private const string REDIRECT_TEST_LOG = "REDIRECT_TEST_LOG";

        public RedirectAction(IRedirectManager redirectManager, ILogger logger, IBlipLogger? blipMonitoringLogger = null)
        {
            _redirectManager = redirectManager;
            _logger = logger;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public string Type => nameof(Redirect);

        public string[]? OutputVariables => null;

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            try
            {
                var redirect = settings.ToObject<Lime.Messaging.Contents.Redirect>(LimeSerializerContainer.Serializer);
                if (context.Input.Message.Metadata != null &&
                    context.Input.Message.Metadata.TryGetValue(REDIRECT_TEST_LOG, out var metadataRedirectAddress) &&
                    redirect.Address.ToString().Contains(metadataRedirectAddress))
                {
                    _logger.Warning("#({REDIRECT_TEST_LOG})# ({Message}) - ({Address}) - ({Context})",
                        REDIRECT_TEST_LOG,
                        context.Input.Message,
                        redirect.Address,
                        redirect.Context);
                }

                await _redirectManager.RedirectUserAsync(context, redirect, cancellationToken);

                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "Redirect",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["address"] = redirect.Address?.ToString(),
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
                    Title = "Redirect",
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
