using Lime.Protocol;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    public interface IContextProvider
    {
        IContext CreateContext(Identity user, LazyInput input, Flow flow);
    }
}