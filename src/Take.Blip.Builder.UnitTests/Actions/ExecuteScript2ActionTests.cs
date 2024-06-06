using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ClearScript;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Serilog;
using Shouldly;
using Take.Blip.Builder.Actions.ExecuteScriptV2;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Utils;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ExecuteScript2ActionTests : ActionTestsBase
    {
        private static ExecuteScriptV2Action GetTarget(IHttpClient client = null)
        {
            var configuration = new TestConfiguration();
            var conventions = new ConventionsConfiguration();

            configuration.ExecuteScriptV2Timeout = TimeSpan.FromMilliseconds(300);
            configuration.ExecuteScriptV2MaxRuntimeHeapSize =
                conventions.ExecuteScriptV2MaxRuntimeHeapSize;
            configuration.ExecuteScriptV2MaxRuntimeStackUsage =
                conventions.ExecuteScriptV2MaxRuntimeStackUsage;

            return new ExecuteScriptV2Action(configuration, client ?? Substitute.For<IHttpClient>(),
                Substitute.For<ILogger>());
        }

        [Fact]
        public async Task ExecuteWithSingleStatementScriptShouldSucceed()
        {
            // Arrange
            const string variableName = "variable1";
            const string variableValue = "my variable 1 value";
            var settings = new ExecuteScriptV2Settings
            {
                Source = $"function run() {{ return '{variableValue}'; }}",
                OutputVariable = variableName
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(),
                CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync(variableName, variableValue,
                CancellationToken);
        }

        [Fact]
        public async Task ExecuteWithCustomTimeZoneDateStringAndTimeStringShouldWork()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                // Fixed date to test timezone
                Source =
                    "function run() { return time.parseDate('2021-01-01T00:00:10').toDateString() + ' ' + time.parseDate('2021-01-01T00:00:10').toTimeString(); }",
                OutputVariable = "test",
                LocalTimeZoneEnabled = true
            };
            var target = GetTarget();

            Context.Flow.Configuration["builder:#localTimeZone"] = "Asia/Shanghai";

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            // Jint doesn't support toLocaleString, so it will return the default date format
            await Context.Received(1).SetVariableAsync("test", "Fri Jan 01 2021 11:00:10 GMT+08:00",
                CancellationToken);
        }

        [Fact]
        public async Task ExecuteWithCustomTimeZoneDateStringAndTimeStringWithAmericaShouldWork()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                // Fixed date to test timezone
                Source =
                    "function run() { return time.parseDate('2021-01-01T00:00:10').toDateString() + ' ' + time.parseDate('2021-01-01T00:00:10').toTimeString(); }",
                OutputVariable = "test",
                LocalTimeZoneEnabled = true
            };
            var target = GetTarget();

            Context.Flow.Configuration["builder:#localTimeZone"] = "America/Sao_Paulo";

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            // Jint doesn't support toLocaleString, so it will return the default date format
            await Context.Received(1).SetVariableAsync("test",
                "Fri Jan 01 2021 00:00:10 GMT-03:00", CancellationToken);
        }

        [Fact]
        public async Task ExecuteWithCustomTimeZoneStringMethodsShouldBeTheSame()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                // Fixed date to test timezone
                Source = @"
