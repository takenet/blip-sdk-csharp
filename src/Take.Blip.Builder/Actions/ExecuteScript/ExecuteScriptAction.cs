using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Debugger;
using Newtonsoft.Json;
using Take.Blip.Builder.Hosting;

namespace Take.Blip.Builder.Actions.ExecuteScript
{
    public class ExecuteScriptAction : ActionBase<ExecuteScriptSettings>
    {
        private const string DEFAULT_FUNCTION = "run";
        private readonly IConfiguration _configuration;

        public ExecuteScriptAction(IConfiguration configuration) 
            : base(nameof(ExecuteScript))
        {
            _configuration = configuration;
        }

        public override async Task ExecuteAsync(IContext context, ExecuteScriptSettings settings, CancellationToken cancellationToken)
        {
            var arguments = await GetScriptArgumentsAsync(context, settings, cancellationToken);

            var engine = new Engine(options => options
                    .LimitRecursion(_configuration.ExecuteScriptLimitRecursion)
                    .MaxStatements(_configuration.ExecuteScriptMaxStatements)
                    .LimitMemory(_configuration.ExecuteScriptLimitMemory)
                    .TimeoutInterval(_configuration.ExecuteScriptTimeout));

            engine.Step += (sender, e) =>
            {
                CheckMemoryUsage(context, e);
                return StepMode.Into;
            };

            engine = engine.Execute(settings.Source);

            var result = arguments != null
                ? engine.Invoke(settings.Function ?? DEFAULT_FUNCTION, arguments)
                : engine.Invoke(settings.Function ?? DEFAULT_FUNCTION);

            await SetScriptResultAsync(context, settings, result, cancellationToken);
        }

        protected async Task<object[]> GetScriptArgumentsAsync(
            IContext context, ExecuteScriptSettings settings, CancellationToken cancellationToken)
        {
            object[] arguments = null;
            if (settings.InputVariables != null && settings.InputVariables.Length > 0)
            {
                arguments = new object[settings.InputVariables.Length];
                for (int i = 0; i < arguments.Length; i++)
                {
                    arguments[i] =
                        await context.GetVariableAsync(settings.InputVariables[i], cancellationToken);
                }
            }

            return arguments;
        }

        private async Task SetScriptResultAsync(
            IContext context, ExecuteScriptSettings settings, JsValue result, CancellationToken cancellationToken)
        {
            if (result != null && !result.IsNull())
            {
                var value = result.Type == Types.Object
                    ? JsonConvert.SerializeObject(result.ToObject())
                    : result.ToString();

                await context.SetVariableAsync(settings.OutputVariable, value, cancellationToken);
            }
            else
            {
                await context.DeleteVariableAsync(settings.OutputVariable, cancellationToken);
            }
        }

        private void CheckMemoryUsage(IContext context, DebugInformation debugInformation)
        {
            if (debugInformation.CurrentMemoryUsage >= _configuration.ExecuteScriptLimitMemoryWarning)
            {
                var currentActionTrace = context.GetCurrentActionTrace();

                if (currentActionTrace != null)
                {
                    currentActionTrace.Warning =
                        $"The script memory usage is above the warning threshold of {_configuration.ExecuteScriptLimitMemoryWarning} bytes({debugInformation.CurrentMemoryUsage})";
                }
            }
        }
    }
}
