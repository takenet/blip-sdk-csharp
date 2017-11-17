using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    public interface IContextProvider
    {
        IContext GetContext(Identity user, string flowId, IDictionary<string, string> flowVariables);
    }
}