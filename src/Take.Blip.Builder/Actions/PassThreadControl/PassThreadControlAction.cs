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
                var resource = BuildThreadControlResource(context, settings, settings.ControlType);
                var command = ThreadControlCommandFactory.CreateSetCommand(resource);
                var response = await _sender.ProcessCommandAsync(command, cancellationToken);
                ThrowIfFailure(response);

                this.LogExecution(_blipMonitoringLogger, context, new JObject
                {
                    ["uri"] = command.Uri.ToString(),
                    ["controlType"] = settings.ControlType.ToString(),
                    ["targetRole"] = settings.TargetRole?.ToString(),
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                });
            }
            catch (Exception ex)
            {
                this.LogError(_blipMonitoringLogger, context, new JObject
                {
                    ["controlType"] = settings.ControlType.ToString(),
                    ["targetRole"] = settings.TargetRole?.ToString(),
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }, ex);
                throw;
            }
        }

        private static ThreadControlCommandResource BuildThreadControlResource(
            IContext context,
            PassThreadControlSettings settings,
            ThreadControlType controlType)
        {
            var resource = new ThreadControlCommandResource
            {
                ControlType = controlType,
                TargetRole = settings.TargetRole,
                Metadata = settings.Metadata,
                UserIdentity = context?.UserIdentity?.ToString()
            };

            return resource;
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
