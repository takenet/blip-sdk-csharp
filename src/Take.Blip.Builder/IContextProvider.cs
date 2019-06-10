using Lime.Protocol;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    public interface IContextProvider
    {
        IContext CreateContext(Identity userIdentity, Identity applicationIdentity, LazyInput input, Flow flow);
    }
}