using Lime.Protocol;

namespace Take.Blip.Builder
{
    public interface IContextProvider
    {
        IContext GetContext(Identity user, string flowId);
    }
}