using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace MessageTypes
{
    public class OptionUserInputMessaReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionUserInputMessaReceiver(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var document = new Input
            {
                Label = {
                    Value = "What is your name?"
                },
                Validation = {
                    Rule = InputValidationRule.Text
                } 
            };

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }
    }

    
    public class UserInputLocationReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public UserInputLocationReceiver(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var document = new Input
            {
                Label = {
                    Value = "Send your location please!"
                },
                Validation = {
                    Rule = InputValidationRule.Type,
                    Type = "application/vnd.lime.location+json"//confirmar se esse type é necessario <<
                } 
            };

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }
    }
}
