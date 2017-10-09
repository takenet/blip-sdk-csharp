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
                Header = new DocumentContainer
                {
                    Value = new WebLink
                    {
                        Title = "Classic T-Shirt Collection",
                        Text = "See all our colors",
                        PreviewUri = new Uri("http://streetwearvilla.com/image/cache/data/Products/Supreme/T-shirt/supreme-box-logo-t-shirt-collection-600x600.png"),
                        Uri = new Uri("http://streetwearvilla.com/supreme-box-logo-t-shirt-white"),
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
                            PreviewUri = new Uri("http://www.plainwhitetshirt.co.uk/image/cache/catalog/images/GD010vwhiteteegildan-750x750.jpg"),
                            Uri = new Uri("http://www.plainwhitetshirt.co.uk/gildan-soft-style-white-vneck-tshirt"),
                            Target = WebLinkTarget.SelfTall
                        }
                    },
                    new DocumentContainer
                    {
                        Value = new WebLink
                        {
                            Title = "Classic Blue T-Shirt",
                            Text = "100% Cotton, 200% Comfortable",
                            PreviewUri = new Uri("https://cdn.shopify.com/s/files/1/1475/5420/products/Classic_Blue_Front_12068_1024x1024.jpg?"),
                            Uri = new Uri("https://www.theringboxingclubshop.com/products/ring-classic-blue-t-shirt"),
                            Target = WebLinkTarget.SelfTall

                        }
                    },
                    new DocumentContainer
                    {
                        Value = new WebLink
                        {
                            Title = "Classic Black T-Shirt",
                            Text = "100% Cotton, 200% Comfortable",
                            PreviewUri = new Uri("http://www.lvnlifestyle.com/wp-content/uploads/2014/08/mens.black_.tshirt.jpg"),
                            Uri = new Uri("http://www.lvnlifestyle.com/product/black-mens-bamboo-organic-cotton-classic-t-shirt/"),
                            Target = WebLinkTarget.SelfTall
                        }
                    }
                }
            };
            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }

    }
}
