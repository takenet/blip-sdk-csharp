using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Blip.Ai.Bot.Monitoring.Logging.Interface;
using Blip.Ai.Bot.Monitoring.Logging.Services;
using Blip.Ai.Bot.Monitoring.Logging.Models;

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

                this.LogExecution(_blipMonitoringLogger, context, new JObject
                {
                    ["variable"] = settings.Variable,
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                });
            }
            catch (Exception ex)
            {
                this.LogError(_blipMonitoringLogger, context, new JObject
                {
                    ["variable"] = settings.Variable,
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }, ex);
                throw;
            }
        }
    }
}
