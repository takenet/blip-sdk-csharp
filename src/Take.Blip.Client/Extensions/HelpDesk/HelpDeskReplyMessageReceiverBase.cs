using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Take.Blip.Client.Extensions.HelpDesk
{
    /// <summary>
    /// Automaticly forwards replies from an Blip App attendant
    /// to user that have sent a message
    /// </summary>
    public abstract class HelpDeskReplyMessageReceiverBase : IMessageReceiver
    {
        private readonly ISender _sender;
        private readonly IHelpDeskExtension _helpDeskExtension;

        public HelpDeskReplyMessageReceiverBase(
            ISender sender,
            IHelpDeskExtension helpDeskExtension)
        {
            _sender = sender;
            _helpDeskExtension = helpDeskExtension;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (_helpDeskExtension.FromAttendant(message))
            {
                var customerIdentityString = Uri.UnescapeDataString(message.From.Name);
                var customerIdentiy = Identity.Parse(customerIdentityString);
                await ReceiveAsync(message, customerIdentiy, cancellationToken);
            }
        }

        /// <summary>
        /// Receives helpDesk reply messages.
        /// </summary>
        protected abstract Task ReceiveAsync(Message message, Identity customerIdentiy, CancellationToken cancellationToken = default(CancellationToken));
    }
}
