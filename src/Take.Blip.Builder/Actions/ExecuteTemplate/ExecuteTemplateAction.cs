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
            configuration = _configuration;
            logger = _logger;
            _handlebars = Handlebars.Create();
            _handlebars.Configuration.UseJson();
        }

        public override async Task ExecuteAsync(IContext context, ExecuteTemplateSettings settings, CancellationToken cancellationToken)
        { 
            var jObject = await GetScriptArgumentsAsync(context, settings, cancellationToken);
            var template = _handlebars.Compile(settings.Template);
            if (template == null)
            {
                return;
            }
            var result = template(jObject);
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
    }
}