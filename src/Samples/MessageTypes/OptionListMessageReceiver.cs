using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace MessageTypes
{
    public class OptionListMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionListMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var document = new DocumentList
            {
                Header = {
                    Value = new WebLink {
                        Title = "Classic T-Shirt Collection",
                        Text = "See all our colors",
                        PreviewUri = new Uri("https://peterssendreceiveapp.ngrok.io/img/collection.png"),
                        Uri = new Uri("https://peterssendreceiveapp.ngrok.io/shop_collection?messengerExtensions=true"),
                        Target = WebLinkTarget.SelfTall
                    }
                },
                Items = new DocumentContainer[]{
                    new DocumentContainer 
                    {
                        Value = new WebLink
                        {
                            Title = "Classic White T-Shirt",
                            Text = "100% Cotton, 200% Comfortable",
                            PreviewUri = new Uri("https://peterssendreceiveapp.ngrok.io/img/white-t-shirt.png"),
                            Uri = new Uri("https://peterssendreceiveapp.ngrok.io/view?item=100&messengerExtensions=true"),
                            Target = WebLinkTarget.SelfTall
                        }
                    },
                    new DocumentContainer
                    {
                        Value = new WebLink
                        {
                            Title = "Classic Blue T-Shirt",
                            Text = "100% Cotton, 200% Comfortable",
                            PreviewUri = new Uri("https://peterssendreceiveapp.ngrok.io/img/blue-t-shirt.png"),
                            Uri = new Uri("https://peterssendreceiveapp.ngrok.io/view?item=101&messengerExtensions=true"),
                            Target = WebLinkTarget.SelfTall

                        }
                    },
                    new DocumentContainer
                    {
                        Value = new WebLink
                        {
                            Title = "Classic Black T-Shirt",
                            Text = "100% Cotton, 200% Comfortable",
                            PreviewUri = new Uri("https://peterssendreceiveapp.ngrok.io/img/black-t-shirt.png"),
                            Uri = new Uri("https://peterssendreceiveapp.ngrok.io/view?item=102&messengerExtensions=true"),
                            Target = WebLinkTarget.SelfTall
                        }
                    }
                }
            };
            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }

    }
}
