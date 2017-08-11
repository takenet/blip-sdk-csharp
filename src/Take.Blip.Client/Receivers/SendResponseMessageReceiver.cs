using Lime.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Client.Receivers
{
    /// <summary>
    /// Implements a <see cref="Message"/> receiver sends a static document message to the sender when a message is received.
    /// </summary>
    /// <seealso cref="IMessageReceiver" />
    public class SendResponseMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        private readonly Document _response;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendResponseMessageReceiver"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="response">The response.</param>
        public SendResponseMessageReceiver(ISender sender, Document response)
        {
            _sender = sender ?? throw new System.ArgumentNullException(nameof(sender));
            _response = response ?? throw new System.ArgumentNullException(nameof(response));
        }

        public virtual Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = new CancellationToken())
        {
            return _sender.SendMessageAsync(_response, envelope.From, cancellationToken);
        }
    }
}