function run() {
    var parsedDate = time.parseDate('2021-01-01T00:00:10');

    return (parsedDate.toDateString() + ' ' + parsedDate.toTimeString()) == parsedDate.toString();
}",
                OutputVariable = "test",
                LocalTimeZoneEnabled = true
            };
            var target = GetTarget();

            Context.Flow.Configuration["builder:#localTimeZone"] = "Asia/Shanghai";

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            // Jint doesn't support toLocaleString, so it will return the default date format
            await Context.Received(1).SetVariableAsync("test", "true", CancellationToken);
        }


        [Fact]
        public async Task ExecuteThrowExceptionTest()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                Source = $"function run() {{ throw new Error('Test error'); }}",
                OutputVariable = "variable1",
                CaptureExceptions = true,
                ExceptionVariable = "exception"
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync("exception", "Error: Test error",
                CancellationToken);
        }

        [Fact]
        public async Task ExecuteWithArgumentsShouldSucceed()
        {
            // Arrange
            const string number1 = "100";
            const string number2 = "250";
            Context.GetVariableAsync(nameof(number1), CancellationToken).Returns(number1);
            Context.GetVariableAsync(nameof(number2), CancellationToken).Returns(number2);

            var settings = new ExecuteScriptV2Settings
            {
                InputVariables = new[] { nameof(number1), nameof(number2) },
                Source = @"
function run(number1, number2) {
    return parseInt(number1) + parseInt(number2);
}",
                OutputVariable = "result"
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(),
                CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync("result", "350", CancellationToken);
        }

        [Fact]
        public async Task ExecuteSetContextVariableShouldSucceed()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                Source = @"
async function run() {
    await context.setVariableAsync('test', 100);

    return true;
}",
                OutputVariable = "result"
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync("test", "100", Arg.Any<CancellationToken>());
            await Context.Received(1)
                .SetVariableAsync("result", "true", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ExecuteMultipleAsyncResults()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                Source = @"
async function testNum() {
    return 1;
}

async function testStr() {
    return 'bla';
}

async function testRecursiveAsync() {
    return await testStr();
}

async function run() {
    return {
        'num': testNum(),
        'str': testStr(),
        'recursive': testRecursiveAsync()
    };
}",
                OutputVariable = "result"
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync("result",
                "{\"num\":1,\"str\":\"bla\",\"recursive\":\"bla\"}", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ExecuteWithMissingArgumentsShouldSucceed()
        {
            // Arrange
            const string number1 = "100";
            const string number2 = "250";
            Context.GetVariableAsync(nameof(number1), CancellationToken).Returns(number1);
            Context.GetVariableAsync(nameof(number2), CancellationToken).Returns(number2);

            var settings = new ExecuteScriptV2Settings
            {
                InputVariables = new[] { nameof(number1), nameof(number2) },
                Source = @"
function run(number1, number2, number3) {
    return parseInt(number1) + parseInt(number2) + (number3 || 150);
}",
                OutputVariable = "result"
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(),
                CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync("result", "500", CancellationToken);
        }

        [Fact]
        public async Task ExecuteUsingLetAndConstVariablesShouldHaveScopeAndSucceed()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                InputVariables = Array.Empty<string>(),
                Source = @"
function scopedFunc() {
    let x = 1;
    const y = 'my value';
    return { x: x, y: y };
}

function run() {
    var scopedReturn = scopedFunc();
    return typeof x === 'undefined' && typeof y === 'undefined' && scopedReturn.x === 1 && scopedReturn.y === 'my value';
}",
                OutputVariable = "result"
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync("result", "true", CancellationToken);
        }

        [Fact]
        public async Task ExecuteWithCustomFunctionNameAndArgumentsShouldSucceed()
        {
            // Arrange
            const string number1 = "100";
            const string number2 = "250";
            Context.GetVariableAsync(nameof(number1), CancellationToken).Returns(number1);
            Context.GetVariableAsync(nameof(number2), CancellationToken).Returns(number2);

            var settings = new ExecuteScriptV2Settings
            {
                Function = "executeFunc",
                InputVariables = new[] { nameof(number1), nameof(number2) },
                Source = @"
function executeFunc(number1, number2) {
    return parseInt(number1) + parseInt(number2);
}",
                OutputVariable = "result"
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(),
                CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync("result", "350", CancellationToken);
        }

        [Fact]
        public async Task ExecuteWithJsonReturnValueShouldSucceed()
        {
            // Arrange
            var result =
                "{\"id\":1,\"valid\":true,\"options\":[1,2,3],\"names\":[\"a\",\"b\",\"c\"],\"others\":[{\"a\":\"value1\"},{\"b\":\"value2\"}],\"content\":{\"uri\":\"https://server.com/image.jpeg\",\"type\":\"image/jpeg\"}}";
            var settings = new ExecuteScriptV2Settings
            {
                Source = @"
function run() {
    return {
        id: 1,
        valid: true,
        options: [ 1, 2, 3 ],
        names: [ 'a', 'b', 'c' ],
        others: [{ a: 'value1' }, { b: 'value2' }],                        
        content: {
            uri: 'https://server.com/image.jpeg',
            type: 'image/jpeg'
        }
    };
}
",
                OutputVariable = "result"
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(),
                CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync("result", result, CancellationToken);
        }

        [Fact]
        public async Task ExecuteWithArrayReturnValueShouldSucceed()
        {
            // Arrange
            const string result = "[1,2,3]";
            var settings = new ExecuteScriptV2Settings
            {
                Source = @"
function run() {
    return [1, 2, 3];
}
",
                OutputVariable = "result"
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(),
                CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync("result", result, CancellationToken);
        }

        [Fact]
        public async Task ExecuteWithWhileTrueShouldFail()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                Source = @"
function run() {
    var value = 0;
    while (true) {
        value++;
    }
    return value;
}",
                OutputVariable = "result"
            };
            var target = GetTarget();

            // Act
            try
            {
                await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);
                throw new Exception("The script was executed");
            }
            catch (TimeoutException ex)
            {
                ex.Message.ShouldBe("Script execution timed out");
            }
        }


        [Fact]
        public async Task ExecuteWithDefaultTimeZoneShouldWork()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                // Test date parsing and also converting to specific format and timezone
                Source =
                    "function run() { return time.parseDate('2021-01-01T00:00:00Z', {format:'yyyy-MM-ddTHH:mm:ssZ'}).toLocaleString('pt-BR', { timeZone: 'America/Sao_Paulo' }); }",
                OutputVariable = "test"
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1)
                .SetVariableAsync("test", "31/12/2020, 21:00:00", CancellationToken);
        }

        [Fact]
        public async Task ExecuteParseDateOverloads()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                // Test date parsing and also converting to specific format and timezone
                Source =
                    @"
function run() {
    return {
        'parseDate': time.parseDate('2021-01-01T19:01:01.0000001+08:00'),
        'parseDateWithFormat': time.parseDate('01/02/2021', {format:'MM/dd/yyyy'}),
        'parseDateWithFormatAndCulture': time.parseDate('01/01/2021', {format: 'MM/dd/yyyy', culture: 'pt-BR'}),
    }
}",
                OutputVariable = "test"
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1)
                .SetVariableAsync("test",
                    "{\"parseDate\":\"2021-01-01T08:01:01.0000000-03:00\",\"parseDateWithFormat\":\"2021-01-02T00:00:00.0000000-03:00\",\"parseDateWithFormatAndCulture\":\"2021-01-01T00:00:00.0000000-03:00\"}",
                    CancellationToken);
        }

        [Fact]
        public async Task ExecuteWithCustomTimeZoneShouldWork()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                // Test date parsing from GMT, with bot on Asia/Shanghai (+8 from GMT) and then converting to SP (-3 from GMT)
                Source =
                    "function run() { return time.parseDate('2021-01-01T00:00:00Z', {format:'yyyy-MM-ddTHH:mm:ssZ'}).toLocaleString('en-US', { timeZone: 'America/Sao_Paulo' }); }",
                OutputVariable = "test",
                LocalTimeZoneEnabled = true
            };
            var target = GetTarget();

            Context.Flow.Configuration["builder:#localTimeZone"] = "Asia/Shanghai";

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync("test", "12/31/2020, 9:00:00 PM",
                CancellationToken);
        }
        [Fact]
        public async Task ExecuteWithArrowFunctionOnRun()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                Source =
                    @"
