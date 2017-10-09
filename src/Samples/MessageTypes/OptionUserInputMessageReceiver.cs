using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace MessageTypes
{
    public class OptionUserInputMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionUserInputMessageReceiver(ISender sender)
        {
            _sender = sender;
        }
        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            Document document;
            if (message.Content.ToString().Equals("ui1"))
                document = getUserName();
            else
                document = getUserInputLocation();

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }

        public Input getUserName()
        {
            var document = new Input
            {
                Label = new DocumentContainer
                {
                    Value = new PlainText 
                    {
                       Text = "What is your name?"
                    } 
                },
                Validation = new InputValidation
                {
                    Rule = InputValidationRule.Text
                } 
            };
            return document;
        }
        public Input getUserInputLocation()
        {
            var document = new Input
            {
                Label = new DocumentContainer
                {
                    Value = "Send your location please!"
                },
                Validation = new InputValidation
                {
                    Rule = InputValidationRule.Type,
                    Type = "application/vnd.lime.location+json"
                } 
            };
            return document;
        }
    }
}
