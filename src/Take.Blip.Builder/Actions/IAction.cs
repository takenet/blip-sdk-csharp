using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Actions
{
    public interface IAction
    {
        string Name { get; }

        Task<bool> ExecuteAsync(IContext context, JObject argument, CancellationToken cancellationToken);
    }
}
