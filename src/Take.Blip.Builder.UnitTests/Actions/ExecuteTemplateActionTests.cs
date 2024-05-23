using System;
using System.Threading.Tasks;
using HandlebarsDotNet;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Serilog;
using Take.Blip.Builder.Actions.ExecuteTemplate;
using Xunit;
using Shouldly;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ExecuteTemplateActionTests : ActionTestsBase
    {
        private IHandlebars Handlebars = Substitute.For<IHandlebars>();

        private ExecuteTemplateAction GetTarget()
        {
            return new ExecuteTemplateAction(Handlebars,Substitute.For<ILogger>());
        }

        [Fact]
        public async Task ExecuteTemplateShouldSuccess()
        {
            //Arrange
            var variableName = "TestName";
            var outputVariable = "";
            Context.GetVariableAsync(nameof(variableName), CancellationToken).Returns(variableName);     
            
            var templateResult = "TemplateResult";
            var handlebarsTemplate = Substitute.For<HandlebarsTemplate<object, object>>();
            handlebarsTemplate(Arg.Any<object>()).Returns("TemplateResult");
            Handlebars.Compile(Arg.Any<string>()).Returns(handlebarsTemplate);
            
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
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Is(templateResult), CancellationToken, Arg.Any<TimeSpan>());
        }
        
        [Fact]
        public async Task ExecuteTemplateWithObjectAsPropertyShouldSuccess()
        {
            //Arrange
            var variableObj = "{ \"people\": [{\"name\": \"TestName\", \"city\": \"Aracaju\"}, {\"name\": \"TestName2\", \"city\": \"Bahia\"}] }";
            var outputVariable = "";
            Context.GetVariableAsync(nameof(variableObj), CancellationToken).Returns(variableObj);
            
            var templateResult = "TemplateResult";
            var handlebarsTemplate = Substitute.For<HandlebarsTemplate<object, object>>();
            handlebarsTemplate(Arg.Any<object>()).Returns("TemplateResult");
            Handlebars.Compile(Arg.Any<string>()).Returns(handlebarsTemplate);
            
            var settings = new ExecuteTemplateSettings
            {
                InputVariables = new []{ nameof(variableObj) },
                Template = "Names: {{#each people}}{{name}} living in {{city}} {{/each}}",
                OutputVariable = outputVariable
            };
            
            //Act
            var action = GetTarget();
            await action.ExecuteAsync(Context, settings, CancellationToken);
            
            // Assert
            Handlebars.Received(1).Compile(settings.Template);
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Is(templateResult), CancellationToken, Arg.Any<TimeSpan>());
        }
        
        [Fact]
        public async Task ExecuteTemplateWithObjectAndStringVariablesAsPropertyShouldSuccess()
        {
            //Arrange
            var variableName = "Peoples:";
            var variableObj = "{ \"people\": [{\"name\": \"TestName\", \"city\": \"London\"}, {\"name\": \"TestName2\", \"city\": \"Manchester\"}] }";
            var arrayVariable = "[{\"city\": \"London\", \"street\": \"Baker Street\"},{\"city\": \"Manchester\", \"street\": \"Oxford Road\"}]";
            var outputVariable = "";
            Context.GetVariableAsync(nameof(variableName), CancellationToken).Returns(variableName);
            Context.GetVariableAsync(nameof(variableObj), CancellationToken).Returns(variableObj);
            Context.GetVariableAsync(nameof(arrayVariable), CancellationToken).Returns(arrayVariable);
            
            var templateResult = "TemplateResult";
            var handlebarsTemplate = Substitute.For<HandlebarsTemplate<object, object>>();
            handlebarsTemplate(Arg.Any<object>()).Returns("TemplateResult");
            Handlebars.Compile(Arg.Any<string>()).Returns(handlebarsTemplate);
            var settings = new ExecuteTemplateSettings
            {
                InputVariables = new []{ nameof(variableName), nameof(variableObj), nameof(arrayVariable) },
                Template = $"{{{{{nameof(variableName)}}}}} {{{{#each people}}}}{{{{name}}}} living in {{{{city}}}} {{{{/each}}}} {{{{{nameof(arrayVariable)}}}}}",
                OutputVariable = outputVariable
            };
            
            //Act
            var action = GetTarget();
            await action.ExecuteAsync(Context, settings, CancellationToken);
            
            // Assert
            Handlebars.Received(1).Compile(settings.Template);
            await Context.Received(1).SetVariableAsync(Arg.Any<string>(), Arg.Is(templateResult), CancellationToken, Arg.Any<TimeSpan>());
        }
        
        [Fact]
        public async Task ExecuteTemplateErrorHandlebarsParseShouldFail()
        {
            //Arrange
            var variableName = "TestName";
            var outputVariable = "";
            Context.GetVariableAsync(nameof(variableName), CancellationToken).Returns(variableName);
            Handlebars.Compile("").ThrowsForAnyArgs(new HandlebarsParserException("could not be converted to an expression"));
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
            }
            catch (HandlebarsParserException ex)
            {
                ex.Message.ShouldContain("could not be converted to an expression");
            }
            
            Handlebars.Received(1).Compile(settings.Template);
            await Context.Received(0).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
        }
        
        [Fact]
        public async Task ExecuteTemplateErrorHandlebarsExecutionShouldFail()
        {
            //Arrange
            var variableName = "TestName";
            var outputVariable = "";
            Context.GetVariableAsync(nameof(variableName), CancellationToken).Returns(variableName);
            Handlebars.Compile("").ThrowsForAnyArgs(new Exception("Error executing the template"));
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
            }
            catch (Exception ex)
            {
                ex.Message.ShouldContain("Error executing the template");
            }
            
            Handlebars.Received(1).Compile(settings.Template);
            await Context.Received(0).SetVariableAsync(Arg.Any<string>(), Arg.Any<string>(), CancellationToken, Arg.Any<TimeSpan>());
        }
    }
}