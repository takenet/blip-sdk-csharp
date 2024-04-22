using System;
using System.Threading.Tasks;
using NSubstitute;
using Serilog;
using Take.Blip.Builder.Actions.ExecuteTemplate;
using Take.Blip.Builder.Hosting;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ExecuteTemplateActionTests : ActionTestsBase
    {
        private ExecuteTemplateAction GetTarget()
        {
            return new ExecuteTemplateAction(new ConventionsConfiguration(), Substitute.For<ILogger>());
        }

        [Fact]
        public async Task ExecuteTemplateShouldSuccess()
        {
            var variableName = "TestName";
            var outputVariable = "";
            Context.GetVariableAsync(nameof(variableName), CancellationToken).Returns(variableName);
            var settings = new ExecuteTemplateSettings
            {
                InputVariables = new []{ nameof(variableName) },
                Template = $"Name: {{{{{nameof(variableName)}}}}}",
                OutputVariable = outputVariable
            };
            var action = GetTarget();
            await action.ExecuteAsync(Context, settings, CancellationToken);
            
            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync(outputVariable, $"Name: {variableName}", CancellationToken);
        }
    }
}