using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Take.Blip.Client;

namespace Take.Blip.Builder.Variables
{
    public class BucketVariableProvider : ResourceVariableProviderBase, IVariableProvider
    {
        public BucketVariableProvider(ISender sender, IDocumentSerializer documentSerializer) : base(sender, documentSerializer, "buckets") { }

        public override VariableSource Source => VariableSource.Bucket;
    }
}
