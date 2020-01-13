using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.Blip.Client.Extensions.ArtificialIntelligence
{
    /// <summary>
    /// Allows the management of the bot artificial intelligence model.
    /// </summary>
    public interface IArtificialIntelligenceExtension
    {
        /// <summary>
        /// Query the stored intentions.
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="ascending"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DocumentCollection> GetIntentionsAsync(int skip = 0, int take = 100, bool ascending = true, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get an intention by its id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Intention> GetIntentionAsync(string id, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sets an intention.
        /// </summary>
        /// <param name="intention"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Intention> SetIntentionAsync(Intention intention, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Query the stored entities.
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="ascending"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DocumentCollection> GetEntitiesAsync(int skip = 0, int take = 100, bool ascending = true, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get an entity by its id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Entity> GetEntityAsync(string id, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sets an entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Entity> SetEntityAsync(Entity entity, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Query the answers associated with an intention
        /// </summary>
        /// <param name="intentionId"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="ascending"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DocumentCollection> GetAnswersAsync(string intentionId, int skip = 0, int take = 100, bool ascending = true, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get a specific intention answer by its id.
        /// </summary>
        /// <param name="intentionId"></param>
        /// <param name="answerId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Answer> GetAnswerAsync(string intentionId, string answerId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Set the answers for an intention.
        /// </summary>
        /// <param name="intentionId"></param>
        /// <param name="answers"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetAnswersAsync(string intentionId, IEnumerable<Answer> answers, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Query the questions associated with an intention
        /// </summary>
        /// <param name="intentionId"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="ascending"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DocumentCollection> GetQuestionsAsync(string intentionId, int skip = 0, int take = 100, bool ascending = true, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get a specific intention question by its id.
        /// </summary>
        /// <param name="intentionId"></param>
        /// <param name="questionId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Question> GetQuestionAsync(string intentionId, string questionId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Set the questions for an intention.
        /// </summary>
        /// <param name="intentionId"></param>
        /// <param name="questions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetQuestionsAsync(string intentionId, IEnumerable<Question> questions, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Query the existing models for the current identity.
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="ascending"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DocumentCollection> GetModelsAsync(int skip = 0, int take = 100, bool ascending = true, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Train the current model into the registered AI providers.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task TrainModelAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Publishes a model into the provider.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task PublishModelAsync(string id, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Analyze the user input using a published model.
        /// </summary>
        /// <param name="analysisRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<AnalysisResponse> AnalyzeAsync(AnalysisRequest analysisRequest, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get content result for the analyzed user input.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ContentResult> GetContentResultAsync(Document resource, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Send a feedback to a executed analysis to help improving the model.
        /// </summary>
        /// <param name="analysisId"></param>
        /// <param name="analysisFeedback"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SendFeedbackAsync(string analysisId, AnalysisFeedback analysisFeedback, CancellationToken cancellationToken = default(CancellationToken));
    }
}
