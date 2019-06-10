using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Serilog;
using Shouldly;
using Take.Blip.Builder.Actions.ProcessHttp;
using Take.Blip.Builder.Utils;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ProcessHttpActionTests : ActionTestsBase
    {
        public ProcessHttpActionTests()
        {
            HttpClient = Substitute.For<IHttpClient>();
            Context.Flow.Returns(new Builder.Models.Flow { Configuration = new Dictionary<string, string>() });
        }

        public IHttpClient HttpClient { get; set; }

        private ProcessHttpAction GetTarget()
        {
            return new ProcessHttpAction(HttpClient, Substitute.For<ILogger>());
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

            await Context.Received(1).SetVariableAsync(settings.ResponseStatusVariable, ((int) HttpStatusCode.Accepted).ToString(),                 Arg.Any<CancellationToken>());
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
            Context.ApplicationIdentity.Returns(Identity.Parse(botIdentity));

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
            requestMessage.Headers.Contains("X-Blip-Bot").ShouldBeTrue();
            requestMessage.Headers.Contains("X-Blip-User").ShouldBeFalse();
            requestMessage.Headers.GetValues("X-Blip-Bot").First().ShouldBe(botIdentity);

            await HttpClient.Received(1).SendAsync(
                Arg.Is<HttpRequestMessage>(
                    h => h.RequestUri.Equals(settings.Uri)), Arg.Any<CancellationToken>());
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
            Context.ApplicationIdentity.Returns(Identity.Parse(botIdentity));

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
    }
}
