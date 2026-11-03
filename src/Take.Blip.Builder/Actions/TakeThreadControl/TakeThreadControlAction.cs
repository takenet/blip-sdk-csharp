using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Blip.Ai.Bot.Monitoring.Logging.Interface;
using Blip.Ai.Bot.Monitoring.Logging.Services;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Actions.ThreadControl;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.TakeThreadControl
{
    public class TakeThreadControlAction : ActionBase<TakeThreadControlSettings>
    {
        private const string MultiPartnerConversationControlTakeType = "application/vnd.iris.multi-partner-conversation-control-take+json";
        private readonly ISender _sender;
        private readonly IBlipLogger _blipMonitoringLogger;

        public TakeThreadControlAction(ISender sender, IRedirectManager redirectManager, IBlipLogger? blipMonitoringLogger = null)
            : base(nameof(TakeThreadControl))
        {
            _sender = sender;
            _ = redirectManager;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(IContext context, TakeThreadControlSettings settings, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                if (ShouldSkipTakeCommand(context))
                {
                    this.LogExecution(_blipMonitoringLogger, context, new JObject
                    {
                        ["controlType"] = ThreadControlType.Take.ToString(),
                        ["skipped"] = true,
                        ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                    });

                    return;
                }

                var resource = BuildThreadControlResource(context, settings);

                var command = ThreadControlCommandFactory.CreateSetCommand(resource);
                var response = await _sender.ProcessCommandAsync(command, cancellationToken);
                ThrowIfFailure(response);

                this.LogExecution(_blipMonitoringLogger, context, new JObject
                {
                    ["controlType"] = ThreadControlType.Take.ToString(),
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                });
            }
            catch (Exception ex)
            {
                this.LogError(_blipMonitoringLogger, context, new JObject
                {
                    ["controlType"] = ThreadControlType.Take.ToString(),
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }, ex);
                throw;
            }
        }

        private static ThreadControlCommandResource BuildThreadControlResource(
            IContext context,
            TakeThreadControlSettings settings)
        {
            var resource = new ThreadControlCommandResource
            {
                ControlType = ThreadControlType.Take,
                Metadata = settings.Metadata,
                UserIdentity = context?.UserIdentity?.ToString()
            };

            return resource;
        }

        private static bool ShouldSkipTakeCommand(IContext context)
        {
            var message = context?.Input?.Message;
            var contentMediaType = message?.Content?.GetMediaType()?.ToString();
            if (IsTakeControlPayloadType(contentMediaType))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(context?.Input?.SerializedMessage))
            {
                return false;
            }

            JObject envelope;
            try
            {
                envelope = JObject.Parse(context.Input.SerializedMessage);
            }
            catch
            {
                return false;
            }

            var envelopeType = envelope.SelectToken("type")?.Value<string>();
            if (IsTakeControlPayloadType(envelopeType))
            {
                return true;
            }

            var documentContainerType = envelope.SelectToken("content.documentContainer.type")?.Value<string>();
            return IsTakeControlPayloadType(documentContainerType);
        }

        private static bool IsTakeControlPayloadType(string type) =>
            string.Equals(type, MultiPartnerConversationControlTakeType, StringComparison.OrdinalIgnoreCase);

        private static void ThrowIfFailure(Command response)
        {
            if (response?.Status == CommandStatus.Failure)
            {
                var reason = response.Reason != null
                    ? $"{response.Reason.Code} - {response.Reason.Description}"
                    : "No reason returned";
                throw new InvalidOperationException($"Take thread control failed: {reason}");
            }
        }
    }
}
