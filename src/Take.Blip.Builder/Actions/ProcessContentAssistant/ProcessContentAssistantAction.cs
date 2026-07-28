using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using Take.Blip.Ai.Bot.Monitoring.Abstractions;
using Take.Blip.Ai.Bot.Monitoring.Abstractions.Models;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.Blip.Builder.Actions.ProcessContentAssistant
{
    /// <summary>
    /// Content Assistant action
    /// </summary>
    public class ProcessContentAssistantAction : ActionBase<ProcessContentAssistantSettings>
    {
        private readonly IArtificialIntelligenceExtension _artificialIntelligenceExtension;
        private readonly IBlipLogger _blipMonitoringLogger;
        private static readonly string[] OUTPUT_PARAMETERS_NAME = new string[] { nameof(ProcessContentAssistantSettings.OutputVariable).ToCamelCase() };
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public ProcessContentAssistantAction(IArtificialIntelligenceExtension artificialIntelligenceExtension, IBlipLogger? blipMonitoringLogger = null) : base(nameof(ProcessContentAssistant), OUTPUT_PARAMETERS_NAME)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            _artificialIntelligenceExtension = artificialIntelligenceExtension;
            _blipMonitoringLogger = blipMonitoringLogger ?? new NullBlipLogger();
        }

        /// <summary>
        /// Get content result to a given input
        /// </summary>
        /// <param name="context">bot context</param>
        /// <param name="settings">ContentAssistant settings</param
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task ExecuteAsync(IContext context, ProcessContentAssistantSettings settings, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var tags = JsonSerializer.Serialize<string[]>(settings?.Tags?.Split(",") ?? new string[0]);

                var contentAssistantResource = new AnalysisRequest
                {
                    Text = settings.Text,
                    Score = settings.Score.HasValue ? settings.Score.Value / Constants.PERCENTAGE_DENOMINATOR : context.Flow.BuilderConfiguration.MinimumIntentScore.Value,
                    Extras = new Dictionary<string, string>
                    {
                        ["Tags"] = tags,
                        ["MessageId"] = context.Input.Message.Id,
                        ["UserIdentity"] = context.UserIdentity.ToString()
                    }
                };

                var result = string.Empty;

                if (settings.V2)
                {
                    var contentResult = await _artificialIntelligenceExtension.GetContentAssistantAsync(
                        contentAssistantResource,
                        cancellationToken);

                    result = contentResult.SerializeContentAssistantActionResponse() ?? string.Empty;
                }
                else
                {
                    var contentResult = await _artificialIntelligenceExtension.GetContentResultAsync(
                        contentAssistantResource,
                        cancellationToken: cancellationToken);
                    result = contentResult.SerializeContentAssistantActionResponse() ?? string.Empty;
                }

                await SetContentResultAsync(context, settings.OutputVariable, result, cancellationToken);

                this.LogExecution(_blipMonitoringLogger, context, new JObject
                {
                    ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                    ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                    ["outputVariable"] = settings.OutputVariable,
                    ["v2"] = settings.V2,
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                });
            }
            catch (Exception ex)
            {
                this.LogError(_blipMonitoringLogger, context, new JObject
                {
                    ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                    ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                    ["outputVariable"] = settings.OutputVariable,
                    ["v2"] = settings.V2,
                    ["elapsedMilliseconds"] = sw.ElapsedMilliseconds,
                }, ex);
                throw;
            }
        }

        private async Task SetContentResultAsync(
          IContext context, string outputVariable, string result, CancellationToken cancellationToken)
        {
            await context.SetVariableAsync(outputVariable, result, cancellationToken);
        }
    }
}