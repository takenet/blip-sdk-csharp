using Lime.Protocol;
using Take.Blip.Client.Content;

namespace Take.Blip.Builder.Content
{
    public static class MessageExtension
    {
        public static Message CreateInputExirationTimeMessage(this Message message, string stateId) 
        {
            var idMessage = GetInputExirationTimeIdMessage(message);

            return new Message(idMessage)
            {
                To = message.To,
                Content = new InputExpiration()
                { 
                    Identity = message.From,
                    StateId = stateId
                }
            };
        }

        public static string GetInputExirationTimeIdMessage(this Message message)
        { 
            return $"{message.From.ToIdentity().ToString()}-inputexpirationtime";
        }


    }
}
