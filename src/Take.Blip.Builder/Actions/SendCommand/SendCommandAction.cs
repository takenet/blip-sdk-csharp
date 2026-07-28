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

                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "SendCommand",
                    EventType = "ActionExecution",
                    StateId = context.GetCurrentStateId(),
                    Data = new JObject
                    {
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["uri"] = command.Uri?.ToString(),
                        ["method"] = command.Method.ToString(),
                        ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
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
                    Title = "SendCommand",
                    EventType = "ActionExecution",
                    StateId = context.GetCurrentStateId(),
                    Data = new JObject
                    {
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
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
