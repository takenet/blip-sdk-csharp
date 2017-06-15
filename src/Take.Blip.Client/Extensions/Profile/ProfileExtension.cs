using Lime.Messaging.Contents;
using Lime.Protocol;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Sender;
using System;

namespace Take.Blip.Client.Extensions.Profile
{
    public class ProfileExtension : ExtensionBase, IProfileExtension
    {
        private const string PROFILE_URI = "/profile";
        private const string GET_STARTED_ID = "get-started";
        private const string PERSISTENT_MENU_ID = "persistent-menu";
        private const string GREETING_ID = "greeting";

        public ProfileExtension(ISender sender) : base(sender)
        {
        }

        public Task<Document> GetGetStartedAsync(CancellationToken cancellationToken)
            => ProcessCommandAsync<Document>(
                CreateGetCommandRequest($"{PROFILE_URI}/{GET_STARTED_ID}"),
                cancellationToken);

        public Task<Document> SetGetStartedAsync(Document getStarted, CancellationToken cancellationToken)
            => ProcessCommandAsync<Document>(
                CreateSetCommandRequest(getStarted, $"{PROFILE_URI}/{GET_STARTED_ID}"),
                cancellationToken);

        public Task DeleteGetStartedAsync(CancellationToken cancellationToken)
            => ProcessCommandAsync<Document>(
                CreateDeleteCommandRequest($"{PROFILE_URI}/{GET_STARTED_ID}"),
                cancellationToken);

        public Task<PlainText> GetGreetingAsync(CancellationToken cancellationToken)
            => ProcessCommandAsync<PlainText>(
                CreateGetCommandRequest($"{PROFILE_URI}/{GREETING_ID}"),
                cancellationToken);

        public Task SetGreetingAsync(PlainText greeting, CancellationToken cancellationToken)
            => ProcessCommandAsync<Document>(
                CreateSetCommandRequest(greeting, $"{PROFILE_URI}/{GREETING_ID}"),
                cancellationToken);

        public Task DeleteGreetingAsync(CancellationToken cancellationToken)
            => ProcessCommandAsync<PlainText>(
                CreateDeleteCommandRequest($"{PROFILE_URI}/{GREETING_ID}"),
                cancellationToken);

        public Task<DocumentSelect> GetPersistentMenuAsync(CancellationToken cancellationToken)
            => ProcessCommandAsync<DocumentSelect>(
                CreateGetCommandRequest($"{PROFILE_URI}/{PERSISTENT_MENU_ID}"),
                cancellationToken);

        public Task SetPersistentMenuAsync(DocumentSelect persistentMenu, CancellationToken cancellationToken)
            => ProcessCommandAsync<Document>(
                CreateSetCommandRequest(persistentMenu, $"{PROFILE_URI}/{PERSISTENT_MENU_ID}"),
                cancellationToken);

        public Task DeletePersistentMenuAsync(CancellationToken cancellationToken)
            => ProcessCommandAsync(
                CreateDeleteCommandRequest($"{PROFILE_URI}/{PERSISTENT_MENU_ID}"),
                cancellationToken);

        protected override void EnsureSuccess(Command responseCommand)
        {
            if (responseCommand.Method == CommandMethod.Get
                && responseCommand.Status == CommandStatus.Failure
                && responseCommand.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                return;
            }

            base.EnsureSuccess(responseCommand);
        }
    }
}
