using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;
using Takenet.Iris.Messaging.Contents;

namespace MessageTypes
{
    public class OptionMultimidiaMenuMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionMultimidiaMenuMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        DocumentSelectOption[] options = new DocumentSelectOption[]{
            new DocumentSelectOption 
            {
                Label = 
                {
                    Value = new WebLink 
                    {
                        Text = "Go to your site",
                        Uri = new Uri("https://petersapparel.parseapp.com/view_item?item_id=100")
                    }
                }
            },
            new DocumentSelectOption 
            {
                Label = 
                {
                    Value = new PlainText 
                    {
                        Text = "Show stock"
                    }
                },
                Value = 
                {
                    Value = new JsonDocument()
                }
            }
            
            
        };
        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var document = new DocumentSelect
            {
                Header = {
                    Value = new MediaLink{
                        Title = "Welcome to mad hatter",
                        Text = "Here we have the best hats for your head.",
                        Type = "image/jpeg",
                        Uri = new Uri("http://petersapparel.parseapp.com/img/item100-thumb.png"),
                        AspectRatio = "1.1"
                    }
                },
                Options = options
            };

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }
    }

     public class MenuMultimidiaGetLocation : IMessageReceiver
    {
        private readonly ISender _sender;

        public MenuMultimidiaGetLocation(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var document = new DocumentSelect
            {
                Header = {
                    Value = new PlainText{
                        Text = "Please, share your location"
                    }
                },
                Options = new DocumentSelectOption[]{
                    new DocumentSelectOption {
                        Label = {
                            Value = new Input {
                                Validation = {
                                    Rule = InputValidationRule.Type,
                                    Type = "application/vnd.lime.location+json"
                                }
                            }
                        }
                    }
                }
                
            };

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }
    }

}
