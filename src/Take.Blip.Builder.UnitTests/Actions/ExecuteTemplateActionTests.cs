using System;
using System.Threading.Tasks;
using HandlebarsDotNet;
using NSubstitute;
using Serilog;
using Take.Blip.Builder.Actions.ExecuteTemplate;
using Take.Blip.Builder.Hosting;
using Xunit;
using Shouldly;

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
            //Arrange
            var variableName = "TestName";
            var outputVariable = "";
            Context.GetVariableAsync(nameof(variableName), CancellationToken).Returns(variableName);
            var settings = new ExecuteTemplateSettings
            {
                InputVariables = new []{ nameof(variableName) },
                Template = $"Name: {{{{{nameof(variableName)}}}}}",
                OutputVariable = outputVariable,
                Handlebars = Handlebars.Create()
            };
            
            //Act
            var action = GetTarget();
            await action.ExecuteAsync(Context, settings, CancellationToken);
            
            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync(outputVariable, $"Name: {variableName}", CancellationToken);
        }
        
        [Fact]
        public async Task ExecuteTemplateWithObjectAsPropertyShouldSuccess()
        {
            //Arrange
            var variableName = "{ \"people\": [{\"name\": \"testName\", \"city\": \"Aracaju\"}] }";
            var outputVariable = "";
            Context.GetVariableAsync(nameof(variableName), CancellationToken).Returns(variableName);
            var settings = new ExecuteTemplateSettings
            {
                InputVariables = new []{ nameof(variableName) },
                Template = "Names: {{#each people}}{{name}} living in {{city}}{{/each}}",
                OutputVariable = outputVariable,
                Handlebars = Handlebars.Create()
            };
            
            //Act
            var action = GetTarget();
            await action.ExecuteAsync(Context, settings, CancellationToken);
            
            // Assert
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
            await Context.Received(1).SetVariableAsync(outputVariable, "Names: testName living in Aracaju", CancellationToken);
        }
        
        
        [Fact]
        public async Task ExecuteTemplateShouldReturnWithoutExecution()
        {
            //Arrange
            var variableName = "TestName";
            var outputVariable = "";
            Context.GetVariableAsync(nameof(variableName), CancellationToken).Returns(variableName);
            var settings = new ExecuteTemplateSettings
            {
                InputVariables = new []{ nameof(variableName) },
                Template = $"Name: {{{{{nameof(variableName)}}}}}",
                OutputVariable = outputVariable,
            };
            
            //Act
            var action = GetTarget();
            await action.ExecuteAsync(Context, settings, CancellationToken);
            
            // Assert
            await Context.Received(0).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
        }
        
        [Fact]
        public async Task ExecuteTemplateShouldFail()
        {
            //Arrange
            var variableName = "TestName";
            var outputVariable = "";
            Context.GetVariableAsync(nameof(variableName), CancellationToken).Returns(variableName);
            var settings = new ExecuteTemplateSettings
            {
                InputVariables = new []{ nameof(variableName) },
                Template = $"Name: {{{{nameof(variableName)}}}}",
                OutputVariable = outputVariable,
                Handlebars = Handlebars.Create()
            };
            
            //Act
            var action = GetTarget();
            try
            {
                await action.ExecuteAsync(Context, settings, CancellationToken);
                throw new Exception("The template was executed");
            }
            catch (HandlebarsCompilerException ex)
            {
                ex.Message.ShouldContain("could not be converted to an expression");
            }
        }
    }
}