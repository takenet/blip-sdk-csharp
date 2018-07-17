using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Lime.Messaging;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Take.Blip.Builder.Actions.SendMessageFromHttp;
using Take.Blip.Builder.Utils;
using Take.Blip.Client;
using Take.Blip.Client.Extensions;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class SendMessageFromHttpActionTests : ActionTestsBase
    {
        public SendMessageFromHttpActionTests()
        {
            Sender = Substitute.For<ISender>();
            HttpClient = Substitute.For<IHttpClient>();
            DocumentTypeResolver = new DocumentTypeResolver().WithBlipDocuments();
            DocumentSerializer = new DocumentSerializer(DocumentTypeResolver);
        }

        public ISender Sender { get; set; }

        public IHttpClient HttpClient { get; set; }

        private IDocumentTypeResolver DocumentTypeResolver { get; }

        private IDocumentSerializer DocumentSerializer { get; }

        private SendMessageFromHttpAction GetTarget()
        {
            return new SendMessageFromHttpAction(Sender, HttpClient, DocumentSerializer);
        }

        [Fact]
        public async Task SendWithPlainHttpContentShouldSucceed()
        {
            // Arrange
            var destination = new Identity(Guid.NewGuid().ToString(), "msging.net");
            Context.User.Returns(destination);
            var uri = new Uri("https://myserver.com/content/id");
            var content = "This is a text content";
            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content)
            };
            HttpClient
                .SendAsync(Arg.Is<HttpRequestMessage>(r => r.RequestUri == uri && r.Method == HttpMethod.Get),
                    CancellationToken)
                .Returns(httpResponseMessage);
            var settings = new SendMessageFromHttpSettings
            {
                Uri = uri,
                Type = PlainText.MIME_TYPE
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
            var uri = new Uri("https://myserver.com/content/id");
            var content = new Select()
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

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(DocumentSerializer.Serialize(content))
            };
            HttpClient
                .SendAsync(Arg.Is<HttpRequestMessage>(r => r.RequestUri == uri && r.Method == HttpMethod.Get),
                    CancellationToken)
                .Returns(httpResponseMessage);
            var settings = new SendMessageFromHttpSettings
            {
                Uri = uri,
                Type = Select.MIME_TYPE
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
                    && ((Select)m.Content).Text == content.Text
                    && ((Select)m.Content).Scope == SelectScope.Immediate
                    && ((Select)m.Content).Options.Length == content.Options.Length
                    && ((Select)m.Content).Options[0].Text == content.Options[0].Text
                    && ((Select)m.Content).Options[1].Text == content.Options[1].Text
                    && ((Select)m.Content).Options[2].Text == content.Options[2].Text),
                CancellationToken);
        }
    }
}