using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Lime.Protocol;
using Newtonsoft.Json;
using Take.Blip.Builder.Models;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Esprima.Ast;

namespace Take.Blip.Builder.Actions.ProcessContentAssistant
{
    /// <summary>
    /// Content Assistant action
    /// </summary>
    public static class ProcessContentAssistantExtencion
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
                Entities = bestCombinationFound?.Entities.ToList() ?? new List<string>()
            });
        }

        /// <summary>
        /// Serialeze result v1
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static string SerializeContentAssistantActionResponse(this Command document)
        {
            var contentResult = document.As<ContentAssistant>();

            var bestCombinationFound = contentResult.Combinations?.FirstOrDefault();  // The first combination is that has the best score
            var result = contentResult.Results?.FirstOrDefault();

            return JsonConvert.SerializeObject(new ContentAssistantActionResponse
            {
                HasCombination = result?.Content != null,
                Value = result?.Content?.ToString() ?? string.Empty,
                Intent = bestCombinationFound?.Intent ?? string.Empty,
                Entities = bestCombinationFound?.Entities.ToList() ?? new List<string>()
            });
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class ContentAssistant : Document
    {
        /// <summary>
        /// Type document
        /// </summary>
        public const string MIME_TYPE = "application/vnd.iris.ai.content-assistant+json";

        /// <summary>
        /// Ctor
        /// </summary>
        public ContentAssistant() : base(MIME_TYPE)
        {
        }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "combinations")]
        public ContentCombination[] Combinations { get; set; }

        /// <summary>
        ///
        /// </summary>
        [DataMember(Name = "results")]
        public Message[] Results { get; set; }
    }
}