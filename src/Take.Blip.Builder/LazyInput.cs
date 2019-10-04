using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Take.Blip.Builder.Models;
using Take.Blip.Client;
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
        private readonly BuilderConfiguration _builderConfiguration;
        private readonly Lazy<string> _lazySerializedContent;
        private readonly Lazy<bool> _analyzable;
        private readonly Lazy<Task<AnalysisResponse>> _lazyAnalyzedContent;
        private readonly Lazy<string> _lazySerializedMessage;

        public LazyInput(
            Message message,
            Identity userIdentity,
            BuilderConfiguration builderConfiguration,
            IDocumentSerializer documentSerializer,
            IEnvelopeSerializer envelopeSerializer,
            IArtificialIntelligenceExtension artificialIntelligenceExtension,
            CancellationToken cancellationToken)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            _builderConfiguration = builderConfiguration ?? throw new ArgumentNullException(nameof(builderConfiguration));
            _lazySerializedContent = new Lazy<string>(() => documentSerializer.Serialize(Content));
            _analyzable = new Lazy<bool>(() =>
            {
                string result = null;
                Message?.Metadata?.TryGetValue("builder.analyzable", out result);
                return result?.ToLower() == "true";
            });
            _lazyAnalyzedContent = new Lazy<Task<AnalysisResponse>>(async () =>
            {
                // Only analyze the input if the type is plain text or analyzable metadata is true.
                if (!_analyzable.Value && Content.GetMediaType() != PlainText.MediaType) return null;

                try
                {
                    return await artificialIntelligenceExtension.AnalyzeAsync(
                        new AnalysisRequest
                        {
                            Text = _lazySerializedContent.Value,
                            Extras = new Dictionary<string, string>
                            {
                                ["MessageId"] = Message.Id,
                                ["UserIdentity"] = userIdentity.ToString()
                            }
                        },
                        cancellationToken);
                }
                catch (LimeException)
                {
                    return null;
                }
            });
            _lazySerializedMessage = new Lazy<string>(() =>
            {
                if (Message != null)
                {
                    return envelopeSerializer.Serialize(Message);
                }

                return null;
            });
        }

        public Message Message { get; }

        public Document Content => Message.Content;

        public string SerializedContent => _lazySerializedContent.Value;

        public string SerializedMessage => _lazySerializedMessage.Value;

        public Task<AnalysisResponse> AnalyzedContent => _lazyAnalyzedContent.Value;

        public async Task<IntentionResponse> GetIntentAsync()
        {
            var minimumIntentScore = _builderConfiguration?.MinimumIntentScore ?? 0.5;

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