async run = () => {
    return 'foo';
}
",
                OutputVariable = "test",
                LocalTimeZoneEnabled = true
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync("test", "foo", CancellationToken);
        }

        [Fact]
        public async Task ExecuteDateToStringWithCustomTimeZoneShouldWork()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                // Test date parsing and also converting to specific format and timezone
                Source =
                    "function run() { return time.dateToString(time.parseDate('2021-01-01T00:00:00Z', {'format':'yyyy-MM-ddTHH:mm:ssZ'})); }",
                OutputVariable = "test",
                LocalTimeZoneEnabled = true
            };
            var target = GetTarget();

            Context.Flow.Configuration["builder:#localTimeZone"] = "Asia/Shanghai";

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync("test", "2021-01-01T08:00:00.0000000+08:00",
                CancellationToken);
        }

        [Fact]
        public async Task ExecuteParseDateWithDefaultFormat()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                // Test date parsing and also converting to specific format and timezone
                Source =
                    @"
function run() {
    var parsed = time.parseDate('2021-01-01T19:00:00.0000000');

    var stringDate = time.dateToString(parsed);

    return {
        'parsed': parsed,
        'stringDate': stringDate
    }
}",
                OutputVariable = "test",
                LocalTimeZoneEnabled = true
            };
            var target = GetTarget();

            Context.Flow.Configuration["builder:#localTimeZone"] = "America/Sao_Paulo";

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync("test",
                "{\"parsed\":\"2021-01-01T19:00:00.0000000-03:00\",\"stringDate\":\"2021-01-01T19:00:00.0000000-03:00\"}",
                CancellationToken);
        }

        [Fact]
        public async Task ExecuteWithInfiniteSleepShouldFail()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                Source = @"
