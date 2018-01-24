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
    public class InputValidationsTests : FlowManagerTestsBase
    {
     
        [Fact]
        public async Task FlowWithRegexInputValidationShouldChangeStateProperly()
        {
            // Arrange
            var input = new PlainText() { Text = "hi" };
            
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
            await target.ProcessInputAsync(input, User, flow, CancellationToken);

            // Assert
            StateManager.Received(1).SetStateIdAsync(flow.Id, User, "state2", CancellationToken);
            StateManager.Received(1).DeleteStateIdAsync(flow.Id, User, CancellationToken);

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

        [Fact]
        public async Task FlowWithInvalidRegexInputValidationShouldFailAndNotChangeStateProperly()
        {
            // Arrange
            var input = new PlainText() { Text = "hi" };

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
            await target.ProcessInputAsync(input, User, flow, CancellationToken);

            // Assert
            StateManager.Received(0).SetStateIdAsync(flow.Id, User, "state2", CancellationToken);
            StateManager.Received(0).DeleteStateIdAsync(flow.Id, User, CancellationToken);

            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == validationMessageContent),
                    CancellationToken);
        }

        [Fact]
        public async Task FlowWithNumberInputValidationShouldChangeStateProperly()
        {
            // Arrange
            var input = new PlainText() { Text = "18" };

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
            await target.ProcessInputAsync(input, User, flow, CancellationToken);

            // Assert
            StateManager.Received(1).SetStateIdAsync(flow.Id, User, "state2", CancellationToken);
            StateManager.Received(1).DeleteStateIdAsync(flow.Id, User, CancellationToken);

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

        [Fact]
        public async Task FlowWithInvalidNumberInputValidationShouldFailAndNotChangeStateProperly()
        {
            // Arrange
            var input = new PlainText() { Text = "some text" };

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
            await target.ProcessInputAsync(input, User, flow, CancellationToken);

            // Assert
            StateManager.Received(0).SetStateIdAsync(flow.Id, User, "state2", CancellationToken);
            StateManager.Received(0).DeleteStateIdAsync(flow.Id, User, CancellationToken);

            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == validationMessageContent),
                    CancellationToken);
        }

        [Fact]
        public async Task FlowWithTypeInputValidationShouldSendMessageWhenInvalid()
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
            StateManager.Received(1).SetStateIdAsync(flow.Id, User, "ping", CancellationToken);
            StateManager.Received(1).DeleteStateIdAsync(flow.Id, User, CancellationToken);
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

        [Fact]
        public async Task FlowWithInvalidTypeInputValidationShouldFailAndNotChangeStateProperly()
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
            StateManager.Received(1).SetStateIdAsync(flow.Id, User, "ping", CancellationToken);
            StateManager.Received(1).DeleteStateIdAsync(flow.Id, User, CancellationToken);
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

        [Fact]
        public async Task FlowWithDateValidationShouldChangeStateProperly()
        {
            // Arrange
            var input = new PlainText() { Text = "2017-11-20T17:13:00Z" };
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
            await target.ProcessInputAsync(input, User, flow, CancellationToken);

            // Assert
            StateManager.Received(1).SetStateIdAsync(flow.Id, User, "ping", CancellationToken);
            StateManager.Received(1).DeleteStateIdAsync(flow.Id, User, CancellationToken);
            
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

        [Fact]
        public async Task FlowWithInvalidDateValidationShouldFailAndNotChangeStateProperly()
        {
            // Arrange
            var input = new PlainText() { Text = "some text different of date" };
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
            await target.ProcessInputAsync(input, User, flow, CancellationToken);

            // Assert
            StateManager.Received(0).SetStateIdAsync(flow.Id, User, "ping", CancellationToken);
            StateManager.Received(0).DeleteStateIdAsync(flow.Id, User, CancellationToken);

            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m =>
                        m.Id != null
                        && m.To.ToIdentity().Equals(User)
                        && m.Type.ToString().Equals(messageType)
                        && m.Content.ToString() == validationMessageContent),
                    CancellationToken);
        }
    }
}
