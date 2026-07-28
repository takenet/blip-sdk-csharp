using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
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

                _blipMonitoringLogger.ActionExecution(new LogInput
                {
                    Title = "ProcessContentAssistant",
                    EventType = "ActionExecution",
                    StateId = context.GetCurrentStateId(),
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["outputVariable"] = settings.OutputVariable,
                        ["v2"] = settings.V2,
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
                    Title = "ProcessContentAssistant",
                    EventType = "ActionExecution",
                    StateId = context.GetCurrentStateId(),
                    Data = new JObject
                    {
                        ["flowId"] = context.Flow?.Id,
                        ["actionId"] = context.GetCurrentActionTrace()?.ActionId,
                        ["actionTitle"] = context.GetCurrentActionTrace()?.ActionTitle,
                        ["outputVariable"] = settings.OutputVariable,
                        ["v2"] = settings.V2,
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

        private async Task SetContentResultAsync(
          IContext context, string outputVariable, string result, CancellationToken cancellationToken)
        {
            await context.SetVariableAsync(outputVariable, result, cancellationToken);
        }
    }
}