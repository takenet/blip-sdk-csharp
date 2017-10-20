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
        Document[] documents;
        JsonDocument jsonDocuments;
        JsonDocument jsonDocuments2;
        JsonDocument jsonDocuments3;

        public OptionDocumentCollectionMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            Document document;

             if (message.Content.ToString().Equals("dc1"))
                document = GetDocumentCollectionText();
            else if(message.Content.ToString().Equals("dc2"))
                document = GetDocumentCollectionWithDiferentTypes();
                else  
                document = getDocumentCollectionMenuMultimidia();

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }

        public DocumentCollection GetDocumentCollectionText()
        {
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

             var document = new DocumentCollection
            {
                ItemType = "text/plain",
                Items = documents
            };
            return document;
        }

        public DocumentCollection GetDocumentCollectionWithDiferentTypes()
        {
            DocumentContainer[] documents = new DocumentContainer[]
            {
                new DocumentContainer{
                    Value = new MediaLink
                    {
                        Uri = new Uri("http://www.petshoplovers.com/wp-content/uploads/2014/03/CUIDADOS-B%C3%81SICOS-PARA-CRIAR-COELHOS.jpg"),
                        Text = "Welcome to our store!",
                        Type = "image/jpeg"
                    }
                },
                new DocumentContainer{
                    Value = new Select
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
                }
            };
            var document = new DocumentCollection
            {
                ItemType = "application/vnd.lime.container+json",
                Items = documents
            };

            return document;
        }

        public DocumentCollection getDocumentCollectionMenuMultimidia()
        {
            jsonDocuments = new JsonDocument();
            jsonDocuments2 = new JsonDocument();    
            jsonDocuments3 = new JsonDocument();

            jsonDocuments.Add("Key1", "value1");
            jsonDocuments.Add("Key2", "2");

            jsonDocuments2.Add("Key3", "value3");
            jsonDocuments2.Add("Key4", "4");

            jsonDocuments3.Add("Key5", "value5");
            jsonDocuments3.Add("Key6", "6");

            DocumentSelect[] documents = new DocumentSelect[]
             {
                new DocumentSelect
                {
                    Header = new DocumentContainer
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
                            Label = new DocumentContainer
                            {
                                Value = new WebLink
                                {
                                    Title = "Link",
                                    Uri = new Uri("http://www.adoteumgatinho.org.br/")
                                }
                            }
                        },
                        new DocumentSelectOption
                        {
                            Label = new DocumentContainer
                            {
                                Value = new PlainText
                                {
                                    Text = "Text 1"
                                }
                            },
                            Value = new DocumentContainer
                            {
                                Value = jsonDocuments
                            }
                        }
                    }
                },
                new DocumentSelect
                {
                    Header = new DocumentContainer
                    {
                        Value = new MediaLink
                        {
                            Title = "Title 2",
                            Text = "This is another item",
                            Type = "image/jpeg",
                            Uri = new Uri("http://www.freedigitalphotos.net/images/img/homepage/87357.jpg")
                        }
                    },
                    Options = new DocumentSelectOption[]
                    {
                        new DocumentSelectOption
                        {
                            Label = new DocumentContainer
                            {
                                Value = new WebLink
                                {
                                    Title = "Second link",
                                    Text = "Weblink",
                                    Uri = new Uri("https://pt.dreamstime.com/foto-de-stock-brinquedo-pl%C3%A1stico-amarelo-do-pato-image44982058")
                                }
                            }
                        },
                        new DocumentSelectOption
                        {
                            Label = new DocumentContainer
                            {
                                Value = new PlainText {
                                    Text = "Second text"
                                }
                            },
                            Value = new DocumentContainer
                            {
                                Value = jsonDocuments2
                            }
                        },
                        new DocumentSelectOption
                        {
                            Label = new DocumentContainer
                            {
                                Value = new PlainText {
                                    Text = "More one text"
                                }
                            },
                            Value = new DocumentContainer
                            {
                                Value = jsonDocuments3
                            }
                        }
                    }
                }

            };

           var document = new DocumentCollection
            {
                ItemType = "application/vnd.lime.document-select+json",
                Items = documents,
            };

            return document;
        }
    }
}