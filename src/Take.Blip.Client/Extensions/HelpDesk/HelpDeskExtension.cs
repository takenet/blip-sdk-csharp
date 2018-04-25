using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using System.Web;
using System;

namespace Take.Blip.Client.Extensions.HelpDesk
{
    public class HelpDeskExtension : IHelpDeskExtension
    {
        private readonly ISender _sender;

        public HelpDeskExtension(ISender sender)
        {
            _sender = sender;
        }
        public async Task ForwardMessageToAttendantAsync(Message message, CancellationToken cancellationToken)
        {
            var customerName = Uri.EscapeDataString(message.From.ToIdentity().ToString());
            var deskNode = new Identity(customerName, "desk.msging.net").ToNode();

            await _sender.SendMessageAsync(message.Content, deskNode, cancellationToken);
        }

        public bool FromAttendant(Message message)
        {
            return message.From.Domain.Equals("desk.msging.net");
        }
    }
}
