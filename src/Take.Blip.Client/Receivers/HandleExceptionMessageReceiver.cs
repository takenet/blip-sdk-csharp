using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using System.Diagnostics;
using Serilog;

namespace Take.Blip.Client.Receivers
{
    /// <summary>
    /// Implements a <see cref="Message"/> receiver that sends a message to the sender in case of processing exception..
    /// </summary>
    /// <seealso cref="Take.Blip.Client.IMessageReceiver" />
    public class HandleExceptionMessageReceiver : IMessageReceiver
    {
        private readonly IMessageReceiver _receiver;
        private readonly ILogger _logger;
        private readonly SendResponseMessageReceiver _sendResponseMessageReceiver;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandleExceptionMessageReceiver"/> class.
        /// </summary>
        /// <param name="receiver">The receiver to intercept the errors.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="exceptionDocument">The exception document.</param>
        /// <param name="logger"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public HandleExceptionMessageReceiver(IMessageReceiver receiver, ISender sender, Document exceptionDocument, ILogger logger)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                _logger.Error(ex, "The receive operation for message '{Id}' failed", envelope.Id);
                await _sendResponseMessageReceiver.ReceiveAsync(envelope, cancellationToken);
            }
        }
    }
}
