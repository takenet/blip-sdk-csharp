using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace MessageTypes
{
    public class OptionSelectMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        JsonDocument jsonDocuments;
        public OptionSelectMessageReceiver(ISender sender)
        {
            _sender = sender;
        }



        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            jsonDocuments = new JsonDocument();
            jsonDocuments.Add("Key1", "value1");
            jsonDocuments.Add("Key2", "2");

            var document = new Select
            {
                //Scope = SelectScope.Immediate, (create a quickreply instead menu)
                Text = "Choice an option:",
                Options = new SelectOption[]
                {
                    new SelectOption
                    {
                        Order = 1,
                        Text = "First option!",
                        Value = new PlainText { Text = "1" }
                    },
                    new SelectOption
                    {
                        Order = 2,
                        Text = "Second option",
                        Value = new PlainText { Text = "2" }
                    },
                    new SelectOption
                    {
                        Order = 3,
                        Text = "Third option",
                        Value = jsonDocuments
                    }
                }
            };

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }

    }
}
