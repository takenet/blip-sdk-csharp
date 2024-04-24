using System;
using System.Threading.Tasks;
using HandlebarsDotNet;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Serilog;
using Take.Blip.Builder.Actions.ExecuteTemplate;
using Take.Blip.Builder.Hosting;
using Xunit;
using Shouldly;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ExecuteTemplateActionTests : ActionTestsBase
    {
        private IHandlebars Handlebars = Substitute.For<IHandlebars>();

        private ExecuteTemplateAction GetTarget()
        {
            return new ExecuteTemplateAction(Handlebars, new ConventionsConfiguration(), Substitute.For<ILogger>());
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
                OutputVariable = outputVariable
            };
            
            //Act
            var action = GetTarget();
            await action.ExecuteAsync(Context, settings, CancellationToken);
            
            // Assert
            Handlebars.Received(1).Compile(settings.Template);
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
        }
        
        [Fact]
        public async Task ExecuteTemplateWithObjectAsPropertyShouldSuccess()
        {
            //Arrange
            var variableObj = "{ \"people\": [{\"name\": \"TestName\", \"city\": \"Aracaju\"}] }";
            var outputVariable = "";
            Context.GetVariableAsync(nameof(variableObj), CancellationToken).Returns(variableObj);
            var settings = new ExecuteTemplateSettings
            {
                InputVariables = new []{ nameof(variableObj) },
                Template = "Names: {{#each people}}{{name}} living in {{city}}{{/each}}",
                OutputVariable = outputVariable
            };
            
            //Act
            var action = GetTarget();
            await action.ExecuteAsync(Context, settings, CancellationToken);
            
            // Assert
            Handlebars.Received(1).Compile(settings.Template);
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
        }
        
        [Fact]
        public async Task ExecuteTemplateWithObjectAndStringVariablesAsPropertyShouldSuccess()
        {
            //Arrange
            var variableName = "Peoples:";
            var variableObj = "{ \"people\": [{\"name\": \"Carlos\", \"city\": \"Aracaju\"}] }";
            var outputVariable = "";
            Context.GetVariableAsync(nameof(variableName), CancellationToken).Returns(variableName);
            Context.GetVariableAsync(nameof(variableObj), CancellationToken).Returns(variableObj);
            var settings = new ExecuteTemplateSettings
            {
                InputVariables = new []{ nameof(variableName), nameof(variableObj) },
                Template = $"{{{{{nameof(variableName)}}}}} {{{{#each people}}}}{{{{name}}}} living in {{{{city}}}}{{{{/each}}}}",
                OutputVariable = outputVariable
            };
            
            //Act
            var action = GetTarget();
            await action.ExecuteAsync(Context, settings, CancellationToken);
            
            // Assert
            Handlebars.Received(1).Compile(settings.Template);
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
        }
        
        [Fact]
        public async Task ExecuteTemplateShouldFail()
        {
            //Arrange
            var variableName = "TestName";
            var outputVariable = "";
            Context.GetVariableAsync(nameof(variableName), CancellationToken).Returns(variableName);
            Handlebars.Compile("").ThrowsForAnyArgs(new HandlebarsCompilerException("could not be converted to an expression"));
            var settings = new ExecuteTemplateSettings
            {
                InputVariables = new []{ nameof(variableName) },
                Template = $"Name: {{{{nameof(variableName)}}}}",
                OutputVariable = outputVariable
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
            
            Handlebars.Received(1).Compile(settings.Template);
            await Context.Received(0).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
        }
    }
}