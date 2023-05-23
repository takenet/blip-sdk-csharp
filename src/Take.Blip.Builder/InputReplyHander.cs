using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Builder;
/// <summary>
/// Class responsible for handling input reply messages.
/// </summary>
public class InputReplyHandler : IInputMessageHandler
{
    /// <summary>
    /// Validates a reply message and extracts the replied content.
    /// </summary>
    /// <param name="message">The input message to be validated.</param>
    /// <returns>
    /// A tuple containing a boolean value indicating the validation result
    /// and the modified message with the replied content.
    /// </returns>
    public (bool, Message) ValidateMessage(Message message)
    {
        if (message.Content is Reply reply)
        {
            message.Content = reply.Replied.Value;
            return (true, message);
        }

        return (false, message);
    }
}
