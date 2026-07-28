using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;

namespace Take.Blip.Builder.Actions.DeleteVariable
{
    public class DeleteVariableAction : ActionBase<DeleteVariableSettings>
    {
        private readonly IBlipLogger _blipMonitoringLogger;

        public DeleteVariableAction(IBlipLogger? blipMonitoringLogger = null)
            : base(nameof(DeleteVariable))
        {
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(IContext context, DeleteVariableSettings settings, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await context.DeleteVariableAsync(settings.Variable, cancellationToken);

                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "DeleteVariable",
                    EventType = "ActionExecution",
                    StateId = context.GetCurrentStateId(),
                    Data = new JObject
                    {
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["variable"] = settings.Variable,
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
                    Title = "DeleteVariable",
                    EventType = "ActionExecution",
                    StateId = context.GetCurrentStateId(),
                    Data = new JObject
                    {
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["variable"] = settings.Variable,
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
