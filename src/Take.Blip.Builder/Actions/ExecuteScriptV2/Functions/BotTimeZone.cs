using System;
using System.Diagnostics;
using Serilog;
using TimeZoneConverter;

namespace Take.Blip.Builder.Actions.ExecuteScriptV2.Functions
{
    /// <summary>
    /// Bot timezone helper.
    /// </summary>
    public static class BotTimeZone
    {
        private const string BRAZIL_TIMEZONE = "America/Sao_Paulo";
        private const string LOCAL_TIMEZONE_SEPARATOR = "builder:#localTimeZone";

        private static readonly TimeZoneInfo _defaultTimezone =
            TZConvert.GetTimeZoneInfo(BRAZIL_TIMEZONE);

        /// <summary>
        /// Get the bot timezone or Brazil timezone if not set.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static TimeZoneInfo GetTimeZone(ILogger logger, IContext context,
            ExecuteScriptV2Settings settings)
        {
            try
            {
                if (context.Flow.Configuration.ContainsKey(LOCAL_TIMEZONE_SEPARATOR) &&
                    settings.LocalTimeZoneEnabled)
                {
                    return TZConvert.GetTimeZoneInfo(
                        context.Flow.Configuration[LOCAL_TIMEZONE_SEPARATOR]);
                }
            }
            catch (Exception ex)
            {
                // TODO: use open telemetry to store exception after updating project to .NET 8 and adding OpenTelemetry dependency
                Activity.Current?.AddEvent(new ActivityEvent(ex.Message));

                var trace = context.GetCurrentActionTrace();
                if (trace != null)
                {
                    trace.Warning =
                        $"Could not convert timezone, using default: {_defaultTimezone.Id}";
                }

                logger.Information(ex, "Error converting timezone, using default");
            }

            return _defaultTimezone;
        }
    }
}