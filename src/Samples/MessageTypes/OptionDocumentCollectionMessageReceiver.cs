using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace MessageTypes
{
    public class OptionDocumentCollectionMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionDocumentCollectionMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        PlainText[] documents = new PlainText[] 
        {
            new PlainText 
            {
                Text = "Text 1"
            },
            new PlainText
            {
                Text = "Text 2"
            },
            new PlainText 
            {
                Text = "Text 3"
            }
        };

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var plainText = new DocumentCollection
            {
                Items = documents
            };
            await _sender.SendMessageAsync(plainText, message.From, cancellationToken);
        }
    }

    public class CollectionWithDiferentTypes : IMessageReceiver
    {
        private readonly ISender _sender;

        public CollectionWithDiferentTypes(ISender sender)
        {
            _sender = sender;
        }

        Document[] documents = new Document[] 
        {
            new MediaLink
            {
                Uri = new Uri("http://petersapparel.parseapp.com/img/item100-thumb.png"),
                Text = "Welcome to our store!",
                Type = "image/jpeg"
            },
            new Select
            {
                Text = "Choice what you need",
                Options = new SelectOption[] 
                {
                    new SelectOption 
                    {
                        Order = 1,
                        Text = "See our stock"
                    },
                    new SelectOption
                    {
                        Order = 2,
                        Text = "Follow an order"
                    }
                }
                
            }
        };

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var plainText = new DocumentCollection
            {
                Items = documents
            };
            await _sender.SendMessageAsync(plainText, message.From, cancellationToken);
        }
    }

    public class CollectionMultimidiaMenu : IMessageReceiver
    {
        private readonly ISender _sender;

        Document[] documents;
        JsonDocument JsonDocuments; 

        public CollectionMultimidiaMenu(ISender sender)
        {
            _sender = sender;   
            initDocument();
        }

        private void initDocument(){
            JsonDocument JsonDocuments = new JsonDocument();
            JsonDocuments.Add("Key1", "value1");
            JsonDocuments.Add("Key2", 2);


            DocumentSelect[] documents = new DocumentSelect[] 
            {
                new DocumentSelect
                {
                    Header = 
                    {
                        Value = new MediaLink
                        {
                            Title = "Title",
                            Text = "This is a first item",
                            Type = "image/jpeg",
                            Uri = new Uri("http://www.isharearena.com/wp-content/uploads/2012/12/wallpaper-281049.jpg"),
                        }
                    },
                    Options = new DocumentSelectOption[]
                    {
                        new DocumentSelectOption
                        {
                            Label = 
                            {
                                Value = new WebLink
                                {
                                    Title = "Link",
                                    Uri = new Uri("https://server.com/first/link1")
                                }
                            }
                        },
                        new DocumentSelectOption
                        {
                            Label =
                            {
                                Value = new PlainText
                                {
                                    Text = "Text 1"
                                }
                            },
                            Value = 
                            {
                                Value = JsonDocuments
                            }
                        }   
                    }
                },
                
            };
        }

        
    
        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {

            var documentCollection = new DocumentCollection
            {
                Items = documents,
            };
            await _sender.SendMessageAsync(documentCollection, message.From, cancellationToken);
        }
    }
}
