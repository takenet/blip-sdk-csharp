using Lime.Protocol;
using Take.Blip.Client.Content;

namespace Take.Blip.Builder.Content
{
    public static class MessageExtension
    {
        public static Message CreateInputExirationTimeMessage(this Message message) 
        {
            var idMessage = GetInputExirationTimeIdMessage(message);

            return new Message(idMessage)
            {
                To = message.To,
                Content = new InputExpirationTimeMarker()
                { 
                    Identity = message.From
                }
            };
        }

        public static string GetInputExirationTimeIdMessage(this Message message)
        { 
            return $"{message.From.ToIdentity().ToString()}-inputexpirationtime";
        }


    }
}
