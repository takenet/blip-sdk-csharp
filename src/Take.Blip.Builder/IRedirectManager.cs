using Lime.Messaging.Contents;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder
{
    public interface IRedirectManager
    {
        Task RedirectUserAsync(IContext context, Redirect redirect, CancellationToken cancellationToken);
    }
}
