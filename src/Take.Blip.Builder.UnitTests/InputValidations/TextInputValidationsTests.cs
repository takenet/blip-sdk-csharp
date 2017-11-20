using System;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Take.Blip.Builder.Models;
using Xunit;
using Action = Take.Blip.Builder.Models.Action;
using Input = Take.Blip.Builder.Models.Input;

#pragma warning disable 4014

namespace Take.Blip.Builder.UnitTests
{
    public class TextInputValidationsTests : FlowManagerTestsBase, IDisposable
    {
     
        [Fact]
        public async Task FlowWithTextTypeInputValidationShouldSendMessageWhenInvalid()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            var invalidInput = new MediaLink();
            var messageType = "text/plain";
            var messageContent = "Pong!";
            var validationMessageContent = "Invalid message type";

            var flow = new Flow()
            {
                Id = Guid.NewGuid().ToString(),
                States = new[]
                {
                    new State
                    {
                        Id = "root",
                        Root = true,
                        Input = new Input
                        {
                            Validation = new InputValidation
                            {
                                Rule = InputValidationRule.Type,
                                Type = PlainText.MediaType,
                                Error = validationMessageContent
                            }
                        },
                        Outputs = new[]
                        {
                            new Output
                            {
                                StateId = "ping"
                            }
                        }
                    },
                    new State
                    {
                        Id = "ping",
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new JObject()
                                {
                                    {"type", messageType},
                                    {"content", messageContent}
                                }
                            }
                        }
                    }
                }
            };
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(invalidInput, User, flow, CancellationToken);
            await target.ProcessInputAsync(input, User, flow, CancellationToken);

            // Assert
            StorageManager.Received(1).SetStateIdAsync(flow.Id, User, "ping", CancellationToken);
            StorageManager.Received(1).DeleteStateIdAsync(flow.Id, User, CancellationToken);
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == validationMessageContent),
                    CancellationToken);
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    CancellationToken);
        }
    }
}
