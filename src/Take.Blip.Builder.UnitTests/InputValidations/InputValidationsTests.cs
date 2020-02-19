using System;
using System.Threading;
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
    public class InputValidationsTests : FlowManagerTestsBase
    {
     
        [Fact]
        public async Task FlowWithRegexInputValidationShouldChangeStateProperly()
        {
            // Arrange
            var input = new PlainText() { Text = "hi" };
            Message.Content = input;
            
            var messageType = "text/plain";
            var messageContent = "Hi for you to!";
            var validationMessageContent = "Invalid message content";

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
                                Rule = InputValidationRule.Regex,
                                Regex = "(hi)",
                                Error = validationMessageContent
                            }
                        },
                        Outputs = new[]
                        {
                            new Output
                            {
                                StateId = "state2"
                            }
                        }
                    },
                    new State
                    {
                        Id = "state2",
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            StateManager.Received(1).SetStateIdAsync(Context, "state2", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());

            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithInvalidRegexInputValidationShouldFailAndNotChangeStateProperly()
        {
            // Arrange
            var input = new PlainText() { Text = "hi" };
            Message.Content = input;

            var messageType = "text/plain";
            var messageContent = "Hi for you to!";
            var validationMessageContent = "Invalid message content";

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
                                Rule = InputValidationRule.Regex,
                                Regex = "(xpto)",
                                Error = validationMessageContent
                            }
                        },
                        Outputs = new[]
                        {
                            new Output
                            {
                                StateId = "state2"
                            }
                        }
                    },
                    new State
                    {
                        Id = "state2",
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            StateManager.Received(0).SetStateIdAsync(Context, "state2", Arg.Any<CancellationToken>());
            StateManager.Received(0).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());

            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == validationMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithNumberInputValidationShouldChangeStateProperly()
        {
            // Arrange
            var input = new PlainText() { Text = "18" };
            Message.Content = input;

            var messageType = "text/plain";
            var messageContent = "Thanks!";
            var validationMessageContent = "Invalid message content";

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
                                Rule = InputValidationRule.Number,
                                Error = validationMessageContent
                            }
                        },
                        Outputs = new[]
                        {
                            new Output
                            {
                                StateId = "state2"
                            }
                        }
                    },
                    new State
                    {
                        Id = "state2",
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            StateManager.Received(1).SetStateIdAsync(Context, "state2", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());

            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithInvalidNumberInputValidationShouldFailAndNotChangeStateProperly()
        {
            // Arrange
            var input = new PlainText() { Text = "some text" };
            Message.Content = input;

            var messageType = "text/plain";
            var messageContent = "Thanks!";
            var validationMessageContent = "Invalid message content";

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
                                Rule = InputValidationRule.Number,
                                Error = validationMessageContent
                            }
                        },
                        Outputs = new[]
                        {
                            new Output
                            {
                                StateId = "state2"
                            }
                        }
                    },
                    new State
                    {
                        Id = "state2",
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            StateManager.Received(0).SetStateIdAsync(Context, "state2", Arg.Any<CancellationToken>());
            StateManager.Received(0).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());

            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == validationMessageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithTypeInputValidationShouldSendMessageWhenInvalid()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
            var invalidInputMessage = new Message()
            {
                From = UserIdentity.ToNode(),
                To = ApplicationIdentity.ToNode(),
                Content = new MediaLink()
            };
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
            await target.ProcessInputAsync(invalidInputMessage, flow, CancellationToken);
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == validationMessageContent),
                    Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithInvalidTypeInputValidationShouldFailAndNotChangeStateProperly()
        {
            // Arrange
            var input = new PlainText() { Text = "Ping!" };
            Message.Content = input;
            var invalidInputMessage = new Message()
            {
                From = UserIdentity.ToNode(),
                To = ApplicationIdentity.ToNode(),
                Content = new MediaLink()
            };
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
            await target.ProcessInputAsync(invalidInputMessage, flow, CancellationToken);
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == validationMessageContent),
                    Arg.Any<CancellationToken>());
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithDateValidationShouldChangeStateProperly()
        {
            // Arrange
            var input = new PlainText() { Text = "2017-11-20T17:13:00Z" };
            Message.Content = input;
            var messageType = "text/plain";
            var messageContent = "Pong!";
            var validationMessageContent = "Invalid message content";

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
                                Rule = InputValidationRule.Date,
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            StateManager.Received(1).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(1).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());
            
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == messageContent),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task FlowWithInvalidDateValidationShouldFailAndNotChangeStateProperly()
        {
            // Arrange
            var input = new PlainText() { Text = "some text different of date" };
            Message.Content = input;
            var invalidInput = new MediaLink();
            var messageType = "text/plain";
            var messageContent = "Pong!";
            var validationMessageContent = "Invalid message content";

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
                                Rule = InputValidationRule.Date,
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
            await target.ProcessInputAsync(Message, flow, CancellationToken);

            // Assert
            StateManager.Received(0).SetStateIdAsync(Context, "ping", Arg.Any<CancellationToken>());
            StateManager.Received(0).DeleteStateIdAsync(Context, Arg.Any<CancellationToken>());

            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(UserIdentity)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == validationMessageContent),
                    Arg.Any<CancellationToken>());
        }
    }
}
