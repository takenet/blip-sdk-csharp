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
                if (settings.Handlebars == null)
                {
                    return;
                }
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