using System;
using System.Threading.Tasks;
using Lime.Messaging;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Take.Blip.Builder.Actions.SendRawMessage;
using Take.Blip.Client;
using Take.Blip.Client.Extensions;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class SendRawMessageActionTests : ActionTestsBase
    {
        public SendRawMessageActionTests()
        {
            Sender = Substitute.For<ISender>();
            DocumentTypeResolver = new DocumentTypeResolver().WithBlipDocuments();
            DocumentSerializer = new DocumentSerializer(DocumentTypeResolver);
        }

        public ISender Sender { get; set; }

        private IDocumentTypeResolver DocumentTypeResolver { get; }

        private IDocumentSerializer DocumentSerializer { get; }

        private SendRawMessageAction GetTarget()
        {
            return new SendRawMessageAction(Sender, DocumentSerializer);
        }

        [Fact]
        public async Task SendWithPlainHttpContentShouldSucceed()
        {
            // Arrange
            var destination = new Identity(Guid.NewGuid().ToString(), "msging.net");
            Context.User.Returns(destination);

            var content = "This is a text content";
            var settings = new SendRawMessageSettings()
            {
                Type = PlainText.MIME_TYPE,
                RawContent = content
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Sender.Received(1).SendMessageAsync(Arg.Is<Message>(m =>
                m.To.ToIdentity().Equals(destination) && m.Type == PlainText.MediaType && m.Content.ToString().Equals(content)), CancellationToken);
        }

        [Fact]
        public async Task SendWithJsonHttpContentShouldSucceed()
        {
            // Arrange
            var destination = new Identity(Guid.NewGuid().ToString(), "msging.net");
            Context.User.Returns(destination);
            var select = new Select()
            {
                Text = "This is the header",
                Scope = SelectScope.Immediate,
                Options = new[]
                {
                    new SelectOption
                    {
                        Text = "This is the first option"
                    },
                    new SelectOption
                    {
                        Text = "This is the second option"
                    },
                    new SelectOption
                    {
                        Text = "This is the third option"
                    }
                }
            };

            var content = DocumentSerializer.Serialize(select);

            var settings = new SendRawMessageSettings
            {
                Type = Select.MIME_TYPE,
                RawContent = content
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Sender.Received(1).SendMessageAsync(
                Arg.Is<Message>(m =>
                    m.To.ToIdentity().Equals(destination)
                    && m.Type == Select.MediaType
                    && m.Content is Select
                    && ((Select)m.Content).Text == select.Text
                    && ((Select)m.Content).Scope == SelectScope.Immediate
                    && ((Select)m.Content).Options.Length == select.Options.Length
                    && ((Select)m.Content).Options[0].Text == select.Options[0].Text
                    && ((Select)m.Content).Options[1].Text == select.Options[1].Text
                    && ((Select)m.Content).Options[2].Text == select.Options[2].Text),
                CancellationToken);
        }
    }
}