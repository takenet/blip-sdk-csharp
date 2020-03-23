using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Runtime;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Serilog;
using Shouldly;
using Take.Blip.Builder.Actions.ExecuteScript;
using Take.Blip.Builder.Hosting;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ExecuteScriptActionTests : ActionTestsBase
    {
        private ExecuteScriptAction GetTarget()
        {
            return new ExecuteScriptAction(new ConventionsConfiguration(), Substitute.For<ILogger>());
        }

        [Fact]
        public async Task ExecuteWithSingleStatementScriptShouldSucceed()
        {
            // Arrange
            var variableName = "variable1";
            var variableValue = "my variable 1 value";
            var settings = new ExecuteScriptSettings()
            {
                Source = $"function run() {{ return '{variableValue}'; }}",
                OutputVariable = variableName
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync(variableName, variableValue, CancellationToken, default(TimeSpan));
        }

        [Fact]
        public async Task ExecuteWithArgumentsShouldSucceed()
        {
            // Arrange
            var number1 = "100";
            var number2 = "250";
            Context.GetVariableAsync(nameof(number1), CancellationToken).Returns(number1);
            Context.GetVariableAsync(nameof(number2), CancellationToken).Returns(number2);
            var result = "";
            
            var settings = new ExecuteScriptSettings()
            {
                InputVariables = new[]
                {
                    nameof(number1),
                    nameof(number2)

                },
                Source = @"
                    function run(number1, number2) {
                        return parseInt(number1) + parseInt(number2);
                    }",
                OutputVariable = nameof(result)
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync(nameof(result), "350", CancellationToken, default(TimeSpan));
        }

        [Fact]
        public async Task ExecuteWithMissingArgumentsShouldSucceed()
        {
            // Arrange
            var number1 = "100";
            var number2 = "250";
            Context.GetVariableAsync(nameof(number1), CancellationToken).Returns(number1);
            Context.GetVariableAsync(nameof(number2), CancellationToken).Returns(number2);
            var result = "";

            var settings = new ExecuteScriptSettings()
            {
                InputVariables = new[]
                {
                    nameof(number1),
                    nameof(number2)

                },
                Source = @"
                    function run(number1, number2, number3) {
                        return parseInt(number1) + parseInt(number2) + (number3 || 150);
                    }",
                OutputVariable = nameof(result)
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync(nameof(result), "500", CancellationToken, default(TimeSpan));
        }

        [Fact]
        public async Task ExecuteUsingLetAndConstVariablesShouldHaveScopeAndSucceed()
        {
            // Arrange
            var result = string.Empty;

            var settings = new ExecuteScriptSettings()
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
                OutputVariable = nameof(result)
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync(nameof(result), bool.TrueString.ToLowerInvariant(), CancellationToken);
        }

        [Fact]
        public async Task ExecuteWithCustomFunctionNameAndArgumentsShouldSucceed()
        {
            // Arrange
            var number1 = "100";
            var number2 = "250";
            Context.GetVariableAsync(nameof(number1), CancellationToken).Returns(number1);
            Context.GetVariableAsync(nameof(number2), CancellationToken).Returns(number2);
            var result = "";

            var settings = new ExecuteScriptSettings()
            {
                Function = "executeFunc",
                InputVariables = new[]
                {
                    nameof(number1),
                    nameof(number2)

                },
                Source = @"
                    function executeFunc(number1, number2) {
                        return parseInt(number1) + parseInt(number2);
                    }",
                OutputVariable = nameof(result)
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync(nameof(result), "350", CancellationToken, default(TimeSpan));
        }

        [Fact]
        public async Task ExecuteWithJsonReturnValueShouldSucceed()
        {
            // Arrange
            var result = "{\"id\":1.0,\"valid\":true,\"options\":[1.0,2.0,3.0],\"names\":[\"a\",\"b\",\"c\"],\"others\":[{\"a\":\"value1\"},{\"b\":\"value2\"}],\"content\":{\"uri\":\"https://server.com/image.jpeg\",\"type\":\"image/jpeg\"}}";
            var settings = new ExecuteScriptSettings()
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
                OutputVariable = nameof(result)
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync(nameof(result), result, CancellationToken, default(TimeSpan));
        }

        [Fact]
        public async Task ExecuteWithArrayReturnValueShouldSucceed()
        {
            // Arrange
            var result = "[1.0,2.0,3.0]";
            var settings = new ExecuteScriptSettings()
            {
                Source = @"
                    function run() {
                        return [1, 2, 3];
                    }
                    ",
                OutputVariable = nameof(result)
            };
            var target = GetTarget();

            // Act
            await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync(nameof(result), result, CancellationToken, default(TimeSpan));
        }

        [Fact]
        public async Task ExecuteWithWhileTrueShouldFail()
        {
            // Arrange            
            var result = "";
            var settings = new ExecuteScriptSettings()
            {
                Source = @"
                    function run() {
                        var value = 0;
                        while (true) {
                            value++;
                        }
                        return value;
                    }
                    ",
                OutputVariable = nameof(result)
            };
            var target = GetTarget();

            // Act            
            try
            {
                await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);
                throw new Exception("The script was executed");
            }
            catch (StatementsCountOverflowException ex)
            {
                ex.Message.ShouldBe("The maximum number of statements executed have been reached.");
            }
        }

        [Fact]
        public async Task ExecuteScripWithXmlHttpRequestShouldFail()
        {
            // Arrange
            var settings = new ExecuteScriptSettings()
            {
                Source = @"
                    function run() {
                        var xhr = new XMLHttpRequest();
                        xhr.onreadystatechange = function() {
                            if (xhr.readyState == XMLHttpRequest.DONE) {
                                alert(xhr.responseText);
                            }
                        }
                        xhr.open('GET', 'http://example.com', true);
                        xhr.send(null);                    
                    }
                    ",
                OutputVariable = "result"
            };
            var target = GetTarget();

            // Act
            try
            {
                await target.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);
                throw new Exception("The script was executed");
            }
            catch (JavaScriptException ex)
            {
                ex.Message.ShouldBe("XMLHttpRequest is not defined");
            }
        }
    }
}
