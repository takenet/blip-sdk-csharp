using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;

namespace Take.Blip.Builder
{
    public interface IRedirectManager
    {
        Task RedirectUserAsync(
            IContext context,
            Redirect redirect,
            CancellationToken cancellationToken
        );
    }
}
