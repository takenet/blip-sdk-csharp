using Lime.Protocol.Serialization;
using Take.Blip.Client;

namespace Take.Blip.Builder.Variables
{
    public class SkillResourceVariableProvider : ResourceVariableProviderBase, IVariableProvider
    {
        public SkillResourceVariableProvider(ISender sender, IDocumentSerializer documentSerializer) : base(sender, documentSerializer, "skillFlow", "postmaster@bulder.msging.net") { }

        public override VariableSource Source => VariableSource.SkillFlow;

    }
}
