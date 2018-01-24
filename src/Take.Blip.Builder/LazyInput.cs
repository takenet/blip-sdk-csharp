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
    internal class LazyInput
    {
        private readonly IDictionary<string, string> _flowConfiguration;
        private readonly Lazy<string> _inputSource;
        private readonly Lazy<Task<AnalysisResponse>> _analysisSource;

        public LazyInput(
            Document input,
            IDictionary<string, string> flowConfiguration,
            IDocumentSerializer documentSerializer,
            IArtificialIntelligenceExtension artificialIntelligenceExtension,
            CancellationToken cancellationToken)
        {
            _flowConfiguration = flowConfiguration;
            Input = input;
            _inputSource = new Lazy<string>(() => documentSerializer.Serialize(input));
            _analysisSource = new Lazy<Task<AnalysisResponse>>(async () =>
            {
                try
                {
                    return await artificialIntelligenceExtension.AnalyzeAsync(
                        new AnalysisRequest
                        {
                            Text = _inputSource.Value
                        },
                        cancellationToken);
                }
                catch (LimeException)
                {
                    return null;
                }
            });
        }

        public Document Input { get; }

        public string SerializedInput => _inputSource.Value;

        public Task<AnalysisResponse> AnalyzedInput => _analysisSource.Value;

        public async Task<IntentionResponse> GetIntentAsync()
        {
            double minimumIntentScore;

            if (_flowConfiguration == null ||
                !_flowConfiguration.TryGetValue($"builder:{nameof(minimumIntentScore)}", out var minimumScoreValue) ||
                !double.TryParse(minimumScoreValue, NumberStyles.Float, CultureInfo.InvariantCulture, out minimumIntentScore))
            {               
                minimumIntentScore = 0.5;
            }

            return (await AnalyzedInput)?
                .Intentions?
                .OrderByDescending(i => i.Score)
                .FirstOrDefault(i => i.Score >= minimumIntentScore);
        }

        public async Task<EntityResponse> GetEntityValue(string entityName)
        {
            return (await AnalyzedInput)?
                .Entities?
                .FirstOrDefault(e => e.Name != null && e.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        }
    }
}