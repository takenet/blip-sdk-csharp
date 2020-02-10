using System;
using System.Threading.Tasks;
using Jint.Runtime;
using Microsoft.ClearScript;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Shouldly;
using Take.Blip.Builder.Actions.ExecuteScript;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ExecuteScriptActionTests : ActionTestsBase
    {
        protected virtual ExecuteScriptActionBase GetTarget()
        {
            return new DefaultExecuteScriptAction();
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
            var result = "{\"id\":1.5,\"valid\":true,\"options\":[1.1,2.1,3.1],\"names\":[\"a\",\"b\",\"c\"],\"others\":[{\"a\":\"value1\"},{\"b\":\"value2\"}],\"content\":{\"uri\":\"https://server.com/image.jpeg\",\"type\":\"image/jpeg\"}}";
            var settings = new ExecuteScriptSettings()
            {
                Source = @"
                    function run() {
                        return {
                            id: 1.5,
                            valid: true,
                            options: [ 1.1, 2.1, 3.1 ],
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
            var result = "[1.1,2.1,3.1]";
            var settings = new ExecuteScriptSettings()
            {
                Source = @"
                    function run() {
                        return [1.1, 2.1, 3.1];
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
            catch (ScriptInterruptedException ex)
            {
                ex.Message.ShouldBe("Script execution interrupted by host");
            }
            catch (StatementsCountOverflowException ex)
            {
                ex.Message.ShouldBe("The maximum number of statements executed have been reached.");
            }
        }

        [Fact]
        public async Task ExecuteWithALargeNumberOfObjectsShouldFail()
        {
            // Arrange
            var result = "";
            var settings = new ExecuteScriptSettings()
            {
                Source = @"
                    function run() {
                        const array = [];
                        for (let i = 0; i < 1 << 30; i++) {
                            array.push({
                                item: JSON.stringify(array)
                            });
                        }
                        return array;
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
            catch (ScriptEngineException ex)
            {
                ex.Message.ShouldBe("The V8 runtime has exceeded its memory limit");
            }
            catch (OutOfMemoryException ex)
            {
                ex.Message.ShouldContain(nameof(OutOfMemoryException));
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
            catch (Exception ex)
            {
                ex.Message.EndsWith("XMLHttpRequest is not defined");
            }
        }
    }
}
