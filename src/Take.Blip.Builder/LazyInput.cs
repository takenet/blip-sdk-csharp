using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Allows the lazy evaluation of input bound values.
    /// This optimizes the calls for serialization and the AI extension.
    /// </summary>
    public class LazyInput
    {
        private readonly IDictionary<string, string> _flowConfiguration;
        private readonly Lazy<string> _lazySerializedContent;
        private readonly Lazy<Task<AnalysisResponse>> _lazyAnalyzedContent;

        public LazyInput(
            Document content,
            IDictionary<string, string> flowConfiguration,
            IDocumentSerializer documentSerializer,
            IArtificialIntelligenceExtension artificialIntelligenceExtension,
            CancellationToken cancellationToken)
        {
            _flowConfiguration = flowConfiguration;
            Content = content;
            _lazySerializedContent = new Lazy<string>(() => documentSerializer.Serialize(content));
            _lazyAnalyzedContent = new Lazy<Task<AnalysisResponse>>(async () =>
            {
                try
                {
                    return await artificialIntelligenceExtension.AnalyzeAsync(
                        new AnalysisRequest
                        {
                            Text = _lazySerializedContent.Value
                        },
                        cancellationToken);
                }
                catch (LimeException)
                {
                    return null;
                }
            });
        }

        public Document Content { get; }

        public string SerializedContent => _lazySerializedContent.Value;

        public Task<AnalysisResponse> AnalyzedContent => _lazyAnalyzedContent.Value;

        public async Task<IntentionResponse> GetIntentAsync()
        {
            double minimumIntentScore;

            if (_flowConfiguration == null ||
                !_flowConfiguration.TryGetValue($"builder:{nameof(minimumIntentScore)}", out var minimumScoreValue) ||
                !double.TryParse(minimumScoreValue, NumberStyles.Float, CultureInfo.InvariantCulture, out minimumIntentScore))
            {               
                minimumIntentScore = 0.5;
            }

            return (await AnalyzedContent)?
                .Intentions?
                .OrderByDescending(i => i.Score)
                .FirstOrDefault(i => i.Score >= minimumIntentScore);
        }

        public async Task<EntityResponse> GetEntityValue(string entityName)
        {
            return (await AnalyzedContent)?
                .Entities?
                .FirstOrDefault(e => e.Name != null && e.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        }
    }
}