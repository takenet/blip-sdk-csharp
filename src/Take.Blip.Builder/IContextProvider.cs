using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder
{
    public interface IContextProvider
    {
        Task<IContext> GetContextAsync(string user, CancellationToken cancellationToken);
    }
}