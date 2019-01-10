using Lime.Protocol;
using Newtonsoft.Json.Linq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Utils;
using Take.Blip.Client;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Diagnostics
{
    public class TraceProcessorTests : CancellationTokenTestsBase
    {
        public TraceProcessorTests()
        {
            HttpClient = Substitute.For<IHttpClient>();
            Sender = Substitute.For<ISender>();
        }

        public IHttpClient HttpClient { get; set; }

        public ISender Sender { get; set; }

        private TraceProcessor GetTarget()
        {
            return new TraceProcessor(HttpClient, Sender);
        }

        [Fact]
        public async Task ProcessHttpTargetEventShouldSucceed()
        {
            // Arrange
            var traceEvent = new TraceEvent
            {
                Settings = new TraceSettings
                {
                    Mode = TraceMode.All,
                    TargetType = TraceTargetType.Http,
                    Target = "http://myserver.com"
                },
                Trace = new InputTrace
                {
                    ElapsedMilliseconds = 100,
                    FlowId = "0",
                    Input = "Hello world",
                    User = "user@domain.com",
                    States = new[]
                    {
                        new StateTrace
                        {
                            Id = "1",
                            InputActions = new List<ActionTrace>
                            {
                                new ActionTrace
                                {
                                    Type = "ProcessHttp",
                                    ParsedSettings = new JObject
                                    {
                                        { "property1", "value1" },
                                        { "property2", 2 }
                                    },
                                    ElapsedMilliseconds = 150
                                }
                            },
                            Outputs = new List<OutputTrace>
                            {
                                new OutputTrace
                                {
                                    StateId = "124",
                                    ElapsedMilliseconds = 241,
                                    ConditionsCount = 3,
                                    Error = "Error processing output"
                                }
                            }
                        },
                        new StateTrace
                        {
                            Id = "2",
                            OutputActions = new List<ActionTrace>
                            {
                                new ActionTrace
                                {
                                    Type = "SendMessage",
                                    ParsedSettings = new JObject
                                    {
                                        { "to", "user@domain.com" },
                                        { "type", "text/plain" },
                                        { "content", "Hi there!" }
                                    },
                                    ElapsedMilliseconds = 150
                                }
                            }
                        }
                    }
                }
            };
            var target = GetTarget();

            HttpClient
                .SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage());

            // Act
            await target.ProcessTraceAsync(traceEvent, CancellationToken);

            // Assert
            await HttpClient.Received(1).SendAsync(
                Arg.Is<HttpRequestMessage>(r =>
                    r.Method.Method == "POST" &&
                    r.Content != null &&
                    r.Content.Headers.ContentType.MediaType == "application/json"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessLimeTargetEventShouldSucceed()
        {
            // Arrange
            var traceIndentity = new Identity(Guid.NewGuid().ToString(), "msging.net");
            var traceEvent = new TraceEvent
            {
                Settings = new TraceSettings
                {
                    Mode = TraceMode.All,
                    TargetType = TraceTargetType.Lime,
                    Target = traceIndentity
                },
                Trace = new InputTrace
                {
                    ElapsedMilliseconds = 100,
                    FlowId = "0",
                    Input = "Hello world",
                    User = "user@domain.com",
                    States = new[]
                    {
                        new StateTrace
                        {
                            Id = "1",
                            InputActions = new List<ActionTrace>
                            {
                                new ActionTrace
                                {
                                    Type = "ProcessHttp",
                                    ParsedSettings = new JObject
                                    {
                                        { "property1", "value1" },
                                        { "property2", 2 }
                                    },
                                    ElapsedMilliseconds = 150
                                }
                            },
                            Outputs = new List<OutputTrace>
                            {
                                new OutputTrace
                                {
                                    StateId = "124",
                                    ElapsedMilliseconds = 241,
                                    ConditionsCount = 3,
                                    Error = "Error processing output"
                                }
                            }
                        },
                        new StateTrace
                        {
                            Id = "2",
                            OutputActions = new List<ActionTrace>
                            {
                                new ActionTrace
                                {
                                    Type = "SendMessage",
                                    ParsedSettings = new JObject
                                    {
                                        { "to", "user@domain.com" },
                                        { "type", "text/plain" },
                                        { "content", "Hi there!" }
                                    },
                                    ElapsedMilliseconds = 150
                                }
                            }
                        }
                    }
                }
            };
            var target = GetTarget();

            // Act
            await target.ProcessTraceAsync(traceEvent, CancellationToken);

            // Assert
            await Sender.Received(1).SendMessageAsync(
                Arg.Is<Message>(m =>
                    m.Id != null &&
                    m.Content != null &&
                    m.To != traceIndentity),
                Arg.Any<CancellationToken>());
        }
    }
}