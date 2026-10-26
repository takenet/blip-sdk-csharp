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
        private readonly ISender _sender;
        private readonly IRedirectManager _redirectManager;
        private readonly IBlipLogger _blipMonitoringLogger;

        public TakeThreadControlAction(ISender sender, IRedirectManager redirectManager, IBlipLogger? blipMonitoringLogger = null)
            : base(nameof(TakeThreadControl))
        {
            _sender = sender;
            _redirectManager = redirectManager;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(IContext context, TakeThreadControlSettings settings, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var resource = BuildThreadControlResource(context, settings);

                var command = ThreadControlCommandFactory.CreateSetCommand(resource);
                var response = await _sender.ProcessCommandAsync(command, cancellationToken);
                ThrowIfFailure(response);

                await RedirectAsync(context, settings, cancellationToken);

                this.LogExecution(_blipMonitoringLogger, context, new JObject
                {
                    ["controlType"] = ThreadControlType.Take.ToString(),
                    ["service"] = settings.Service,
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                });
            }
            catch (Exception ex)
            {
                this.LogError(_blipMonitoringLogger, context, new JObject
                {
                    ["controlType"] = ThreadControlType.Take.ToString(),
                    ["service"] = settings.Service,
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

        private async Task RedirectAsync(IContext context, TakeThreadControlSettings settings, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(settings.Service))
            {
                return;
            }

            var redirectSettings = new JObject
            {
                ["address"] = settings.Service
            };

            if (!string.IsNullOrWhiteSpace(settings.Message))
            {
                redirectSettings["context"] = new JObject
                {
                    ["type"] = "text/plain",
                    ["value"] = settings.Message,
                };
            }

            var redirect = redirectSettings.ToObject<Lime.Messaging.Contents.Redirect>(LimeSerializerContainer.Serializer);
            await _redirectManager.RedirectUserAsync(context, redirect, cancellationToken);
        }

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
