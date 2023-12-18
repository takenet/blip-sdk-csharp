﻿using System.Threading;
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
using Esprima;

namespace Take.Blip.Builder.Actions.ExecuteScript
{
    public class ExecuteScriptAction : ActionBase<ExecuteScriptSettings>
    {
        private const string DEFAULT_FUNCTION = "run";
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        const string BRAZIL_TIMEZONE = "E. South America Standard Time";
        const string LOCAL_TIMEZONE_SEPARATOR = "builder:#localTimeZone";

        public ExecuteScriptAction(IConfiguration configuration, ILogger logger)
            : base(nameof(ExecuteScript))
        {
            _configuration = configuration;
            _logger = logger;
        }

        public override async Task ExecuteAsync(IContext context, ExecuteScriptSettings settings, CancellationToken cancellationToken)
        {
            var arguments = await GetScriptArgumentsAsync(context, settings, cancellationToken);
            TimeZoneInfo timeZoneLocal = TZConvert.GetTimeZoneInfo(BRAZIL_TIMEZONE);
            Engine engine;

            try
            {
                if (context.Flow.Configuration.ContainsKey(LOCAL_TIMEZONE_SEPARATOR) && settings.LocalTimeZoneEnabled)
                {
                    timeZoneLocal = TZConvert.GetTimeZoneInfo(context.Flow.Configuration[LOCAL_TIMEZONE_SEPARATOR]);
                }
            }
            catch (Exception e)
            {
                _logger.Information(e, "Error converting timezone");
            }

            engine = new Engine(options => options
                    .LimitRecursion(_configuration.ExecuteScriptLimitRecursion)
                    .MaxStatements(_configuration.ExecuteScriptMaxStatements)
                    .LimitMemory(_configuration.ExecuteScriptLimitMemory)
                    .TimeoutInterval(_configuration.ExecuteScriptTimeout)
                    .DebugMode()
                    .LocalTimeZone(timeZoneLocal));

            engine.Step += (sender, e) =>
            {
                CheckMemoryUsage(context, e);
                return StepMode.Into;
            };

            var DefaultParserOptions = new ParserOptions()
            {
                AdaptRegexp = false,
                Tolerant = true
            };

            try
            {
                engine = engine.Execute(settings.Source, DefaultParserOptions);

                var result = arguments != null
                   ? engine.Invoke(settings.Function ?? DEFAULT_FUNCTION, arguments)
                   : engine.Invoke(settings.Function ?? DEFAULT_FUNCTION);

                await SetScriptResultAsync(context, settings, result, cancellationToken);
            } catch (Exception ex)
            {
                throw TransformJavascriptException(ex);
            }
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
                using (LogContext.PushProperty(nameof(DebugInformation), debugInformation, true))
                    _logger.Warning("The script memory allocation ({CurrentMemoryUsage}:N0 bytes) is above the warning threshold of {ExecuteScriptLimitMemoryWarning}:N0 bytes",
                        debugInformation.CurrentMemoryUsage,
                        _configuration.ExecuteScriptLimitMemoryWarning);


                var currentActionTrace = context.GetCurrentActionTrace();
                if (currentActionTrace != null)
                {
                    currentActionTrace.Warning = $"The script memory allocation ({debugInformation.CurrentMemoryUsage:N0} bytes) is above the warning threshold of {_configuration.ExecuteScriptLimitMemoryWarning:N0} bytes";
                }
            }
        }
        private static Exception TransformJavascriptException(Exception ex)
        {
            if (ex is JavaScriptException)
            {
                var jIntException = (ex as JavaScriptException);

                //It's needed because the Error property of JavascriptException is very large and later it's added to application log
                return new InternalJavaScriptException(ex.Message)
                {
                    CallStack = jIntException.CallStack,
                    Location = jIntException.Location,
                    Source = "Jint"
                };
            }

            return ex;
        }
    }
}
