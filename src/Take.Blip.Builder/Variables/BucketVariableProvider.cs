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

        public BucketVariableProvider(ISender sender, IDocumentSerializer documentSerializer)
        {
            _sender = sender;
            _documentSerializer = documentSerializer;
        }

        public VariableSource Source => VariableSource.Bucket;

        public async Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken)
        {
            try
            {
                // We are sending the command directly here because the BucketExtension requires us to known the type.
                var bucketCommandResult = await _sender.ProcessCommandAsync(
                    new Command()
                    {
                        Uri = new LimeUri($"/buckets/{Uri.EscapeDataString(name)}"),
                        Method = CommandMethod.Get,
                    },
                    cancellationToken);

                if (bucketCommandResult.Status != CommandStatus.Success) return null;
                if (!bucketCommandResult.Resource.GetMediaType().IsJson) return bucketCommandResult.Resource.ToString();
                return _documentSerializer.Serialize(bucketCommandResult.Resource);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                return null;
            }
        }
    }
}
