using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder.Hosting;
using Take.Blip.Client.Extensions.HelpDesk;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Builder
{
    public class AttendanceRedirectHandler : IAttendanceRedirectHandler
    {

        public static string ATTENDANCE_REDIRECT_KEY = "attendanceRedirect";
        public static string HELP_DESK_HAS_TICKET = "helpDeskHasTicket";
        public static string DIRECT_TRANSFER_KEY = "DIRECT_TRANSFER";

        public readonly IHelpDeskExtension _helpDeskExtension;
        public readonly IConfiguration _configuration;

        public AttendanceRedirectHandler(IHelpDeskExtension helpDeskExtension, IConfiguration configuration)
        {
            _helpDeskExtension = helpDeskExtension;
            _configuration = configuration;
        }

        public async Task RedirectToAttendanceAsync(Identity fromIdentity, Identity userIdentity,
            IContext context, CancellationToken cancellationToken)
        {
            if (!_configuration.IsAttendanceRedirectAllowed)
                return;

            var attendanceRedirect = await context.GetContextVariableAsync(ATTENDANCE_REDIRECT_KEY, cancellationToken);

            if (attendanceRedirect.IsNullOrWhiteSpace())
                return;

            await context.SetVariableAsync(HELP_DESK_HAS_TICKET, true.ToString(), cancellationToken);

            await CreateTicketWithRedirectAsync(attendanceRedirect, fromIdentity, userIdentity, cancellationToken);

            await context.DeleteVariableAsync(ATTENDANCE_REDIRECT_KEY, cancellationToken);
        }


        private async Task CreateTicketWithRedirectAsync(string attendanceRedirect, Identity fromIdentity, 
            Identity userIdentity, CancellationToken cancellationToken)
        {
            var content = new Ticket()
            {
                CustomerIdentity = userIdentity,
                Team = DIRECT_TRANSFER_KEY,
                AgentIdentity = attendanceRedirect
            };

            await _helpDeskExtension.CreateTicketAsync(fromIdentity, content, cancellationToken);
        }

    }
}
