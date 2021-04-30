using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Debugger;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;
using Take.Blip.Builder.Hosting;
using System;
using TimeZoneConverter;

namespace Take.Blip.Builder.Actions.ExecuteScript
{
    public class ExecuteScriptAction : ActionBase<ExecuteScriptSettings>
    {
        private const string DEFAULT_FUNCTION = "run";
        private const string BRAZIL_TIMEZONE = "E. South America Standard Time";
        private const string LOCAL_TIMEZONE_SEPARATOR = "builder:#localTimeZone";

        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public ExecuteScriptAction(IConfiguration configuration, ILogger logger) 
            : base(nameof(ExecuteScript))
        {
            _configuration = configuration;
            _logger = logger;
        }

        public override async Task ExecuteAsync(IContext context, ExecuteScriptSettings settings, CancellationToken cancellationToken)
        {
            var arguments = await GetScriptArgumentsAsync(context, settings, cancellationToken);
            var timeZoneLocal = TZConvert.GetTimeZoneInfo(BRAZIL_TIMEZONE);

            try
            {
                if (context.Flow.Configuration.ContainsKey(LOCAL_TIMEZONE_SEPARATOR))
                {
                    timeZoneLocal = TZConvert.GetTimeZoneInfo(context.Flow.Configuration[LOCAL_TIMEZONE_SEPARATOR]);
                } 
            }
            catch (Exception e) 
            {
                _logger.Information(e.Message);
            }

            var engine = new Engine(options =>
            {
                options
                    .LimitRecursion(_configuration.ExecuteScriptLimitRecursion)
                    .MaxStatements(_configuration.ExecuteScriptMaxStatements)
                    .LimitMemory(_configuration.ExecuteScriptLimitMemory)
                    .TimeoutInterval(_configuration.ExecuteScriptTimeout)
                    .DebugMode();

                if (settings.LocalTimeZoneEnabled)
                {
                    options.LocalTimeZone(timeZoneLocal);
                }
            });

            var currentActionTrace = context.GetCurrentActionTrace();
            engine.SetValue("debug_warning", new Action<string>(warningMessage =>
            {
                if (currentActionTrace != null)
                {
                    if (currentActionTrace.Warning == null)
                    {
                        currentActionTrace.Warning = string.Empty;
                    }
                    else
                    {
                        currentActionTrace.Warning += '\n';
                    }

                    currentActionTrace.Warning += warningMessage;
                }
            }));

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
            if (result?.IsNull() == false)
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
                var warningMessage =
                    $"The script memory allocation ({debugInformation.CurrentMemoryUsage:N0} bytes) is above the warning threshold of {_configuration.ExecuteScriptLimitMemoryWarning:N0} bytes";
                
                using (LogContext.PushProperty(nameof(DebugInformation), debugInformation, true))
                    _logger.Warning(warningMessage);

                var currentActionTrace = context.GetCurrentActionTrace();
                if (currentActionTrace != null)
                {
                    currentActionTrace.Warning = warningMessage;
                }
            }
        }
    }
}
