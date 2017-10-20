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
        private DocumentSelectOption[] document2;

        public OptionMultimidiaMenuMessageReceiver(ISender sender)
        {
            _sender = sender;
        }
        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            Document document;

            if (message.Content.ToString().Equals("mm1"))
                document = getDocumentSelectWithImage();
            else
                document = getDocumentSelectWithLocation();

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }

        //metodo atualmente não funcionando corretamente
        public DocumentSelect getDocumentSelectWithLocation()
        {
            return new DocumentSelect
            {
                Header = new DocumentContainer
                {
                    Value = new PlainText
                    {
                        Text = "Please, share your location"
                    }
                },
                Options = new DocumentSelectOption[]{
                    new DocumentSelectOption {
                        Label = new DocumentContainer{
                            Value = new Input {
                                Label = new DocumentContainer {
                                    Value = new PlainText {
                                        Text = "Press Button"
                                    }
                                },
                                Validation = new InputValidation
                                {
                                    Type = Location.MediaType,
                                    Rule = InputValidationRule.Type
                                }
                            }
                        }
                    }
                }
            };
        }

        public DocumentSelect getDocumentSelectWithImage()
        {
            JsonDocument jsonDocuments = new JsonDocument();
            jsonDocuments.Add("action", "show-items");
            document2 = new DocumentSelectOption[]
            {
                new DocumentSelectOption
                {
                    Label = new DocumentContainer
                    {
                        Value = new WebLink
                        {
                            Text = "Go to your site",
                            Uri = new Uri("https://meusanimais.com.br/14-nomes-criativos-para-o-seu-gato/")
                        }
                    }
                },
                new DocumentSelectOption
                {
                    Label = new DocumentContainer
                    {
                        Value = new PlainText
                        {
                            Text = "Show stock here!"
                        }
                    },
                    Value = new DocumentContainer
                    {
                        Value = jsonDocuments
                    }
                }
            };

            var document = new DocumentSelect
            {
                Header = new DocumentContainer
                {
                    Value = new MediaLink
                    {
                        Title = "Welcome to mad hatter",
                        Text = "Here we have the best hats for your head.",
                        Type = "image/jpeg",
                        Uri = new Uri("http://i.overboard.com.br/imagens/produtos/0741720126/Ampliada/chapeu-new-era-bucket-print-vibe.jpg"),
                        AspectRatio = "1.1"
                    }
                },
                Options = document2
            };

            return document;
        }
    }
}
