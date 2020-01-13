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
        private readonly Lazy<Task<ContentResult>> _lazyGetContentResult;
        private readonly Lazy<string> _lazySerializedMessage;
        private readonly IArtificialIntelligenceExtension _artificialIntelligenceExtension;
        private readonly CancellationToken _cancellationToken;

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
            _artificialIntelligenceExtension = artificialIntelligenceExtension;
            _cancellationToken = cancellationToken;
            _lazyAnalyzedContent = CreateLazyAnalyzedContent(userIdentity);
            _lazySerializedMessage = CreateLazySerializedMessage(envelopeSerializer);
            _lazyGetContentResult = CreateLazyGetContentResult();
        }

        private Lazy<Task<AnalysisResponse>> CreateLazyAnalyzedContent(Identity userIdentity) => new Lazy<Task<AnalysisResponse>>(async () =>
        {
            // Only analyze the input if the type is plain text or analyzable metadata is true.
            if (!_analyzable.Value && Content.GetMediaType() != PlainText.MediaType) return null;

            try
            {
                return await _artificialIntelligenceExtension.AnalyzeAsync(
                    new AnalysisRequest
                    {
                        Text = _lazySerializedContent.Value,
                        Extras = new Dictionary<string, string>
                        {
                            ["MessageId"] = Message.Id,
                            ["UserIdentity"] = userIdentity.ToString()
                        }
                    },
                    _cancellationToken);
            }
            catch (LimeException)
            {
                return null;
            }
        });

        private Lazy<string> CreateLazySerializedMessage(IEnvelopeSerializer envelopeSerializer) => new Lazy<string>(() =>
        {
            if (Message != null)
            {
                return envelopeSerializer.Serialize(Message);
            }
            return null;
        });

        private Lazy<Task<ContentResult>> CreateLazyGetContentResult() => new Lazy<Task<ContentResult>>(async () =>
        {
            var intentId = (await GetIntentAsync())?.Id;
            if (intentId == null) return null;
            var entityValues = (await AnalyzedContent)?
                .Entities?
                .Select(entity => entity.Value)
                .ToArray();
            try
            {
                return await _artificialIntelligenceExtension.GetContentResultAsync(
                   new ContentCombination
                   {
                       Intent = intentId,
                       Entities = entityValues
                   },
                   _cancellationToken);
            }
            catch (LimeException)
            {
                return null;
            }
        });

        public Message Message { get; }

        public Document Content => Message.Content;

        public string SerializedContent => _lazySerializedContent.Value;

        public string SerializedMessage => _lazySerializedMessage.Value;

        public Task<AnalysisResponse> AnalyzedContent => _lazyAnalyzedContent.Value;

        public Task<ContentResult> ContentResult => _lazyGetContentResult.Value;

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