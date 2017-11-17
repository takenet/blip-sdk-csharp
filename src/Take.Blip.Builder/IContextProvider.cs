using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder
{
    public interface IContextProvider
    {
        IContext GetContext(Identity user, IDictionary<string, string> flowVariables);
    }
}