function run() {
    time.sleep(1000000000);

    return value;
}",
                OutputVariable = "result"
            };
            var target = GetTarget();

            // Act
            try
            {
                await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);
                throw new Exception("The script was executed");
            }
            catch (TimeoutException ex)
            {
                ex.Message.ShouldBe("Script execution timed out");
            }
            catch (ScriptEngineException ex)
            {
                ex.Message.ShouldBe("Error: Script execution timed out");
            }
        }

        [Fact]
        public async Task ExecuteScripWithXmlHttpRequestShouldFail()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                Source = @"
function run() {
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function() {
        if (xhr.readyState == XMLHttpRequest.DONE) {
            alert(xhr.responseText);
        }
    }
    xhr.open('GET', 'https://example.com', true);
    xhr.send(null);                    
}",
                OutputVariable = "result"
            };
            var target = GetTarget();

            // Act
            try
            {
                await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);
                throw new Exception("The script was executed");
            }
            catch (ScriptEngineException ex)
            {
                ex.Message.ShouldContain("XMLHttpRequest is not defined");
            }
        }

        [Fact]
        [SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
        public async Task ExecuteScriptWithFetchRequestShouldSucceed()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                Source = @"
async function run() {
    var response = await request.fetchAsync('https://mock.com', {
        'method': 'POST',
        'body': 'r8eht438thj9848',
        'headers': {
            'Content-Type': 'application/text',
            'test': 'test2',
            'test2': ['bla', 'bla2']
        }
    });

    return response;
}",
                OutputVariable = "result"
            };

            using var response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("{\"result\": \"bla\"}");
            response.Headers.Add("test", "test2");
            response.Headers.Add("test2", new[] { "bla", "bla2" });

            var httpClient = Substitute.For<IHttpClient>();

            HttpRequestMessage resultMessage = null;
            httpClient.SendAsync(Arg.Do<HttpRequestMessage>(message =>
                {
                    resultMessage = new HttpRequestMessage
                    {
                        Method = message.Method,
                        RequestUri = message.RequestUri,
                        Content = new StringContent(message.Content!.ReadAsStringAsync()
                                .GetAwaiter()
                                .GetResult(), Encoding.UTF8,
                            message.Content.Headers.ContentType?.MediaType!)
                    };

                    for (var i = 0; i < message.Headers.Count(); i++)
                    {
                        var header = message.Headers.ElementAt(i);
                        resultMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }), Arg.Any<CancellationToken>())
                .Returns(response);

            var target = GetTarget(httpClient);

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync("result",
                "{\"status\":200,\"success\":true,\"body\":\"{\\\"result\\\": \\\"bla\\\"}\",\"headers\":{\"test\":[\"test2\"],\"test2\":[\"bla\",\"bla2\"]}}",
                CancellationToken);

            resultMessage.Method.ShouldBe(HttpMethod.Post);
            resultMessage.RequestUri.ShouldBe(new Uri("https://mock.com"));

            var requestBody = await resultMessage.Content!.ReadAsStringAsync();

            requestBody.ShouldBe("r8eht438thj9848");

            resultMessage.Headers.GetValues("test").First().ShouldBe("test2");
            resultMessage.Headers.GetValues("test2").ShouldBe(new[] { "bla", "bla2" });

            resultMessage.Content.Headers.ContentType!.MediaType.ShouldBe("application/text");
        }

        [Fact]
        [SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
        public async Task ExecuteScriptWithRequestParseJsonResponse()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                Source = @"
async function run() {
    var response = await request.fetchAsync('https://mock.com', {
        'method': 'POST',
        'body': 'r8eht438thj9848',
        'headers': {
            'Content-Type': 'application/text',
            'test': 'test2',
            'test2': ['bla', 'bla2']
        }
    });

    return await response.jsonAsync();
}",
                OutputVariable = "result"
            };

            using var response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;
            response.Content =
                new StringContent("{\"result\": \"bla\"}", Encoding.UTF8, "application/json");
            response.Headers.Add("test", "test2");
            response.Headers.Add("test2", new[] { "bla", "bla2" });

            var httpClient = Substitute.For<IHttpClient>();

            httpClient.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(response);

            var target = GetTarget(httpClient);

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync("result",
                "{\"result\":\"bla\"}",
                CancellationToken);
        }

        [Fact]
        [SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
        public async Task ExecuteScriptWithFetchRequestWithoutOptionsShouldSucceed()
        {
            // Arrange
            var settings = new ExecuteScriptV2Settings
            {
                Source = @"
async function run() {
    var response = await request.fetchAsync('https://mock.com');

    return response;
}",
                OutputVariable = "result"
            };

            using var response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent("{\"result\": \"bla\"}");
            response.Headers.Add("test", "test2");
            response.Headers.Add("test2", new[] { "bla", "bla2" });

            var httpClient = Substitute.For<IHttpClient>();

            HttpRequestMessage resultMessage = null;
            httpClient.SendAsync(Arg.Do<HttpRequestMessage>(message =>
                {
                    resultMessage = new HttpRequestMessage
                    {
                        Method = message.Method,
                        RequestUri = message.RequestUri,
                        Content = message.Content,
                    };

                    for (var i = 0; i < message.Headers.Count(); i++)
                    {
                        var header = message.Headers.ElementAt(i);
                        resultMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }), Arg.Any<CancellationToken>())
                .Returns(response);

            var target = GetTarget(httpClient);

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync("result",
                "{\"status\":200,\"success\":true,\"body\":\"{\\\"result\\\": \\\"bla\\\"}\",\"headers\":{\"test\":[\"test2\"],\"test2\":[\"bla\",\"bla2\"]}}",
                CancellationToken);

            resultMessage.Method.ShouldBe(HttpMethod.Get);
            resultMessage.RequestUri.ShouldBe(new Uri("https://mock.com"));
            resultMessage.Content.ShouldBeNull();
        }

        [Fact]
        public async Task ExecuteScriptParseIntWithExceededLengthShouldSucceed()
        {
            // Arrange
            var result = "NaN";
            var settings = new ExecuteScriptV2Settings
            {
                Source = @"
function run() {
    let numberTest = new Array(100000).join('Z');
    let convert = parseInt(numberTest);
    return convert;
}",
                OutputVariable = "result"
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(),
                CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync("result", result, CancellationToken);
        }

        [Fact]
        public async Task ExecuteScriptJsonParseWithSpecialCharacterShouldSucceed()
        {
            // Arrange
            var invalidCharacter = "?";
            Context.GetVariableAsync(nameof(invalidCharacter), CancellationToken)
                .Returns(invalidCharacter);
            var result = "{\"value\":\"\"}";

            var settings = new ExecuteScriptV2Settings
            {
                InputVariables = new[] { nameof(invalidCharacter) },
                Source = @"
function run (input) {
    try {
        return JSON.parse(input);
    } catch (e) {
        return {
            value: ''
        };
    }
}",
                OutputVariable = "result"
            };

            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(),
                CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync("result", result, CancellationToken);
        }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class TestConfiguration : IConfiguration
        {
            public TimeSpan InputProcessingTimeout { get; set; }
            public int RedisDatabase { get; set; }
            public string RedisKeyPrefix { get; set; }
            public string InternalUris { get; set; }
            public int MaxTransitionsByInput { get; set; }
            public int TraceQueueBoundedCapacity { get; set; }
            public int TraceQueueMaxDegreeOfParallelism { get; set; }
            public TimeSpan TraceTimeout { get; set; }
            public TimeSpan DefaultActionExecutionTimeout { get; set; }
            public int ExecuteScriptLimitRecursion { get; set; }
            public int ExecuteScriptMaxStatements { get; set; }
            public long ExecuteScriptLimitMemory { get; set; }
            public long ExecuteScriptLimitMemoryWarning { get; set; }
            public TimeSpan ExecuteScriptTimeout { get; set; }
            public TimeSpan ExecuteScriptV2Timeout { get; set; }
            public int MaximumInputExpirationLoop { get; set; }
            public long ExecuteScriptV2MaxRuntimeHeapSize { get; set; }
            public long ExecuteScriptV2MaxRuntimeStackUsage { get; set; }
        }
    }
}