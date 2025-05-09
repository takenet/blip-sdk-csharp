﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Serilog;
using Shouldly;
using Take.Blip.Builder.Actions.ProcessHttp;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Utils;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ProcessHttpActionTests : ActionTestsBase
    {
        private const string USER_IDENTITY = "user@domain.local";
        private const string BOT_IDENTITY = "papagaio@msging.net";
        private const string BOT_IDENTIFIER_CONFIG_VARIABLE_NAME = "processHttpAddBotIdentityToRequestHeader";
        private const string SEND_HEADERS_TO_TRACE_COLLECTOR_VARIABLE_NAME = "sendHeadersToTraceCollector";

        public ProcessHttpActionTests()
        {
            HttpClient = Substitute.For<IHttpClient>();
            Context.Flow.Returns(new Builder.Models.Flow { Configuration = new Dictionary<string, string>() });
            configuration = Substitute.For<IConfiguration>();
            variableReplacer = Substitute.For<IVariableReplacer>();
            sensitiveInfoReplacer = new SensitiveInfoReplacer();
        }

        public IHttpClient HttpClient { get; set; }
        public IConfiguration configuration { get; set; }
        public ISensitiveInfoReplacer sensitiveInfoReplacer { get; set; }
        public IVariableReplacer variableReplacer { get; set; }

        private ProcessHttpAction GetTarget()
        {
            return new ProcessHttpAction(HttpClient, Substitute.For<ILogger>(), configuration, sensitiveInfoReplacer, variableReplacer);
        }

        [Fact]
        public async Task ProcessPostActionShouldSucceed()
        {
            // Arrange
            var settings = new ProcessHttpSettings
            {
                Uri = new Uri("https://blip.ai"),
                Method = HttpMethod.Post.ToString(),
                Body = "{\"plan\":\"Premium\",\"details\":{\"address\": \"Rua X\"}}",
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Authorization", "Key askçjdhaklsdghasklgdasd="}
                },
                ResponseBodyVariable = "httpResultBody",
                ResponseStatusVariable = "httpResultStatus",

            };

            var target = GetTarget();

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Accepted,
                Content = new StringContent("Some result")
            };

            HttpClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>()).Returns(httpResponseMessage);

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await HttpClient.Received(1).SendAsync(
                Arg.Is<HttpRequestMessage>(
                    h => h.RequestUri.Equals(settings.Uri)), Arg.Any<CancellationToken>());

            await Context.Received(1).SetVariableAsync(settings.ResponseStatusVariable, ((int)HttpStatusCode.Accepted).ToString(), Arg.Any<CancellationToken>());
            await Context.Received(1).SetVariableAsync(settings.ResponseBodyVariable, "Some result", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessPostActionWithoutValidSettingsShouldFailed()
        {
            // Arrange
            var settings = new ProcessHttpSettings
            {
                Method = HttpMethod.Post.ToString(),
                Body = "{\"plan\":\"Premium\",\"details\":{\"address\": \"Rua X\"}}",
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Authorization", "Key askçjdhaklsdghasklgdasd="}
                },
            };

            var target = GetTarget();

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Error")
            };

            HttpClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>()).Returns(httpResponseMessage);

            // Act
            try
            {
                await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);
                throw new Exception();
            }
            catch (ValidationException exception)
            {
                // Assert
                await HttpClient.DidNotReceive().SendAsync(
                    Arg.Is<HttpRequestMessage>(
                        h => h.RequestUri.Equals(settings.Uri)), Arg.Any<CancellationToken>());
            }
        }

        [Fact]
        public async Task ProcessActionWithUserHeaderShouldSucceed()
        {
            // Arrange
            const string userIdentity = "user@domain.local";
            const string userToRequestHeaderVariableName = "processHttpAddUserToRequestHeader";
            Context.Flow.Configuration.Add(userToRequestHeaderVariableName, "true");
            Context.UserIdentity.Returns(Identity.Parse(userIdentity));

            var settings = new ProcessHttpSettings
            {
                Uri = new Uri("https://blip.ai"),
                Method = HttpMethod.Post.ToString(),
                Body = "{\"plan\":\"Premium\",\"details\":{\"address\": \"Rua X\"}}",
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Authorization", "Key askçjdhaklsdghasklgdasd="}
                },
                ResponseBodyVariable = "httpResultBody",
                ResponseStatusVariable = "httpResultStatus",

            };

            var target = GetTarget();

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Accepted,
                Content = new StringContent("Some result")
            };

            HttpRequestMessage requestMessage = null;
            HttpClient
                .SendAsync(Arg.Do<HttpRequestMessage>(m => requestMessage = m), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(httpResponseMessage);

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            requestMessage.Headers.Contains("X-Blip-User").ShouldBeTrue();
            requestMessage.Headers.Contains("X-Blip-Bot").ShouldBeFalse();
            requestMessage.Headers.GetValues("X-Blip-User").First().ShouldBe(userIdentity);

            await HttpClient.Received(1).SendAsync(
                Arg.Is<HttpRequestMessage>(
                    h => h.RequestUri.Equals(settings.Uri)), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessActionWithBotIdentifierHeaderShouldSucceed()
        {
            // Arrange
            const string userIdentity = "user@domain.local";
            const string botIdentity = "papagaio@msging.net";
            const string botIdentifierConfigVariableName = "processHttpAddBotIdentityToRequestHeader";
            Context.Flow.Configuration.Add(botIdentifierConfigVariableName, "true");
            Context.UserIdentity.Returns(Identity.Parse(userIdentity));
            Context.OwnerIdentity.Returns(Identity.Parse(botIdentity));

            var settings = new ProcessHttpSettings
            {
                Uri = new Uri("https://test.msging.net"),
                Method = HttpMethod.Post.ToString(),
                Body = "{\"plan\":\"Premium\",\"details\":{\"address\": \"Rua X\"}}",
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Authorization", "Key askçjdhaklsdghasklgdasd="}
                },
                ResponseBodyVariable = "httpResultBody",
                ResponseStatusVariable = "httpResultStatus",

            };

            var target = GetTarget();

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Accepted,
                Content = new StringContent("Some result")
            };

            HttpRequestMessage requestMessage = null;
            HttpClient
                .SendAsync(Arg.Do<HttpRequestMessage>(m => requestMessage = m), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(httpResponseMessage);

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            requestMessage.Headers.Contains("X-Blip-Bot").ShouldBeTrue();
            requestMessage.Headers.Contains("X-Blip-User").ShouldBeFalse();
            requestMessage.Headers.GetValues("X-Blip-Bot").First().ShouldBe(botIdentity);


            await HttpClient.Received(1).SendAsync(
                Arg.Is<HttpRequestMessage>(
                    h => h.RequestUri.Equals(settings.Uri)), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessActionTimeoutShouldSucceed()
        {
            // Arrange
            var settings = new ProcessHttpSettings
            {
                Uri = new Uri("https://blip.ai"),
                Method = HttpMethod.Post.ToString(),
                Body = "{\"plan\":\"Premium\",\"details\":{\"address\": \"Rua X\"}}",
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Authorization", "Key askçjdhaklsdghasklgdasd="}
                },
                ResponseBodyVariable = "httpResultBody",
                ResponseStatusVariable = "httpResultStatus",

            };

            var target = GetTarget();

            HttpClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(async token =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(20), token.Arg<CancellationToken>());
                    return Substitute.For<HttpResponseMessage>();
                });

            // Act
            using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(10)))
                await target.ExecuteAsync(Context, JObject.FromObject(settings), cts.Token);

            //Assert
            await Context.DidNotReceive().SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData("potato", false)]
        [InlineData("false", false)]
        [InlineData("true", true)]
        [InlineData("", false)]
        public async Task ProcessAction_CheckConfigurationVariableValues(string botIdentifierVariableValue, bool expectedResult)
        {
            // Arrange
            const string userIdentity = "user@domain.local";
            const string botIdentity = "papagaio@msging.net";
            const string botIdentifierConfigVariableName = "processHttpAddBotIdentityToRequestHeader";
            Context.Flow.Configuration.Add(botIdentifierConfigVariableName, botIdentifierVariableValue);
            Context.UserIdentity.Returns(Identity.Parse(userIdentity));
            Context.OwnerIdentity.Returns(Identity.Parse(botIdentity));

            var settings = new ProcessHttpSettings
            {
                Uri = new Uri("https://blip.ai"),
                Method = HttpMethod.Post.ToString(),
                Body = "{\"plan\":\"Premium\",\"details\":{\"address\": \"Rua X\"}}",
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Authorization", "Key askçjdhaklsdghasklgdasd="}
                },
                ResponseBodyVariable = "httpResultBody",
                ResponseStatusVariable = "httpResultStatus",

            };

            var target = GetTarget();

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Accepted,
                Content = new StringContent("Some result")
            };

            HttpRequestMessage requestMessage = null;
            HttpClient
                .SendAsync(Arg.Do<HttpRequestMessage>(m => requestMessage = m), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(httpResponseMessage);

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            requestMessage.Headers.Contains("X-Blip-Bot").ShouldBe(expectedResult);
            if (expectedResult)
            {
                requestMessage.Headers.GetValues("X-Blip-Bot").First().ShouldBe(botIdentity);
            }

            await HttpClient.Received(1).SendAsync(
                Arg.Is<HttpRequestMessage>(
                    h => h.RequestUri.Equals(settings.Uri)), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessAction_AddBotIdentifierAndStateId()
        {
            // Arrange
            configuration.InternalUris.Returns("msging.net");

            var settings = new ProcessHttpSettings
            {
                Uri = new Uri("https://test.msging.net"),
                Method = HttpMethod.Post.ToString(),
                Body = "{\"plan\":\"Premium\",\"details\":{\"address\": \"Rua X\"}}",
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Authorization", "Key askçjdhaklsdghasklgdasd="}
                },
                ResponseBodyVariable = "httpResultBody",
                ResponseStatusVariable = "httpResultStatus",

            };

            var target = GetTarget();

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Accepted,
                Content = new StringContent("Some result")
            };

            HttpRequestMessage requestMessage = null;
            HttpClient
                .SendAsync(Arg.Do<HttpRequestMessage>(m => requestMessage = m), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(httpResponseMessage);

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            requestMessage.Headers.Contains("X-Blip-Bot").ShouldBeTrue();
            requestMessage.Headers.Contains("X-Blip-StateId").ShouldBeTrue();

            await HttpClient.Received(1).SendAsync(
                Arg.Is<HttpRequestMessage>(
                    h => h.RequestUri.Equals(settings.Uri)), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessAction_CheckIfInternalUriNotExist()
        {
            // Arrange
            var settings = new ProcessHttpSettings
            {
                Uri = new Uri("https://blip.ai"),
                Method = HttpMethod.Post.ToString(),
                Body = "{\"plan\":\"Premium\",\"details\":{\"address\": \"Rua X\"}}",
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Authorization", "Key askçjdhaklsdghasklgdasd="}
                },
                ResponseBodyVariable = "httpResultBody",
                ResponseStatusVariable = "httpResultStatus",

            };

            var target = GetTarget();

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Accepted,
                Content = new StringContent("Some result")
            };

            HttpRequestMessage requestMessage = null;
            HttpClient
                .SendAsync(Arg.Do<HttpRequestMessage>(m => requestMessage = m), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(httpResponseMessage);

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            requestMessage.Headers.Contains("X-Blip-Bot").ShouldBeFalse();
            requestMessage.Headers.Contains("X-Blip-StateId").ShouldBeFalse();

            await HttpClient.Received(1).SendAsync(
              Arg.Is<HttpRequestMessage>(
                  h => h.RequestUri.Equals(settings.Uri)), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessAction_CheckTwoOrMoreInternalUris()
        {
            // Arrange
            configuration.InternalUris.Returns("msging.net;blip.ai");
            var settings = new ProcessHttpSettings
            {
                Uri = new Uri("https://test.msging.net"),
                Method = HttpMethod.Post.ToString(),
                Body = "{\"plan\":\"Premium\",\"details\":{\"address\": \"Rua X\"}}",
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Authorization", "Key askçjdhaklsdghasklgdasd="}
                },
                ResponseBodyVariable = "httpResultBody",
                ResponseStatusVariable = "httpResultStatus",

            };

            var target = GetTarget();

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Accepted,
                Content = new StringContent("Some result")
            };

            HttpRequestMessage requestMessage = null;
            HttpClient
                .SendAsync(Arg.Do<HttpRequestMessage>(m => requestMessage = m), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(httpResponseMessage);

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            requestMessage.Headers.Contains("X-Blip-Bot").ShouldBeTrue();
            requestMessage.Headers.Contains("X-Blip-StateId").ShouldBeTrue();

            await HttpClient.Received(1).SendAsync(
              Arg.Is<HttpRequestMessage>(
                  h => h.RequestUri.Equals(settings.Uri)), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessAction_CheckIfHeaderWillBeNotDuplicated()
        {
            // Arrange
            const string botIdentifierConfigVariableName = "processHttpAddBotIdentityToRequestHeader";
            configuration.InternalUris.Returns("msging.net");
            Context.Flow.Configuration.Add(botIdentifierConfigVariableName, "true");

            var settings = new ProcessHttpSettings
            {
                Uri = new Uri("https://test.msging.net"),
                Method = HttpMethod.Post.ToString(),
                Body = "{\"plan\":\"Premium\",\"details\":{\"address\": \"Rua X\"}}",
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Authorization", "Key askçjdhaklsdghasklgdasd="}
                },
                ResponseBodyVariable = "httpResultBody",
                ResponseStatusVariable = "httpResultStatus",

            };

            var target = GetTarget();

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Accepted,
                Content = new StringContent("Some result")
            };

            HttpRequestMessage requestMessage = null;
            HttpClient
                .SendAsync(Arg.Do<HttpRequestMessage>(m => requestMessage = m), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(httpResponseMessage);

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert

            requestMessage.Headers.GetValues("X-Blip-Bot").Count().ShouldBe(1);

            await HttpClient.Received(1).SendAsync(
              Arg.Is<HttpRequestMessage>(
                  h => h.RequestUri.Equals(settings.Uri)), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessAction_CheckIfInternalUriNotMatch()
        {
            // Arrange
            configuration.InternalUris.Returns("msging.net");

            var settings = new ProcessHttpSettings
            {
                Uri = new Uri("https://blip.ai"),
                Method = HttpMethod.Post.ToString(),
                Body = "{\"plan\":\"Premium\",\"details\":{\"address\": \"Rua X\"}}",
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"Authorization", "Key askçjdhaklsdghasklgdasd="}
                },
                ResponseBodyVariable = "httpResultBody",
                ResponseStatusVariable = "httpResultStatus",

            };

            var target = GetTarget();

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Accepted,
                Content = new StringContent("Some result")
            };

            HttpRequestMessage requestMessage = null;
            HttpClient
                .SendAsync(Arg.Do<HttpRequestMessage>(m => requestMessage = m), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(httpResponseMessage);

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            requestMessage.Headers.Contains("X-Blip-Bot").ShouldBeFalse();
            requestMessage.Headers.Contains("X-Blip-StateId").ShouldBeFalse();

            await HttpClient.Received(1).SendAsync(
              Arg.Is<HttpRequestMessage>(
                  h => h.RequestUri.Equals(settings.Uri)), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessAction_ChangeHttpTraceHeadersSucceed()
        {
            // Arrange
            Context.Flow.Configuration.Add(BOT_IDENTIFIER_CONFIG_VARIABLE_NAME, "true");
            Context.UserIdentity.Returns(Identity.Parse(USER_IDENTITY));
            Context.OwnerIdentity.Returns(Identity.Parse(BOT_IDENTITY));

            var actionTrace = new ActionTrace
            {
                Order = 1,
                Type = "ProcessHttp",
                ContinueOnError = true,
                ParsedSettings = new JRaw(@"{""headers"":{""BotKey"":""Key AAAAAAAAAAAAA"",""OtherHeader"":""OtherValue"",""Content-Type"":""application/json""},""method"":""GET"",""uri"":""https://enz557qv71nso.x.pipedream.net""}")
            };

            Context.InputContext.TryGetValue("current-action-trace", out Arg.Any<object>()).Returns(x =>
            {
                x[1] = actionTrace;
                return true;
            });

            var settings = new ProcessHttpSettings
            {
                Uri = new Uri("https://test.msging.net"),
                Method = HttpMethod.Post.ToString(),
                Body = "{\"plan\":\"Premium\",\"details\":{\"address\": \"Rua X\"}}",
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"BotKey", "Key AAAAAAAAAAAAA"},
                    {"OtherHeader", "OtherValue" }
                },
                ResponseBodyVariable = "httpResultBody",
                ResponseStatusVariable = "httpResultStatus",

            };

            var target = GetTarget();

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Accepted,
                Content = new StringContent("Some result")
            };

            HttpRequestMessage requestMessage = null;
            HttpClient
                .SendAsync(Arg.Do<HttpRequestMessage>(m => requestMessage = m), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(httpResponseMessage);

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            requestMessage.Headers.Contains("X-Blip-Bot").ShouldBeTrue();
            requestMessage.Headers.Contains("X-Blip-User").ShouldBeFalse();
            requestMessage.Headers.GetValues("X-Blip-Bot").First().ShouldBe(BOT_IDENTITY);

            var parsedSettings = JsonConvert.DeserializeObject<ProcessHttpSettings>(Context.GetCurrentActionTrace().ParsedSettings.ToString());

            parsedSettings.Headers.ShouldNotBeNull();
            parsedSettings.Headers.ShouldContainKeyAndValue("Content-Type", "***");
            parsedSettings.Headers.ShouldContainKeyAndValue("BotKey", "***");
            parsedSettings.Headers.ShouldContainKeyAndValue("OtherHeader", "***");

            await HttpClient.Received(1).SendAsync(
                Arg.Is<HttpRequestMessage>(
                    h => h.RequestUri.Equals(settings.Uri)), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessAction_FlowConfiguredWithSendHeadersToTraceShouldSucceed()
        {
            // Arrange
            Context.Flow.Configuration.Add(BOT_IDENTIFIER_CONFIG_VARIABLE_NAME, "true");
            Context.Flow.Configuration.Add(SEND_HEADERS_TO_TRACE_COLLECTOR_VARIABLE_NAME, "true");

            Context.UserIdentity.Returns(Identity.Parse(USER_IDENTITY));
            Context.OwnerIdentity.Returns(Identity.Parse(BOT_IDENTITY));

            var actionTrace = new ActionTrace
            {
                Order = 1,
                Type = "ProcessHttp",
                ContinueOnError = true,
                ParsedSettings = new JRaw(@"{""headers"":{""BotKey"":""Key AAAAAAAAAAAAA"",""OtherHeader"":""OtherValue"",""Content-Type"":""application/json""},""method"":""GET"",""uri"":""https://enz557qv71nso.x.pipedream.net""}")
            };

            Context.InputContext.TryGetValue("current-action-trace", out Arg.Any<object>()).Returns(x =>
            {
                x[1] = actionTrace;
                return true;
            });

            var settings = new ProcessHttpSettings
            {
                Uri = new Uri("https://test.msging.net"),
                Method = HttpMethod.Post.ToString(),
                Body = "{\"plan\":\"Premium\",\"details\":{\"address\": \"Rua X\"}}",
                Headers = new Dictionary<string, string>()
                {
                    {"Content-Type", "application/json"},
                    {"BotKey", "Key AAAAAAAAAAAAA"},
                    {"OtherHeader", "OtherValue" }
                },
                ResponseBodyVariable = "httpResultBody",
                ResponseStatusVariable = "httpResultStatus",

            };

            var target = GetTarget();

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Accepted,
                Content = new StringContent("Some result")
            };

            HttpRequestMessage requestMessage = null;
            HttpClient
                .SendAsync(Arg.Do<HttpRequestMessage>(m => requestMessage = m), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(httpResponseMessage);

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            requestMessage.Headers.Contains("X-Blip-Bot").ShouldBeTrue();
            requestMessage.Headers.Contains("X-Blip-User").ShouldBeFalse();
            requestMessage.Headers.GetValues("X-Blip-Bot").First().ShouldBe(BOT_IDENTITY);

            var parsedSettings = JsonConvert.DeserializeObject<ProcessHttpSettings>(Context.GetCurrentActionTrace().ParsedSettings.ToString());

            parsedSettings.Headers.ShouldNotBeNull();
            parsedSettings.Headers.ShouldContainKeyAndValue("Content-Type", "application/json");
            parsedSettings.Headers.ShouldContainKeyAndValue("BotKey", "Key AAAAAAAAAAAAA");
            parsedSettings.Headers.ShouldContainKeyAndValue("OtherHeader", "OtherValue");

            await HttpClient.Received(1).SendAsync(
                Arg.Is<HttpRequestMessage>(
                    h => h.RequestUri.Equals(settings.Uri)), Arg.Any<CancellationToken>());
        }
    }
}
