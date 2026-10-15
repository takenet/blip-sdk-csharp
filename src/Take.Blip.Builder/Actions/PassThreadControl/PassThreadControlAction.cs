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

namespace Take.Blip.Builder.Actions.PassThreadControl
{
    public class PassThreadControlAction : ActionBase<PassThreadControlSettings>
    {
        private readonly ISender _sender;
        private readonly IBlipLogger _blipMonitoringLogger;

        public PassThreadControlAction(ISender sender, IBlipLogger? blipMonitoringLogger = null)
            : base(nameof(PassThreadControl))
        {
            _sender = sender;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(IContext context, PassThreadControlSettings settings, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var resource = new JObject();

                if (!string.IsNullOrWhiteSpace(settings.TargetRole))
                {
                    resource["targetRole"] = settings.TargetRole;
                }

                if (!string.IsNullOrWhiteSpace(settings.Metadata))
                {
                    resource["metadata"] = settings.Metadata;
                }

                var command = ThreadControlCommandFactory.CreateSetCommand(ThreadControlConstants.PassUri, resource);
                var response = await _sender.ProcessCommandAsync(command, cancellationToken);
                ThrowIfFailure(response);

                this.LogExecution(_blipMonitoringLogger, context, new JObject
                {
                    ["uri"] = command.Uri.ToString(),
                    ["targetRole"] = settings.TargetRole,
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                });
            }
            catch (Exception ex)
            {
                this.LogError(_blipMonitoringLogger, context, new JObject
                {
                    ["targetRole"] = settings.TargetRole,
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
                throw new InvalidOperationException($"Pass thread control failed: {reason}");
            }
        }
    }
}
