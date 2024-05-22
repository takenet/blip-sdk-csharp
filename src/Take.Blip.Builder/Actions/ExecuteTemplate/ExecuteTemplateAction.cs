using System;
using System.Threading;
using System.Threading.Tasks;
using Esprima;
using HandlebarsDotNet;
using Lime.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Take.Blip.Builder.Utils;
using Takenet.Iris.Messaging;

namespace Take.Blip.Builder.Actions.ExecuteTemplate
{
    public class ExecuteTemplateAction : ActionBase<ExecuteTemplateSettings>
    {
        private readonly ILogger _logger;
        private readonly IHandlebars _handlebars;
        
        public ExecuteTemplateAction(IHandlebars handlebars, ILogger logger) : base(nameof(ExecuteTemplate))
        {
            _logger = logger;
            _handlebars = handlebars;
        }

        public override async Task ExecuteAsync(IContext context, ExecuteTemplateSettings settings, CancellationToken cancellationToken)
        {
            string result;
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
                    _logger.Warning(ex, "Unexpected error while trying to parse Handlebars template");
                    throw;
                }

                _logger.Warning(ex, "Unexpected error while execute action Execute Template");
                throw;
            }
            
            await SetScriptResultAsync(context, settings, result, cancellationToken);
        }

        private async Task<JObject> GetScriptArgumentsAsync(
            IContext context, ExecuteTemplateSettings settings, CancellationToken cancellationToken)
        {
            var obj = new JObject();

            if (settings.InputVariables.IsNullOrEmpty())
            {
                return obj;
            }

            foreach (var variable in settings.InputVariables)
            {
                var variableValue = await context.GetVariableAsync(variable, cancellationToken);
                obj[variable] = variableValue;
            }
            return obj;
        }
        
        private async Task SetScriptResultAsync(
            IContext context, ExecuteTemplateSettings settings, string result, CancellationToken cancellationToken)
        {
            if (result.IsNullOrEmpty())
            {
                await context.DeleteVariableAsync(settings.OutputVariable, cancellationToken);
            }
            else
            {
                await context.SetVariableAsync(settings.OutputVariable, result, cancellationToken);
            }
        }
        
        private JObject CopyArgumentsToObject(JObject arguments)
        {
            var obj = new JObject();
            foreach (var property in arguments.Properties())
            {
                try
                {
                    var deserializedJson = JsonConvert.DeserializeObject<JObject>(property.Value.ToString(), JsonSerializerSettingsContainer.Settings);
                    if (deserializedJson != null)
                    {
                        obj.Merge(deserializedJson);
                    }
                } catch (Exception ex)
                {
                    if (ex is JsonReaderException)
                    {
                        obj[property.Name] = property.Value;
                    }
                    else if (ex is InvalidCastException)
                    {
                        var jToken = JsonConvert.DeserializeObject<JToken>(property.Value.ToString(), JsonSerializerSettingsContainer.Settings);
                        if (jToken != null)
                        {
                            obj.Add(property.Name, jToken);
                        }
                    }
                }                
            }

            return obj;
        }
    }
}