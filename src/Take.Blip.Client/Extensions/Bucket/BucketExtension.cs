using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;

namespace Take.Blip.Client.Extensions.Bucket
{
    public class BucketExtension : IBucketExtension
    {
        private readonly ISender _sender;
        private readonly string _resourceName;

        public BucketExtension(ISender sender)
            : this(sender, "buckets")
        {
        }

        protected BucketExtension(ISender sender, string resourceName)
        {
            _sender = sender;
            _resourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
        }

        public async Task<T> GetAsync<T>(string id, CancellationToken cancellationToken = default(CancellationToken)) where T : Document
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var getRequestCommand = new Command()
            {
                Method = CommandMethod.Get,
                Uri = new LimeUri($"/{_resourceName}/{Uri.EscapeDataString(id)}")
            };

            var getResponseCommand = await _sender.ProcessCommandAsync(
                getRequestCommand,
                cancellationToken);

            if (getResponseCommand.Status != CommandStatus.Success)
            {
                if (getResponseCommand.Reason?.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
                {
                    return null;
                }

                throw new LimeException(
                    getResponseCommand.Reason ?? 
                    new Reason() { Code = ReasonCodes.COMMAND_PROCESSING_ERROR, Description = "An error occurred" });
            }
            return (T)getResponseCommand.Resource;
        }

        public async Task<DocumentCollection> GetIdsAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default(CancellationToken))
        {
            var getRequestCommand = new Command()
            {
                Method = CommandMethod.Get,
                Uri = new LimeUri($"/{_resourceName}?$skip={skip}&$take={take}")
            };

            var getResponseCommand = await _sender.ProcessCommandAsync(
                getRequestCommand,
                cancellationToken);

            if (getResponseCommand.Status != CommandStatus.Success)
            {
                if (getResponseCommand.Reason?.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
                {
                    return null;
                }

                throw new LimeException(
                    getResponseCommand.Reason ??
                    new Reason() { Code = ReasonCodes.COMMAND_PROCESSING_ERROR, Description = "An error occurred" });
            }
            return (DocumentCollection)getResponseCommand.Resource;
        }

        public async Task SetAsync<T>(string id, T document, TimeSpan expiration = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken)) where T : Document
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (document == null) throw new ArgumentNullException(nameof(document));

            var uri = $"/{_resourceName}/{Uri.EscapeDataString(id)}";
            if (expiration != default(TimeSpan))
            {
                uri = $"{uri}?expiration={expiration.TotalMilliseconds}";
            }

            var setRequestCommand = new Command()
            {
                Method = CommandMethod.Set,
                Uri = new LimeUri(uri),
                Resource = document
            };

            var setResponseCommand = await _sender.ProcessCommandAsync(
                setRequestCommand,
                cancellationToken);
            if (setResponseCommand.Status != CommandStatus.Success)
            {
                throw new LimeException(
                    setResponseCommand.Reason ??
                    new Reason() { Code = ReasonCodes.COMMAND_PROCESSING_ERROR, Description = "An error occurred" });
            }
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var deleteRequestCommand = new Command()
            {
                Method = CommandMethod.Delete,
                Uri = new LimeUri($"/{_resourceName}/{Uri.EscapeDataString(id)}")
            };

            var deleteResponseCommand = await _sender.ProcessCommandAsync(
                deleteRequestCommand,
                cancellationToken);

            if (deleteResponseCommand.Status != CommandStatus.Success)
            {
                throw new LimeException(
                    deleteResponseCommand.Reason ??
                    new Reason() { Code = ReasonCodes.COMMAND_PROCESSING_ERROR, Description = "An error occurred" });
            }
        }
    }
}