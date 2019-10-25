using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Take.Blip.Client;

namespace Take.Blip.Builder.Variables
{
    public class BucketVariableProvider : IVariableProvider
    {
        private readonly ISender _sender;
        private readonly IDocumentSerializer _documentSerializer;
        private readonly string _resourceName;

        public BucketVariableProvider(ISender sender, IDocumentSerializer documentSerializer)
            : this(sender, documentSerializer, "buckets")
        {
        }

        protected BucketVariableProvider(ISender sender, IDocumentSerializer documentSerializer, string resourceName)
        {
            _sender = sender;
            _documentSerializer = documentSerializer;
            _resourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
        }

        public VariableSource Source => VariableSource.Bucket;

        public async Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken)
        {
            try
            {
                var resourceCommandResult = await ExecuteGetResourceCommandAsync(name, cancellationToken);

                if (resourceCommandResult.Status != CommandStatus.Success)
                {
                    return null;
                }

                if (!resourceCommandResult.Resource.GetMediaType().IsJson)
                {
                    return resourceCommandResult.Resource.ToString();
                }

                return _documentSerializer.Serialize(resourceCommandResult.Resource);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                return null;
            }
        }

        private async Task<Command> ExecuteGetResourceCommandAsync(string name, CancellationToken cancellationToken)
        {
            // We are sending the command directly here because the ResourceExtension use the BucketExtension and it requires us to know the type.
            var getResourceCommand = new Command()
            {
                Uri = new LimeUri($"/{_resourceName}/{Uri.EscapeDataString(name)}"),
                Method = CommandMethod.Get,
            };

            var resourceCommandResult = await _sender.ProcessCommandAsync(
                getResourceCommand,
                cancellationToken);

            return resourceCommandResult;
        }
    }
}
