using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;
using System.Diagnostics;

namespace Take.Blip.Client.Receivers
{
    /// <summary>
    /// Implements a <see cref="Message"/> receiver that sends a message to the sender in case of processing exception..
    /// </summary>
    /// <seealso cref="Take.Blip.Client.IMessageReceiver" />
    public class HandleExceptionMessageReceiver : IMessageReceiver
    {
        private readonly IMessageReceiver _receiver;
        private readonly SendResponseMessageReceiver _sendResponseMessageReceiver;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandleExceptionMessageReceiver"/> class.
        /// </summary>
        /// <param name="receiver">The receiver to intercept the errors.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="exceptionDocument">The exception document.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public HandleExceptionMessageReceiver(IMessageReceiver receiver, ISender sender, Document exceptionDocument)
        {
            if (receiver == null) throw new ArgumentNullException(nameof(receiver));

            _receiver = receiver;
            _sendResponseMessageReceiver = new SendResponseMessageReceiver(sender, exceptionDocument);
        }

        public async Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                await _receiver.ReceiveAsync(envelope, cancellationToken);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                await _sendResponseMessageReceiver.ReceiveAsync(envelope, cancellationToken);
            }
        }
    }
}
