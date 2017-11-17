using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Actions
{
    public interface IAction
    {
        string Type { get; }

        Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken);
    }
}
