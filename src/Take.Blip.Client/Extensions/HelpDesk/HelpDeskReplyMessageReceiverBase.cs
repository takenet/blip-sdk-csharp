using Lime.Messaging.Contents;
using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Take.Blip.Client.Extensions.HelpDesk
{
    /// <summary>
    /// Automaticly forwards replies from a BLiP Desk agent to the user
    /// </summary>
    public abstract class HelpDeskReplyMessageReceiverBase : IMessageReceiver
    {
        protected readonly ISender _sender;
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
            if (_helpDeskExtension.IsFromAgent(message) && 
                !message.Content.GetType().Equals(typeof(Redirect)))
            {
                var customerIdentityString = Uri.UnescapeDataString(message.From.Name);
                var customerIdentiy = Node.Parse(customerIdentityString);
                await ReceiveAsync(message, customerIdentiy, cancellationToken);
            }
        }

        /// <summary>
        /// Receives helpDesk reply messages.
        /// </summary>
        protected abstract Task ReceiveAsync(Message message, Node customerIdentiy, CancellationToken cancellationToken = default(CancellationToken));
    }
}
