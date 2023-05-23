using Lime.Protocol;

namespace Take.Blip.Builder
{
    public interface IInputReplyHandler
    {
        (bool, Message) ValidateReplyMessage(Message message);
    }
}
