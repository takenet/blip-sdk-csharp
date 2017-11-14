using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder
{
    public interface IFlowManager
    {
        Task ProcessAsync(string user, CancellationToken cancellationToken);
    }
}