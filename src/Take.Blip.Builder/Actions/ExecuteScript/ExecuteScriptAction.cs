using System;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Actions.ExecuteScript
{
    public class ExecuteScriptAction : ActionBase<ExecuteScriptSettings>
    {
        private const string DEFAULT_FUNCTION = "run";

        public ExecuteScriptAction() 
            : base(nameof(ExecuteScript))
        {
        }

        public override async Task ExecuteAsync(IContext context, ExecuteScriptSettings settings, CancellationToken cancellationToken)
        {
            // Retrive the input variables
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

            var engine = new Engine(options => options
                    .LimitRecursion(5)
                    .MaxStatements(50)
                    .TimeoutInterval(TimeSpan.FromSeconds(2)))
                .Execute(settings.Source);

            var result = arguments != null
                ? engine.Invoke(settings.Function ?? DEFAULT_FUNCTION, arguments)
                : engine.Invoke(settings.Function ?? DEFAULT_FUNCTION);

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
    }
}
