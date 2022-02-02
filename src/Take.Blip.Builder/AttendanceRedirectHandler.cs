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
        public const string HELP_DESK_HAS_TICKET = "helpDeskHasTicket";
        public const string DIRECT_TRANSFER_KEY = "DIRECT_TRANSFER";

        public readonly IHelpDeskExtension _helpDeskExtension;
        public readonly IConfiguration _configuration;

        public AttendanceRedirectHandler(IHelpDeskExtension helpDeskExtension, IConfiguration configuration)
        {
            _helpDeskExtension = helpDeskExtension;
            _configuration = configuration;
        }

        public async Task RedirectToAttendanceAsync(string attendanceRedirect, Identity userIdentity, IContext context, Message message, CancellationToken cancellationToken)
        {
            if (!_configuration.IsAttendanceRedirectAllowed)
                return;

            await context.SetVariableAsync(HELP_DESK_HAS_TICKET, true.ToString(), cancellationToken);

            await CreateTicketWithRedirectAsync(attendanceRedirect, userIdentity, message, cancellationToken);

            await context.DeleteVariableAsync(ATTENDANCE_REDIRECT_KEY, cancellationToken);
        }


        private async Task CreateTicketWithRedirectAsync(string attendanceRedirect, Identity userIdentity, Message message, CancellationToken cancellationToken)
        {
            var content = new Ticket()
            {
                CustomerIdentity = userIdentity,
                Team = DIRECT_TRANSFER_KEY,
                AgentIdentity = attendanceRedirect
            };

            await _helpDeskExtension.CreateTicketAsync(message.From.ToIdentity(), content, cancellationToken);
        }

    }
}
