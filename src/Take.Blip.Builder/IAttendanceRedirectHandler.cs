using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder
{
    public interface IAttendanceRedirectHandler
    {
        Task RedirectToAttendanceAsync(Identity fromIdentity, Identity userIdentity,
            IContext context, CancellationToken cancellationToken);
    }
}
