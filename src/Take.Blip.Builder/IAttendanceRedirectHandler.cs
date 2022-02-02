using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder
{
    public interface IAttendanceRedirectHandler
    {
        Task RedirectToAttendanceAsync(string attendanceRedirect, Identity userIdentity,
            IContext context, Message message,
            CancellationToken cancellationToken);
    }
}
