using Lime.Messaging.Contents;
using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Client.Receivers
{
    /// <summary>
    /// Implements a <see cref="Message"/> receiver that send a <see cref="ChatState"/> message the message sender with the <see cref="ChatStateEvent.Composing"/> state.
    /// </summary>
    /// <seealso cref="Take.Blip.Client.IMessageReceiver" />
    public class ComposingMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComposingMessageReceiver"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public ComposingMessageReceiver(ISender sender)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public virtual Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = new CancellationToken())
        {
            return _sender.SendMessageAsync(
                new Message
                {
                    To = envelope.From,
                    Content = new ChatState
                    {
                        State = ChatStateEvent.Composing
                    }
                },
                cancellationToken);
        }
    }
}
