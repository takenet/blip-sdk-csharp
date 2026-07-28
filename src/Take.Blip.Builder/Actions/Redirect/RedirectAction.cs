using System;
using System.Threading;
using System.Diagnostics;
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
            var sw = Stopwatch.StartNew();
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

                _blipMonitoringLogger.ActionExecution(context.ToActionLog("Redirect", new JObject
                {
                    ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                    ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                    ["address"] = redirect.Address?.ToString(),
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }));
            }
            catch (Exception ex)
            {
                _blipMonitoringLogger.ErrorEvents(context.ToActionLog("Redirect", new JObject
                {
                    ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                    ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }), ex);
                throw;
            }
        }
    }
}
