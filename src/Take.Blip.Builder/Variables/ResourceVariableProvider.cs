using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Take.Blip.Client;

namespace Take.Blip.Builder.Variables
{
    public class ResourceVariableProvider : ResourceVariableProviderBase, IVariableProvider
    {
        public ResourceVariableProvider(ISender sender, IDocumentSerializer documentSerializer) : base(sender, documentSerializer, "resources") { }

        public override VariableSource Source => VariableSource.Resource;

    }
}
