using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using SmartFormat;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.Blip.Client.Extensions.ArtificialIntelligence
{
    public class ArtificialIntelligenceExtension : ExtensionBase, IArtificialIntelligenceExtension
    {
        private static readonly Node ArtificialIntelligenceAddress = Node.Parse($"postmaster@ai.{Constants.DEFAULT_DOMAIN}");

        public ArtificialIntelligenceExtension(ISender sender)
            : base(sender)
        {
        }

        public Task<AnalysisResponse> AnalyzeAsync(AnalysisRequest analysisRequest, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync<AnalysisResponse>(
                CreateSetCommandRequest(
                    analysisRequest, UriTemplates.ANALYSIS, ArtificialIntelligenceAddress),
                cancellationToken);

        public Task<ContentResult> GetContentResultAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync<ContentResult>(
                CreateSetCommandRequest(
                    document, UriTemplates.CONTENT_ANALYSIS, ArtificialIntelligenceAddress),
                cancellationToken);

        public Task SendFeedbackAsync(string analysisId, AnalysisFeedback analysisFeedback,
            CancellationToken cancellationToken = new CancellationToken())
            => ProcessCommandAsync<AnalysisFeedback>(
                CreateSetCommandRequest(
                    analysisFeedback,
                    Smart.Format(UriTemplates.ANALYSIS_FEEDBACK, new { analysisId }),
                    ArtificialIntelligenceAddress),
                cancellationToken);

        public Task<Answer> GetAnswerAsync(string intentionId, string answerId, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync<Answer>(
                CreateGetCommandRequest(
                    Smart.Format(UriTemplates.INTENTION_ANSWER, new { intentionId, answerId }),
                    ArtificialIntelligenceAddress),
                cancellationToken);

        public Task<DocumentCollection> GetAnswersAsync(string intentionId, int skip = 0, int take = 100, bool ascending = true, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync<DocumentCollection>(
                CreateGetCommandRequest(
                    $"{Smart.Format(UriTemplates.INTENTION_ANSWERS, new { intentionId })}?{GetQueryString(skip, take, @ascending)}",
                    ArtificialIntelligenceAddress),
                cancellationToken);

        public Task<DocumentCollection> GetEntitiesAsync(int skip = 0, int take = 100, bool ascending = true, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync<DocumentCollection>(
                CreateGetCommandRequest(
                    $"{UriTemplates.ENTITIES}?{GetQueryString(skip, take, @ascending)}",
                    ArtificialIntelligenceAddress),
                cancellationToken);

        public Task<Entity> GetEntityAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync<Entity>(
                CreateGetCommandRequest(
                    $"{Smart.Format(UriTemplates.ENTITY, new { id })}",
                    ArtificialIntelligenceAddress),
                cancellationToken);

        public Task<Intention> GetIntentionAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync<Intention>(
                CreateGetCommandRequest(
                    $"{Smart.Format(UriTemplates.INTENTION, new { id })}",
                    ArtificialIntelligenceAddress),
                cancellationToken);

        public Task<DocumentCollection> GetIntentionsAsync(int skip = 0, int take = 100, bool ascending = true, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync<DocumentCollection>(
                CreateGetCommandRequest(
                    $"{UriTemplates.INTENTIONS}?{GetQueryString(skip, take, @ascending)}",
                    ArtificialIntelligenceAddress),
                cancellationToken);

        public Task<DocumentCollection> GetModelsAsync(int skip = 0, int take = 100, bool ascending = true, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync<DocumentCollection>(
                CreateGetCommandRequest(
                    $"{UriTemplates.MODELS}?{GetQueryString(skip, take, @ascending)}",
                    ArtificialIntelligenceAddress),
                cancellationToken);

        public Task<Question> GetQuestionAsync(string intentionId, string questionId, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync<Question>(
                CreateGetCommandRequest(
                    $"{Smart.Format(UriTemplates.INTENTION_QUESTION, new { intentionId, questionId })}",
                    ArtificialIntelligenceAddress),
                cancellationToken);

        public Task<DocumentCollection> GetQuestionsAsync(string intentionId, int skip = 0, int take = 100, bool ascending = true, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync<DocumentCollection>(
                CreateGetCommandRequest(
                    $"{Smart.Format(UriTemplates.INTENTION_QUESTIONS, new { intentionId })}?{GetQueryString(skip, take, @ascending)}",
                    ArtificialIntelligenceAddress),
                cancellationToken);

        public Task PublishModelAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync(
                CreateSetCommandRequest(
                    new ModelPublishing { Id = id },
                    UriTemplates.MODELS,
                    ArtificialIntelligenceAddress),
                cancellationToken);

        public Task SetAnswersAsync(string intentionId, IEnumerable<Answer> answers, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync(
                CreateSetCommandRequest(
                    new DocumentCollection { Items = answers.ToArray(), ItemType = Answer.MediaType },
                    Smart.Format(UriTemplates.INTENTION_ANSWERS, new { intentionId }),
                    ArtificialIntelligenceAddress),
                cancellationToken);

        public Task<Entity> SetEntityAsync(Entity entity, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync<Entity>(
                CreateSetCommandRequest(
                    entity,
                    UriTemplates.ENTITIES,
                    ArtificialIntelligenceAddress),
                cancellationToken);

        public Task<Intention> SetIntentionAsync(Intention intention, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync<Intention>(
                CreateSetCommandRequest(
                    intention,
                    UriTemplates.INTENTIONS,
                    ArtificialIntelligenceAddress),
                cancellationToken);

        public Task SetQuestionsAsync(string intentionId, IEnumerable<Question> questions, CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync<Question>(
                CreateSetCommandRequest(
                    new DocumentCollection { Items = questions.ToArray(), ItemType = Question.MediaType },
                    Smart.Format(UriTemplates.INTENTION_QUESTIONS, new { intentionId }),
                    ArtificialIntelligenceAddress),
                cancellationToken);

        public Task TrainModelAsync(CancellationToken cancellationToken = default(CancellationToken))
            => ProcessCommandAsync(
                CreateSetCommandRequest(
                    new ModelTraining(),
                    UriTemplates.MODELS,
                    ArtificialIntelligenceAddress),
                cancellationToken);

        private static string GetQueryString(int skip, int take, bool ascending) => $"$skip={skip.ToString()}&$take={take.ToString()}&$ascending={ascending.ToString()}";
    }
}
