using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.Client.Extensions
{
    public static class UriTemplates
    {
        /// <summary>
        /// Template for the address aliases.
        /// </summary>
        public const string ALIASES = "/aliases";

        /// <summary>
        /// Template for a specific address alias.
        /// </summary>
        public const string ALIAS_IDENTITY = "/aliases/{aliasIdentity}";

        /// <summary>
        /// Template for a specific address alias.
        /// </summary>
        public const string ALIAS_NODE = "/aliases/{aliasIdentity}/{aliasInstance}";

        /// <summary>
        /// Template for account keys.
        /// </summary>
        public const string ACCOUNT_KEYS = "/account/keys";

        /// <summary>
        /// Template for account key.
        /// </summary>
        public const string ACCOUNT_KEY = "/account/keys/{id}";

        /// <summary>
        /// Template for the bucket storage ids.
        /// </summary>
        public const string BUCKETS = "/buckets";

        /// <summary>
        /// Template for the bucket storage.
        /// </summary>
        public const string BUCKET = "/buckets/{id}";

        /// <summary>
        /// Template for the thread storage.
        /// </summary>
        public const string THREADS = "/threads";

        /// <summary>
        /// Template for the thread storage for an identity.
        /// </summary>
        public const string THREAD = "/threads/{identity}";

        /// <summary>
        /// Template for the event tracks.
        /// </summary>
        public const string EVENT_TRACK = "/event-track";

        /// <summary>
        /// Template for the event tracks categories.
        /// </summary>
        public const string EVENT_TRACK_CATEGORY = "/event-track/{category}";

        /// <summary>
        /// Template for the event tracks category actions.
        /// </summary>
        public const string EVENT_TRACK_CATEGORY_ACTION = "/event-track/{category}/{action}";

        /// <summary>
        /// Template for delegations.
        /// </summary>
        public const string DELEGATIONS = "/delegations";

        /// <summary>
        /// Template for delegations node
        /// </summary>
        public const string DELEGATIONS_IDENTITY = "/delegations/{targetIdentity}";

        /// <summary>
        /// Template for delegations node
        /// </summary>
        public const string DELEGATIONS_NODE = "/delegations/{targetIdentity}/{targetInstance}";

        /// <summary>
        /// Template for accounts.
        /// </summary>
        public const string DIRECTORY_ACCOUNTS = "/accounts";

        /// <summary>
        /// Template for an account resource.
        /// </summary>
        public const string DIRECTORY_ACCOUNT = "/accounts/{accountIdentity}";

        /// <summary>
        /// Template for an account key.
        /// </summary>
        public const string DIRECTORY_ACCOUNT_KEY = "/accounts/{accountIdentity}/key";

        /// <summary>
        /// Template for the server configuration.
        /// </summary>
        public const string CONFIGURATION = "/configuration";

        /// <summary>
        /// Template for the domain configurations.
        /// </summary>
        public const string DOMAIN_CONFIGURATIONS = "/configurations/{domain}";

        /// <summary>
        /// Template for an specific domain configurations value.
        /// </summary>
        public const string DOMAIN_CONFIGURATIONS_VALUE = "/configurations/{domain}/{key}";

        /// <summary>
        /// Template for the linked contacts.
        /// </summary>
        public const string LINKED_CONTACTS = "/contacts/{contactIdentity}/linked";

        /// <summary>
        /// Template for a specific linked contacts resource.
        /// </summary>
        public const string LINKED_CONTACT = "/contacts/{contactIdentity}/linked/{linkedContactIdentity}";

        /// <summary>
        /// Template for the messages.
        /// </summary>
        public const string MESSAGES = "/messages";

        /// <summary>
        /// Template for a specific messages.
        /// </summary>
        public const string MESSAGE = "/messages/{internalId}";

        /// <summary>
        /// Template for a message buffer options.
        /// </summary>
        public const string MESSAGE_BUFFER = "/message-buffer";

        /// <summary>
        /// Template for the messages history.
        /// </summary>
        public const string MESSAGES_HISTORY = "/messages-history";

        public const string MESSAGE_RESPONSE_TIME = "/message-statistic/responseTime";

        /// <summary>
        /// Template for the message word count.
        /// </summary>
        public const string MESSAGE_WORD_COUNT = "/message-statistic/word-count";

        /// <summary>
        /// Template for the notifications.
        /// </summary>
        public const string NOTIFICATIONS = "/notifications";

        /// <summary>
        /// Template for a specific notifications.
        /// </summary>
        public const string NOTIFICATION = "/notifications/{internalId}";

        /// <summary>
        /// Template for the session statistic.
        /// </summary>
        public const string SESSION_STATISTIC = "/session-statistic/{guid}";

        /// <summary>
        /// Template for the remote envelope dispatch command.
        /// </summary>
        public const string REMOTE_SESSION_DISPATCH = "/sessions/{sessionId}?expiration={expiration}";

        /// <summary>
        /// Template for the session.
        /// </summary>
        public const string SESSION = "/sessions/{sessionId}";

        /// <summary>
        /// Template for the pipeline.
        /// </summary>
        public const string PIPELINE = "/pipeline";
        /// <summary>
        /// Template for the pipeline senders.
        /// </summary>
        public const string PIPELINE_SENDERS = PIPELINE + "/senders";

        /// <summary>
        /// Template for presence instance.
        /// </summary>
        public const string PRESENCE_INSTANCE = "/presence/{instance}";

        /// <summary>
        /// Template for the public profile storage ids.
        /// </summary>
        public const string PROFILES = "/profile";

        /// <summary>
        /// Template for the public profile storage.
        /// </summary>
        public const string PROFILE = "/profile/{id}";

        /// <summary>
        /// Template for the public resource storage ids.
        /// </summary>
        public const string RESOURCES = "/resources";

        /// <summary>
        /// Template for the public resource storage 
        /// </summary>
        public const string RESOURCE = "/resources/{id}";

        /// <summary>
        /// Template for AI intentions.
        /// </summary>
        public const string INTENTIONS = "/intentions";

        /// <summary>
        /// Template for an AI intention.
        /// </summary>
        public const string INTENTION = "/intentions/{id}";

        /// <summary>
        /// Template for AI intention questions.
        /// </summary>
        public const string INTENTION_QUESTIONS = "/intentions/{intentionId}/questions";

        /// <summary>
        /// Template for AI an intention question.
        /// </summary>
        public const string INTENTION_QUESTION = "/intentions/{intentionId}/questions/{questionId}";

        /// <summary>
        /// Template for AI intention answers.
        /// </summary>
        public const string INTENTION_ANSWERS = "/intentions/{intentionId}/answers";

        /// <summary>
        /// Template for AI an intention answer.
        /// </summary>
        public const string INTENTION_ANSWER = "/intentions/{intentionId}/answers/{answerId}";

        /// <summary>
        /// Template for AI entity.
        /// </summary>
        public const string ENTITIES = "/entities";

        /// <summary>
        /// Template for an AI entity.
        /// </summary>
        public const string ENTITY = "/entities/{id}";

        /// <summary>
        /// Template for an AI analysis.
        /// </summary>
        public const string ANALYSIS = "/analysis";

        /// <summary>
        /// Template for an AI content analysis.
        /// </summary>
        public const string CONTENT_ANALYSIS = "/content/analysis";

        /// <summary>
        /// Template for an AI models.
        /// </summary>
        public const string MODELS = "/models";

        /// <summary>
        /// Template for an AI analysis feedback.
        /// </summary>
        public const string ANALYSIS_FEEDBACK = "/analysis/{analysisId}/feedback";
    }
}
