using System;
using System.Threading;
using System.Diagnostics;
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
            var sw = Stopwatch.StartNew();
            var expiration = settings.Expiration.HasValue
                ? TimeSpan.FromSeconds(settings.Expiration.Value)
                : default(TimeSpan);

            try
            {
                await context.SetVariableAsync(settings.Variable, settings.Value, cancellationToken, expiration);

                _blipMonitoringLogger.ActionExecution(context.ToActionLog("SetVariable", new JObject
                {
                    ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                    ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                    ["variable"] = settings.Variable,
                    ["expiration"] = settings.Expiration,
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }));
            }
            catch (Exception ex)
            {
                _blipMonitoringLogger.ErrorEvents(context.ToActionLog("SetVariable", new JObject
                {
                    ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                    ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                    ["variable"] = settings.Variable,
                    ["expiration"] = settings.Expiration,
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }), ex);
                throw;
            }
        }
    }
}
