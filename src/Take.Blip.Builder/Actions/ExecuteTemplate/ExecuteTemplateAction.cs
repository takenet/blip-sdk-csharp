using System;
using System.Threading;
using System.Threading.Tasks;
using HandlebarsDotNet;
using HandlebarsDotNet.Extension.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Take.Blip.Builder.Hosting;

namespace Take.Blip.Builder.Actions.ExecuteTemplate
{
    public class ExecuteTemplateAction : ActionBase<ExecuteTemplateSettings>
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IHandlebars _handlebars;
        
        public ExecuteTemplateAction(IConfiguration configuration, ILogger logger) : base(nameof(ExecuteTemplateAction))
        {
            _configuration = configuration;
            _logger = logger;
        }

        public override async Task ExecuteAsync(IContext context, ExecuteTemplateSettings settings, CancellationToken cancellationToken)
        {
            string result;
            var obj = new JObject();
            var arguments = await GetScriptArgumentsAsync(context, settings, cancellationToken);
            try
            {
                if (settings?.Handlebars == null)
                {
                    return;
                }
                foreach (var property in arguments.Properties())
                {
                    if (IsJsonFormat(property.Value.ToString()))
                    {
                        var data = JObject.Parse(property.Value.ToString());
                        foreach (var item in data)
                        {
                            obj[item.Key] = item.Value;
                        }
                    }
                    else
                    {
                        obj = arguments;
                    }
                }
                settings.Handlebars.Configuration.UseJson();
                var template = settings.Handlebars.Compile(settings.Template);
                result = template(obj);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error while trying to execute Handlebars template");
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
            if (!string.IsNullOrEmpty(result))
            {
                await context.SetVariableAsync(settings.OutputVariable, result, cancellationToken);
            }
            else
            {
                await context.DeleteVariableAsync(settings.OutputVariable, cancellationToken);
            }
        }
        
        private bool IsJsonFormat(string strInput)
        {
            strInput = strInput.Trim();
            return (strInput.StartsWith("{") && strInput.EndsWith("}")) ||
                   (strInput.StartsWith("[") && strInput.EndsWith("]"));
        }
    }
}