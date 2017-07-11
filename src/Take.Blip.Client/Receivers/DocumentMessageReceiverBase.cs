using Lime.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Client.Receivers
{
    /// <summary>
    /// Implements a <see cref="Message"/> receiver that casts the <see cref="Message"/> content to the <see cref="TDocument"/> type.
    /// </summary>
    /// <typeparam name="TDocument"></typeparam>
    /// <seealso cref="IMessageReceiver" />
    public abstract class DocumentMessageReceiverBase<TDocument> : IMessageReceiver where TDocument : Document
    {
        public Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = new CancellationToken())
            => ReceiveDocumentAsync((TDocument)envelope.Content, envelope.From, cancellationToken);

        public abstract Task ReceiveDocumentAsync(TDocument document, Node from,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
