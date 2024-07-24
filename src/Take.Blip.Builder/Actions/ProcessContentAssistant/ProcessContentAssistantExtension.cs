using System.Collections.Generic;
using System.Linq;
using Lime.Protocol;
using Newtonsoft.Json;
using Take.Blip.Builder.Models;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.Blip.Builder.Actions.ProcessContentAssistant
{
    /// <summary>
    /// Content Assistant action
    /// </summary>
    public static class ProcessContentAssistantExtension
    {
        /// <summary>
        /// Create command analysis
        /// </summary>
        /// <param name="contentAssistantResource"></param>
        /// <returns></returns>
        public static Command CommandAnalysis(this AnalysisRequest contentAssistantResource)
            => new Command()
            {
                Resource = contentAssistantResource,
                Uri = "/assistant/analysis",
                To = "postmaster@ai.msging.net"
            };

        /// <summary>
        /// Serialeze result v1
        /// </summary>
        /// <param name="contentResult"></param>
        /// <returns></returns>
        public static string SerializeContentAssistantActionResponse(this ContentResult contentResult)
        {
            var bestCombinationFound = contentResult.Combinations?.FirstOrDefault();  // The first combination is that has the best score

            return JsonConvert.SerializeObject(new ContentAssistantActionResponse
            {
                HasCombination = contentResult?.Result?.Content != null,
                Value = contentResult?.Result?.Content?.ToString() ?? string.Empty,
                Intent = bestCombinationFound?.Intent ?? string.Empty,
                Entities = bestCombinationFound?.Entities.ToList() ?? new List<string>(),
                V2 = false
            });
        }

        /// <summary>
        /// Serialeze result v2
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static string SerializeContentAssistantActionResponse(this ContentAssistant contentResult)
        {
            var bestCombinationFound = contentResult.Combinations?.FirstOrDefault();  // The first combination is that has the best score
            var result = contentResult?.Results?.FirstOrDefault();

            return JsonConvert.SerializeObject(new ContentAssistantActionResponse
            {
                HasCombination = result?.Content != null,
                Value = result?.Content?.ToString() ?? string.Empty,
                Intent = bestCombinationFound?.Intent ?? string.Empty,
                Entities = bestCombinationFound?.Entities.ToList() ?? new List<string>(),
                V2 = true
            });
        }
    }
}