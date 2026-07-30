using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions.SendCommand
{
    public class SendCommandAction : IAction
    {
        private readonly ISender _sender;
        private readonly IBlipLogger _blipMonitoringLogger;

        public SendCommandAction(ISender sender, IBlipLogger? blipMonitoringLogger = null)
        {
            _sender = sender;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public string Type => nameof(SendCommand);

        public string[]? OutputVariables => null;

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings), $"The settings are required for '{nameof(SendCommandAction)}' action");

            try
            {
                var command = settings.ToObject<Command>(LimeSerializerContainer.Serializer);
                command.Id = EnvelopeId.NewId();

                await _sender.SendCommandAsync(command, cancellationToken);

                this.LogExecution(_blipMonitoringLogger, context, new JObject
                {
                    ["uri"] = command.Uri?.ToString(),
                    ["method"] = command.Method.ToString(),
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }, new JObject
                {
                    ["Command"] = settings
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
    }
}
