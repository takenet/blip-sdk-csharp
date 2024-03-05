using Lime.Protocol.Serialization;
using Serilog;
using Take.Blip.Client;

namespace Take.Blip.Builder.Variables
{
    public class SecretVariableProvider : ResourceVariableProviderBase, IVariableProvider
    {
        public SecretVariableProvider(ISender sender, IDocumentSerializer documentSerializer, ILogger logger) : base(sender, documentSerializer, "secrets", logger) { }

        public override VariableSource Source => VariableSource.Secret;

    }
}
