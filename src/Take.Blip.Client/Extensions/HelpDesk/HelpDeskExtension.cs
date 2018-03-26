using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Lime.Protocol;

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
            var customerName = HttpUtility.UrlEncode(message.From.ToIdentity().ToString());
            var deskNode = new Identity(customerName, "desk.msging.net").ToNode();

            await _sender.SendMessageAsync(message.Content, deskNode, cancellationToken);
        }

        public bool FromAttendant(Message message)
        {
            return message.From.Domain.Equals("desk.msging.net");
        }
    }
}
