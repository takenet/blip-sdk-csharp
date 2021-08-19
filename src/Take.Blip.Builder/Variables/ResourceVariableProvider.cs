using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Serilog;
using Take.Blip.Client;

namespace Take.Blip.Builder.Variables
{
    public class ResourceVariableProvider : ResourceVariableProviderBase, IVariableProvider
    {
        public ResourceVariableProvider(ISender sender, IDocumentSerializer documentSerializer, ILogger logger) : base(sender, documentSerializer, "resources", logger) { }

        public override VariableSource Source => VariableSource.Resource;

    }
}
