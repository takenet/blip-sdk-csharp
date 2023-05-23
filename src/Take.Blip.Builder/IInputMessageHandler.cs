using Lime.Protocol;

namespace Take.Blip.Builder
{
    public interface IInputMessageHandler
    {
        (bool, Message) ValidateMessage(Message message);
    }
}
