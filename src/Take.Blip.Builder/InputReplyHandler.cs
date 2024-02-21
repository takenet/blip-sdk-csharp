using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Take.Blip.Builder;
using Take.Blip.Builder.Models;

/// <summary>
/// Class responsible for handling input reply messages.
/// </summary>
public class InputReplyHandler : IInputMessageHandler
{
    public const string REPLY_CONTENT = "#replyContent";
    public const string IN_REPLY_TO_ID = "#inReplyToId";

    private readonly Document _emptyContent = new PlainText() { Text = string.Empty };
    private readonly IDocumentSerializer _documentSerializer;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="documentSerializer">The document serializer.</param>
    public InputReplyHandler(IDocumentSerializer documentSerializer)
    {
        _documentSerializer = documentSerializer;
    }

    /// <summary>
    /// Validates a reply message and extracts the replied content.
    /// </summary>
    /// <param name="message">The input message to be validated.</param>
    /// <returns>
    /// A tuple containing a boolean value indicating the validation result
    /// and the modified message with the replied content.
    /// </returns>
    public (bool MessageHasChanged, Message NewMessage) HandleMessage(Message message)
    {
        if (message.Content is Reply reply)
        {
            message.Content = reply.Replied?.Value ?? _emptyContent;

            TryAddMetadataIntoMessage(message, reply);

            return (true, message);
        }

        return (false, message);
    }

    /// <summary>
    /// Checks if state is valid. It isn't implemented yet.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="message">The message.</param>
    /// <param name="flow">The flow.</param>
    /// <returns>True if state is valid; Otherwise false.</returns>
    public bool IsValidateState(State state, Message message, Flow flow) => true;

    /// <summary>
    /// Executes before the flow process. It isn't implemented yet.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="message">The message.</param>
    /// <param name="from">From of message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public Task OnFlowPreProcessingAsync(State state, Message message, Node from, IContext context, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Executes after the flow process. It isn't implemented yet.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="flow">The flow.</param>
    /// <param name="message">The message.</param>
    /// <param name="from">From of message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public Task OnFlowProcessedAsync(State state, Flow flow, Message message, Node from, CancellationToken cancellationToken) => Task.CompletedTask;

    private void TryAddMetadataIntoMessage(Message message, Reply reply)
    {
        message.Metadata ??= new Dictionary<string, string>();
        message?.Metadata?.TryAdd(REPLY_CONTENT, _documentSerializer.Serialize(reply));

        if (ShouldAddMetadataInReplyToId(message, reply))
        {
            message?.Metadata?.TryAdd(IN_REPLY_TO_ID, reply.InReplyTo.Id);
        }
    }

    private bool ShouldAddMetadataInReplyToId(Message message, Reply reply) =>
           reply.InReplyTo != null
        && !message.Metadata.ContainsKey(IN_REPLY_TO_ID)
        && !string.IsNullOrEmpty(reply.InReplyTo.Id);
}
