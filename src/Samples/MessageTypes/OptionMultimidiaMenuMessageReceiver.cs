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
            DocumentSelect document;

            if (message.Content.ToString().Equals("mm1"))
                document = GetDocumentSelectWithImage();
            else
                document = GetDocumentSelectWithLocation();

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }

        //metodo atualmente não funcionando corretamente
        public DocumentSelect GetDocumentSelectWithLocation()
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
                                        Text = "Teste"
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

        public DocumentSelect GetDocumentSelectWithImage()
        {

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
                        Value = new JsonDocument()
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
                        Uri = new Uri("http://petersapparel.parseapp.com/img/item100-thumb.png"),
                        AspectRatio = "1.1"
                    }
                },
                Options = document2
            };

            return document;
        }
    }
}
