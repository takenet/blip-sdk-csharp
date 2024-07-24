using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public ProcessContentAssistantAction(IArtificialIntelligenceExtension artificialIntelligenceExtension) : base(nameof(ProcessContentAssistant))
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            _artificialIntelligenceExtension = artificialIntelligenceExtension;
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
        }

        private async Task SetContentResultAsync(
          IContext context, string outputVariable, string result, CancellationToken cancellationToken)
        {
            await context.SetVariableAsync(outputVariable, result, cancellationToken);
        }
    }
}