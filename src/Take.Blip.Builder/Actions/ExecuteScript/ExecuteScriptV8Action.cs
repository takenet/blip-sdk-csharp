using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ClearScript.V8;
using Newtonsoft.Json;

namespace Take.Blip.Builder.Actions.ExecuteScript
{
    public class ExecuteScriptV8Action : ExecuteScriptActionBase
    {
        private const string VERSION = "V8";

        public ExecuteScriptV8Action() : base(VERSION)
        {
        }

        public override async Task ExecuteAsync(IContext context, ExecuteScriptSettings settings, CancellationToken cancellationToken)
        {
            var arguments = await GetScriptArgumentsAsync(context, settings, cancellationToken);

            using var engine = new V8ScriptEngine();
            engine.MaxRuntimeStackUsage = new UIntPtr(1 << 16);
            engine.MaxRuntimeHeapSize = new UIntPtr(1 << 24);

            object result;
            using (var timer = new Timer(_ => engine.Interrupt(), null, TimeoutInterval, TimeSpan.Zero))
            {
                engine.Execute(settings.Source);

                result = arguments != null
                    ? engine.Invoke(settings.Function ?? DEFAULT_FUNCTION, arguments)
                    : engine.Invoke(settings.Function ?? DEFAULT_FUNCTION);
            }

            if (result != null)
            {
                var value = result as string ?? JsonConvert.SerializeObject(result);
                await context.SetVariableAsync(settings.OutputVariable, value, cancellationToken);
            }
            else
            {
                await context.DeleteVariableAsync(settings.OutputVariable, cancellationToken);
            }
        }
    }
}
