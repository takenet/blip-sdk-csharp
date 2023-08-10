using Lime.Messaging.Contents;
using System.Threading.Tasks;
using Xunit;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using NSubstitute;
using System.Collections.Generic;
using System;
using Take.Blip.Builder.Diagnostics;

namespace Take.Blip.Builder.UnitTests
{
    public class InputReplyHandlerTests
    {
        private readonly IDocumentTypeResolver _documentTypeResolver;

        public InputReplyHandlerTests()
        {
            _documentTypeResolver = Substitute.For<IDocumentTypeResolver>();

            UserIdentity = new Identity("user", "domain");
            ApplicationIdentity = new Identity("application", "domain");
            Message = new Message()
            {
                From = UserIdentity.ToNode(),
                To = ApplicationIdentity.ToNode()
            };

            var documentSerializer = new DocumentSerializer(_documentTypeResolver);
            InputHandler = new InputReplyHandler(documentSerializer);
        }

        public Message Message { get; }
        public Identity UserIdentity { get; }
        public Identity ApplicationIdentity { get; }
        public InputReplyHandler InputHandler { get; }

        [Fact]
        public async Task HandleMessage_WhenInReplyToIsNull()
        {
            // Arrange
            var plainText = MockPlainText();
            Message.Content = MockReplyMessage(plainText, isInReplyToNull: true);

            // Act
            var (messageHasChanged, returnedMessage) = InputHandler.HandleMessage(Message);

            // Assert
            returnedMessage.Content.Equals(plainText);
            Assert.True(messageHasChanged);
            Assert.True(returnedMessage.Metadata.ContainsKey(InputReplyHandler.REPLY_CONTENT));
            Assert.False(returnedMessage.Metadata.ContainsKey(InputReplyHandler.IN_REPLY_TO_ID));
        }

        [Fact]
        public async Task HandleMessage_WhenMessageContentIsNotReply()
        {
            // Arrange
            var plainText = MockPlainText();
            Message.Content = plainText;

            // Act
            var (messageHasChanged, returnedMessage) = InputHandler.HandleMessage(Message);

            // Assert
            returnedMessage.Content.Equals(plainText);
            Assert.False(messageHasChanged);
            Assert.Null(returnedMessage.Metadata);
        }

        [Fact]
        public async Task HandleMessage_WhenMessageHasMetadata()
        {
            // Arrange
            var traceIdentity = new Identity(Guid.NewGuid().ToString(), "msging.net");
            var mode = "All";
            var targetType = "Lime";

            Message.Metadata = new Dictionary<string, string>
            {
                { TraceSettings.BUILDER_TRACE_MODE, mode },
                { TraceSettings.BUILDER_TRACE_TARGET_TYPE, targetType },
                { TraceSettings.BUILDER_TRACE_TARGET, traceIdentity },
            };

            var plainText = MockPlainText();
            Message.Content = MockReplyMessage(plainText);

            // Act
            var (messageHasChanged, returnedMessage) = InputHandler.HandleMessage(Message);

            // Assert
            returnedMessage.Content.Equals(plainText);
            Assert.True(returnedMessage.Metadata.ContainsKey(TraceSettings.BUILDER_TRACE_TARGET));
            Assert.True(returnedMessage.Metadata.ContainsKey(TraceSettings.BUILDER_TRACE_TARGET_TYPE));
            Assert.True(returnedMessage.Metadata.ContainsKey(TraceSettings.BUILDER_TRACE_MODE));
            AssertMessageMetadatas(messageHasChanged, returnedMessage);
        }

        [Fact]
        public async Task HandleMessage_WhenMessageContentIsPlainText()
        {
            // Arrange
            var plainText = MockPlainText();
            Message.Content = MockReplyMessage(plainText);

            // Act
            var (messageHasChanged, returnedMessage) = InputHandler.HandleMessage(Message);

            // Assert
            returnedMessage.Content.Equals(plainText);
            AssertMessageMetadatas(messageHasChanged, returnedMessage);
        }

        [Fact]
        public async Task HandleMessage_WhenMessageContentIsLocation()
        {
            // Arrange
            var location = new Location
            {
                Latitude = 34.988889,
                Longitude = -106.614444,
                Text = "Text"
            };
            Message.Content = MockReplyMessage(location);

            // Act
            var (messageHasChanged, returnedMessage) = InputHandler.HandleMessage(Message);

            // Assert
            returnedMessage.Content.Equals(location);
            AssertMessageMetadatas(messageHasChanged, returnedMessage);
        }

        [Fact]
        public async Task HandleMessage_WhenMessageContentIsMediaLink()
        {
            // Arrange
            var mediaLink = new MediaLink
            {
                Type = new MediaType("application", "vnd.openxmlformats-officedocument.presentationml.presentation"),
                Uri = new System.Uri("https://www.uri.com.br/file.pptx"),
                Text = "Text"
            };
            Message.Content = MockReplyMessage(mediaLink);

            // Act
            var (messageHasChanged, returnedMessage) = InputHandler.HandleMessage(Message);

            // Assert
            returnedMessage.Content.Equals(mediaLink);
            AssertMessageMetadatas(messageHasChanged, returnedMessage);
        }

        [Fact]
        public async Task HandleMessage_WhenMessageContentIsReply()
        {
            // Arrange
            var plainText = MockPlainText();
            var reply = MockReplyMessage(plainText);
            Message.Content = MockReplyMessage(reply);

            // Act
            var (messageHasChanged, returnedMessage) = InputHandler.HandleMessage(Message);

            // Assert
            returnedMessage.Content.Equals(plainText);
            AssertMessageMetadatas(messageHasChanged, returnedMessage);
        }

        private static Document MockReplyMessage(Document document = null, bool isInReplyToNull = false) =>
            new Reply()
            {
                Replied = new DocumentContainer()
                {
                    Value = new PlainText
                    {
                        Text = "Text"
                    }
                },
                InReplyTo = isInReplyToNull
                    ? null
                    : new InReplyTo
                    {
                        Id = "ReplyToId",
                        Value = document,
                        Direction = MessageDirection.Received
                    }
            };

        private static void AssertMessageMetadatas(bool messageHasChanged, Message message)
        {
            Assert.True(messageHasChanged);
            Assert.True(message.Metadata.ContainsKey(InputReplyHandler.IN_REPLY_TO_ID));
            Assert.True(message.Metadata.ContainsKey(InputReplyHandler.REPLY_CONTENT));
        }

        private static Document MockPlainText() =>
            new PlainText
            {
                Text = "Text"
            };
    }
}
