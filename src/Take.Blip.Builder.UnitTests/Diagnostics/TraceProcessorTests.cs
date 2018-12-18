using Newtonsoft.Json.Linq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Utils;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Diagnostics
{
    public class TraceProcessorTests : CancellationTokenTestsBase
    {
        public TraceProcessorTests()
        {
            HttpClient = Substitute.For<IHttpClient>();
        }

        public IHttpClient HttpClient { get; set; }

        private TraceProcessor GetTarget()
        {
            return new TraceProcessor(HttpClient);
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
            HttpClient.Received(1).SendAsync(
                Arg.Is<HttpRequestMessage>(r =>
                    r.Method.Method == "POST" &&
                    r.Content != null &&
                    r.Content.Headers.ContentType.MediaType == "application/json"),
                Arg.Any<CancellationToken>());
        }
    }
}
