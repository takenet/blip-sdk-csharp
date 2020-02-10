using System;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Runtime;
using Newtonsoft.Json;

namespace Take.Blip.Builder.Actions.ExecuteScript
{
    public class DefaultExecuteScriptAction : ExecuteScriptActionBase
    {
        public override async Task ExecuteAsync(IContext context, ExecuteScriptSettings settings, CancellationToken cancellationToken)
        {
            var arguments = await GetScriptArgumentsAsync(context, settings, cancellationToken);

            var engine = new Engine(options => options
                    .LimitRecursion(50)
                    .MaxStatements(1000)
                    .TimeoutInterval(TimeoutInterval))
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
