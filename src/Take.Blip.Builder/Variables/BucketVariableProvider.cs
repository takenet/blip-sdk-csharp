using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization.Newtonsoft;
using Newtonsoft.Json;
using Take.Blip.Client;

namespace Take.Blip.Builder.Variables
{
    public class BucketVariableProvider : IVariableProvider
    {
        private readonly ISender _sender;

        public BucketVariableProvider(ISender sender)
        {
            _sender = sender;
        }

        public VariableSource Source => VariableSource.Bucket;

        public async Task<string> GetVariableAsync(string name, Identity user, CancellationToken cancellationToken)
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
                return JsonConvert.SerializeObject(bucketCommandResult.Resource, JsonNetSerializer.Settings);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                return null;
            }
        }
    }
}
