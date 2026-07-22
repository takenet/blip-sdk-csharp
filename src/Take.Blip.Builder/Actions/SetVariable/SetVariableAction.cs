using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;

namespace Take.Blip.Builder.Actions.SetVariable
{
    public class SetVariableAction : ActionBase<SetVariableSettings>
    {
        private static readonly string[] OUTPUT_PARAMETERS_NAME = new string[] { nameof(SetVariableSettings.Variable).ToCamelCase() };
        private readonly IBlipLogger _blipMonitoringLogger;

        public SetVariableAction(IBlipLogger? blipMonitoringLogger = null)
            : base(nameof(SetVariable), OUTPUT_PARAMETERS_NAME)
        {
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(IContext context, SetVariableSettings settings, CancellationToken cancellationToken)
        {
            var expiration = settings.Expiration.HasValue
                ? TimeSpan.FromSeconds(settings.Expiration.Value)
                : default(TimeSpan);

            try
            {
                await context.SetVariableAsync(settings.Variable, settings.Value, cancellationToken, expiration);

                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "SetVariable",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["variable"] = settings.Variable,
                        ["expiration"] = settings.Expiration,
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
                    Title = "SetVariable",
                    EventType = "ActionExecution",
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["variable"] = settings.Variable,
                        ["expiration"] = settings.Expiration,
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
