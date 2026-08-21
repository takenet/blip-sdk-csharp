using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Blip.Ai.Bot.Monitoring.Logging.Interface;
using Blip.Ai.Bot.Monitoring.Logging.Models;
using Blip.Ai.Bot.Monitoring.Logging.Services;
using Lime.Protocol;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Actions.SetVariable
{
    public class SetVariableAction : ActionBase<SetVariableSettings>
    {
        private static readonly string[] OUTPUT_PARAMETERS_NAME = new string[]
        {
            nameof(SetVariableSettings.Variable).ToCamelCase(),
        };
        private readonly IBlipLogger _blipMonitoringLogger;

        public SetVariableAction(IBlipLogger? blipMonitoringLogger = null)
            : base(nameof(SetVariable), OUTPUT_PARAMETERS_NAME)
        {
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        public override async Task ExecuteAsync(
            IContext context,
            SetVariableSettings settings,
            CancellationToken cancellationToken
        )
        {
            var sw = Stopwatch.StartNew();
            var expiration = settings.Expiration.HasValue
                ? TimeSpan.FromSeconds(settings.Expiration.Value)
                : default(TimeSpan);

            try
            {
                await context.SetVariableAsync(
                    settings.Variable,
                    settings.Value,
                    cancellationToken,
                    expiration
                );

                this.LogExecution(
                    _blipMonitoringLogger,
                    context,
                    new JObject
                    {
                        ["variable"] = settings.Variable,
                        ["expiration"] = settings.Expiration,
                        ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                    },
                    new JObject { ["value"] = settings.Value }
                );
            }
            catch (Exception ex)
            {
                this.LogError(
                    _blipMonitoringLogger,
                    context,
                    new JObject
                    {
                        ["variable"] = settings.Variable,
                        ["expiration"] = settings.Expiration,
                        ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                    },
                    ex
                );
                throw;
            }
        }
    }
}
