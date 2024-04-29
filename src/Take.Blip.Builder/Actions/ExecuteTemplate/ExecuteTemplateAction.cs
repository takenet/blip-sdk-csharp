using System;
using System.Threading;
using System.Threading.Tasks;
using HandlebarsDotNet;
using HandlebarsDotNet.Extension.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Take.Blip.Builder.Hosting;

namespace Take.Blip.Builder.Actions.ExecuteTemplate
{
    public class ExecuteTemplateAction : ActionBase<ExecuteTemplateSettings>
    {
        private readonly ILogger _logger;
        private readonly IHandlebars _handlebars;
        
        public ExecuteTemplateAction(IHandlebars handlebars, ILogger logger) : base(nameof(ExecuteTemplateAction))
        {
            _logger = logger;
            _handlebars = handlebars;
        }

        public override async Task ExecuteAsync(IContext context, ExecuteTemplateSettings settings, CancellationToken cancellationToken)
        {
            var result = string.Empty;
            try 
            {
                var arguments = await GetScriptArgumentsAsync(context, settings, cancellationToken);
                var obj = CopyArgumentsToObject(arguments);
                var template = _handlebars.Compile(settings.Template);
                result = template(obj);
            }
            catch (Exception ex)
            {
                if (ex is HandlebarsParserException)
                {
                    _logger.Error(ex, "Unexpected error while trying to parse Handlebars template");
                    throw;
                }

                _logger.Error(ex, "Unexpected error while execute action Execute Template");
                throw;
            }
            
            await SetScriptResultAsync(context, settings, result, cancellationToken);
        }

        private async Task<JObject> GetScriptArgumentsAsync(
            IContext context, ExecuteTemplateSettings settings, CancellationToken cancellationToken)
        {
            var obj = new JObject();
            if (settings.InputVariables != null && settings.InputVariables.Length > 0)
            {
                for (int i = 0; i < settings.InputVariables.Length; i++)
                {
                    var variableValue = await context.GetVariableAsync(settings.InputVariables[i], cancellationToken);
                    obj[settings.InputVariables[i]] = variableValue;
                }
            }
            return obj;
        }
        
        private async Task SetScriptResultAsync(
            IContext context, ExecuteTemplateSettings settings, string result, CancellationToken cancellationToken)
        {
            if (result != null)
            {
                await context.SetVariableAsync(settings.OutputVariable, result, cancellationToken);
            }
            else
            {
                await context.DeleteVariableAsync(settings.OutputVariable, cancellationToken);
            }
        }
        
        private JObject CopyArgumentsToObject(JObject arguments)
        {
            var obj = new JObject();
            foreach (var property in arguments.Properties())
            {
                var success = TryParseJson<JObject>(property.Value.ToString(), out var jObject);
                if (success)
                {
                    foreach (var item in jObject)
                    {
                        obj[item.Key] = item.Value;
                    }
                }
                else
                {
                    obj[property.Name] = property.Value;
                }
            }

            return obj;
        }
        
        private bool TryParseJson<T>(string json, out T result)
        {
            bool success = true;
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) => { success = false; args.ErrorContext.Handled = true; },
                MissingMemberHandling = MissingMemberHandling.Error
            };
            result = JsonConvert.DeserializeObject<T>(json, settings);
            return success;
        }
    }
}