using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Blip.Ai.Bot.Monitoring.Logging.Interface;
using Blip.Ai.Bot.Monitoring.Logging.Models;
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
        private readonly IBlipLogger _blipMonitoringLogger;

        public TakeThreadControlAction(ISender sender, IBlipLogger? blipMonitoringLogger = null)
            : base(nameof(TakeThreadControl))
        {
            _sender = sender;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(IContext context, TakeThreadControlSettings settings, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var resource = new JObject();

                if (!string.IsNullOrWhiteSpace(settings.Metadata))
                {
                    resource["metadata"] = settings.Metadata;
                }

                var command = ThreadControlCommandFactory.CreateSetCommand(ThreadControlConstants.TakeUri, resource);
                var response = await _sender.ProcessCommandAsync(command, cancellationToken);
                ThrowIfFailure(response);

                this.LogExecution(_blipMonitoringLogger, context, new JObject
                {
                    ["uri"] = command.Uri.ToString(),
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                });
            }
            catch (Exception ex)
            {
                this.LogError(_blipMonitoringLogger, context, new JObject
                {
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }, ex);
                throw;
            }
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
