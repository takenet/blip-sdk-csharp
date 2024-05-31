using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Microsoft.ClearScript;
using Serilog;

namespace Take.Blip.Builder.Actions.ExecuteScriptV2.Functions
{
    /// <summary>
    /// Time to manipulate time inside the script engine
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Time
    {
        private const string DEFAULT_TIME_FORMAT = "yyyy-MM-dd'T'HH:mm:ss.fffffffK";
        private const string DEFAULT_CULTURE_INFO = "en-US";

        private const string TIMEZONE_KEY = "timeZone";
        private const string FORMAT_KEY = "format";
        private const string CULTURE_KEY = "culture";

        private readonly TimeZoneInfo _timeZoneInfo;
        private readonly CancellationToken _cancellationToken;


        /// <summary>
        /// Initializes a new instance of the <see cref="Time"/> class.
        /// </summary>
        public Time(ILogger logger, IContext context, ExecuteScriptV2Settings settings,
            CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            _timeZoneInfo = BotTimeZone.GetTimeZone(logger, context, settings);
        }

        /// <summary>
        /// Parses the date to a DateTime object using the time timezone info.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public DateTime ParseDate(string date, IScriptObject options = null)
        {
            var timezoneOption = options?[TIMEZONE_KEY] as string;

            var timeZoneInfo = (timezoneOption?.IsNullOrEmpty() ?? true)
                ? _timeZoneInfo
                : TimeZoneInfo.FindSystemTimeZoneById(timezoneOption);

            if (!(options?[FORMAT_KEY] is string format))
            {
#pragma warning disable S6580 - Use a format provider when parsing date and time.
                if (DateTime.TryParse(date, out var parsedDate))
#pragma warning restore S6580
                {
                    return parsedDate.Kind == DateTimeKind.Unspecified
                        ?
                        // Convert the parsed DateTimeOffset to the desired time zone
                        TimeZoneInfo.ConvertTime(parsedDate, timeZoneInfo, _timeZoneInfo)
                        : parsedDate;
                }

                format = DEFAULT_TIME_FORMAT;
            }

            var cultureOption = options?[CULTURE_KEY] as string;
            var culture = new CultureInfo((cultureOption?.IsNullOrEmpty() ?? true)
                ? DEFAULT_CULTURE_INFO
                : cultureOption);

            // Parse the date string to a DateTimeOffset object
            if (!DateTime.TryParseExact(date,
                    format.IsNullOrEmpty() ? DEFAULT_TIME_FORMAT : format, culture,
                    DateTimeStyles.None, out var parsedDateOffset))
            {
                throw new ArgumentException($"Invalid date format ({format}) to parse: {date}",
                    nameof(date));
            }

            // Convert the parsed DateTimeOffset to the desired time zone
            return parsedDateOffset.Kind == DateTimeKind.Unspecified
                ? TimeZoneInfo.ConvertTime(parsedDateOffset, timeZoneInfo, _timeZoneInfo)
                : parsedDateOffset;
        }

        /// <summary>
        /// Converts a DateTime object to a string using the time timezone info.
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <param name="options">The options to parse.</param>
        /// <returns></returns>
        public string DateToString(DateTime date, IScriptObject options = null)
        {
            // Convert the DateTime object to a DateTimeOffset object
            var convertedDate = new DateTimeOffset(date);

            return DateOffsetToString(convertedDate, options);
        }

        /// <summary>
        /// Converts a DateTimeOffset object to a string using the time timezone info.
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <param name="options">The options to parse.</param>
        /// <returns></returns>
        public string DateOffsetToString(DateTimeOffset date,
            IScriptObject options = null)
        {
            var timezoneOption = options?[TIMEZONE_KEY] as string;

            var timeZoneInfo = (timezoneOption?.IsNullOrEmpty() ?? true)
                ? _timeZoneInfo
                : TimeZoneInfo.FindSystemTimeZoneById(timezoneOption);

            // Convert the DateTimeOffset to the desired time zone
            var convertedDateInTimeZone = TimeZoneInfo.ConvertTime(date, timeZoneInfo);

            var formatOption = options?[FORMAT_KEY] as string;

            // Return the string representation of the converted DateTimeOffset
            return convertedDateInTimeZone.ToString((formatOption?.IsNullOrEmpty() ?? true)
                ? DEFAULT_TIME_FORMAT
                : formatOption);
        }

        /// <summary>
        /// Sleeps for the specified millisecondsDelay.
        /// </summary>
        /// <param name="millisecondsDelay"></param>
        public void Sleep(int millisecondsDelay)
        {
            var task = Task.Delay(millisecondsDelay, _cancellationToken);
            try
            {
                task.Wait(_cancellationToken);
            }
            catch when (task.IsCanceled)
            {
                throw new TimeoutException("Script execution timed out");
            }
        }
    }
}