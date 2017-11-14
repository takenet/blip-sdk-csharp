using Lime.Protocol;
using Lime.Protocol.Network;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Client.Extensions.Threads
{
    public class ThreadExtension : ExtensionBase, IThreadExtension
    {
        public const string THREADS_URI = "/threads";

        public ThreadExtension(ISender sender) 
            : base(sender)
        {
        }

        public Task<DocumentCollection> GetThreadsAsync(CancellationToken cancellationToken)
        {
            var requestCommand = CreateGetCommandRequest(THREADS_URI);
            return ProcessCommandAsync<DocumentCollection>(requestCommand, cancellationToken);
        }

        public Task<DocumentCollection> GetThreadAsync(Identity identity, CancellationToken cancellationToken = default(CancellationToken)) 
            => GetThreadInternalAsync(identity, null, cancellationToken);

        public Task<DocumentCollection> GetThreadAsync(Identity identity, int take, CancellationToken cancellationToken = default(CancellationToken)) 
            => GetThreadInternalAsync(identity, take, cancellationToken);

        private Task<DocumentCollection> GetThreadInternalAsync(Identity identity, int? take, CancellationToken cancellationToken = default(CancellationToken))
        {
            var requestUri = $"{THREADS_URI}/{identity}";
            if (take.HasValue)
            {
                requestUri = $"{requestUri}?$take={take}";
            }
            var requestCommand = CreateGetCommandRequest(requestUri);
            return ProcessCommandAsync<DocumentCollection>(requestCommand, cancellationToken);
        }

        public Task<Document> GetTranscriptionAsync(Identity identity, string accessKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            if ("".Equals(accessKey) || accessKey == null)
            {
                throw new LimeException(ReasonCodes.COMMAND_INVALID_ARGUMENT, "Invalid value for accessKey: " + accessKey);
            }

            var requestUri = $"/threads/{identity}/transcription?accessKey={accessKey}";
            var requestCommand = CreateGetCommandRequest(requestUri);
            return ProcessCommandAsync<Document>(requestCommand, cancellationToken);
        }
    }
}
