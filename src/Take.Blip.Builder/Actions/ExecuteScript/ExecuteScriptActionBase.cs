using System;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Actions.ExecuteScript
{
    public abstract class ExecuteScriptActionBase : ActionBase<ExecuteScriptSettings>
    {
        protected const string DEFAULT_FUNCTION = "run";

        protected static readonly TimeSpan TimeoutInterval = TimeSpan.FromSeconds(5);
        protected static readonly uint MemoryLimit = 1 << 16;

        protected ExecuteScriptActionBase() : base(nameof(ExecuteScript))
        {
        }

        protected ExecuteScriptActionBase(string version) : base($"{nameof(ExecuteScript)}:{version}")
        {
        }

        protected virtual async Task<object[]> GetScriptArgumentsAsync(
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
    }
}
