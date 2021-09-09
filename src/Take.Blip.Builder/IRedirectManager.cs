using Lime.Messaging.Contents;
using Lime.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder
{
    public interface IRedirectManager
    {
        Task RedirectUserAsync(IContext context, Redirect redirect, Identity contact, CancellationToken cancellationToken);
    }
}
