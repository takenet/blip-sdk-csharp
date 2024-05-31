using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using Serilog;
using Take.Blip.Builder.Actions.ExecuteScriptV2.Functions;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Actions.ExecuteScriptV2
{
    /// <inheritdoc />
    public class ExecuteScriptV2Action : ActionBase<ExecuteScriptV2Settings>
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;

        /// <inheritdoc />
        public ExecuteScriptV2Action(IConfiguration configuration, IHttpClient httpClient,
            ILogger logger)
            : base(nameof(ExecuteScriptV2))
        {
            HostSettings.CustomAttributeLoader = new LowerCaseMembersLoader();

            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <inheritdoc />
        public override async Task ExecuteAsync(IContext context, ExecuteScriptV2Settings settings,
            CancellationToken cancellationToken)
        {
            try
            {
                var arguments = await GetScriptArgumentsAsync(context, settings, cancellationToken);

                using var engine = new V8ScriptEngine(
                    V8ScriptEngineFlags.AddPerformanceObject |
                    V8ScriptEngineFlags.EnableTaskPromiseConversion |
                    V8ScriptEngineFlags.UseSynchronizationContexts |
                    V8ScriptEngineFlags.EnableStringifyEnhancements |
                    V8ScriptEngineFlags.EnableDateTimeConversion |
                    V8ScriptEngineFlags.EnableValueTaskPromiseConversion |
                    V8ScriptEngineFlags.HideHostExceptions
                );

                engine.DocumentSettings.AccessFlags |= DocumentAccessFlags.AllowCategoryMismatch;
                engine.MaxRuntimeHeapSize =
                    new UIntPtr((ulong)_configuration.ExecuteScriptV2MaxRuntimeHeapSize);
                engine.MaxRuntimeStackUsage =
                    new UIntPtr((ulong)_configuration.ExecuteScriptV2MaxRuntimeStackUsage);
                engine.AllowReflection = false;

                engine.RuntimeHeapSizeViolationPolicy = V8RuntimeViolationPolicy.Exception;

                // Create new token cancellation token with _configuration.ExecuteScriptV2Timeout based on the current token
                using var timeoutToken =
                    new CancellationTokenSource(_configuration.ExecuteScriptV2Timeout);
                using var linkedToken =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                        timeoutToken.Token);

                var time = new Time(_logger, context, settings, linkedToken.Token);

                engine.RegisterFunctions(settings, _httpClient, context, time,
                    linkedToken.Token);

                var result = engine.ExecuteInvoke(settings.Source, settings.Function,
                    _configuration.ExecuteScriptV2Timeout, arguments);

                await SetScriptResultAsync(context, settings, result, time, cancellationToken);
            }
            catch (Exception ex)
            {
                if (!settings.CaptureExceptions)
                {
                    throw;
                }

                string exceptionMessage = null;

                try
                {
                    exceptionMessage =
                        await _captureException(context, settings, cancellationToken, ex);
                }
                finally
                {
                    var trace = context.GetCurrentActionTrace();
                    if (trace != null)
                    {
                        trace.Warning = exceptionMessage ??
                                        "An error occurred while executing the script.";
                    }
                }
            }
        }

        private async Task<string> _captureException(IContext context,
            ExecuteScriptV2Settings settings,
            CancellationToken cancellationToken, Exception ex)
        {
            if (ex is ScriptEngineException ||
                ex is ScriptInterruptedException ||
                ex is TimeoutException ||
                ex is ArgumentException ||
                ex is ValidationException ||
                ex is OperationCanceledException)
            {
                if (!settings.ExceptionVariable.IsNullOrEmpty())
                {
                    await context.SetVariableAsync(settings.ExceptionVariable, ex.Message,
                        cancellationToken);
                }

                return ex.Message;
            }

            var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

            var exceptionMessage =
                $"Internal script error, please contact the support with the following id: {traceId}";

            _logger.Error(ex, "Internal unknown bot error, support trace id: {TraceId}",
                traceId);

            if (!settings.ExceptionVariable.IsNullOrEmpty())
            {
                await context.SetVariableAsync(settings.ExceptionVariable,
                    exceptionMessage,
                    cancellationToken);
            }

            return exceptionMessage;
        }

        private static async Task<object[]> GetScriptArgumentsAsync(
            IContext context, ExecuteScriptV2Settings settings, CancellationToken cancellationToken)
        {
            if (settings.InputVariables == null || settings.InputVariables.Length <= 0)
            {
                return null;
            }

            object[] arguments = new object[settings.InputVariables.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                arguments[i] =
                    await context.GetVariableAsync(settings.InputVariables[i],
                        cancellationToken);
            }

            return arguments;
        }

        private static async Task SetScriptResultAsync(
            IContext context, ExecuteScriptV2Settings settings, object result, Time time,
            CancellationToken cancellationToken)
        {
            var data = await ScriptObjectConverter.ToStringAsync(result, time, cancellationToken);

            if (data != null)
            {
                await context.SetVariableAsync(settings.OutputVariable, data, cancellationToken);
            }
            else
            {
                await context.DeleteVariableAsync(settings.OutputVariable, cancellationToken);
            }
        }
    }
}