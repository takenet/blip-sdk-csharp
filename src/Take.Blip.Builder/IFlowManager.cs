using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a flow management service.
    /// </summary>
    public interface IFlowManager
    {
        Task ProcessInputAsync(Document input, Identity user, Flow flow, CancellationToken cancellationToken);
    }
}