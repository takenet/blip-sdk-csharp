using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder.Variables
{
    public class CalendarVariableProvider : IVariableProvider
    {
        private static Regex DateOperationRegex = new Regex("(?<operation>plus|minus)(?<value>\\d+)(?<period>millisecond(s)?|second(s)?|minute(s)?|hour(s)?|day(s)?|week(s)?|month(s)?|year(s)?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public VariableSource Source => VariableSource.Calendar;

        public Task<string> GetVariableAsync(string name, IContext context, CancellationToken cancellationToken) =>
            GetVariable(name).AsCompletedTask();

        private string GetVariable(string name)
        {
            var dateTime = DateTimeOffset.UtcNow;

            var names = name.ToLowerInvariant().Split('.').ToList();
            if (names.Count > 1)
            {
                switch (names[0])
                {
                    case "tomorrow":
                        dateTime = dateTime.AddDays(1);
                        names.Remove("tomorrow");
                        break;

                    case "yesterday":
                        dateTime = dateTime.AddDays(-1);
                        names.Remove("yesterday");
                        break;

                    case "today":
                        names.Remove("today");
                        break;
                }                
            }

            if (names.Count > 1)
            {
                var match = DateOperationRegex.Match(names[0]);
                if (match.Success)
                {
                    var operation = match.Groups["operation"].Value;
                    var period = match.Groups["period"].Value;
                    var value = int.Parse(match.Groups["value"].Value);

                    dateTime = GetDateFromOperationVariable(dateTime, operation, period, value);
                    names.Remove(match.Value);
                }
            }

            switch (names[0])
            {
                case "datetime":
                    return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

                case "date":
                    return dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                case "day":
                    return dateTime.Day.ToString();

                case "month":
                    return dateTime.Month.ToString();

                case "year":
                    return dateTime.Year.ToString();

                case "time":
                    return dateTime.ToString("t", CultureInfo.InvariantCulture);

                case "hour":
                    return dateTime.Hour.ToString();

                case "minute":
                    return dateTime.Minute.ToString();

                case "second":
                    return dateTime.Second.ToString();

                case "unixtime":
                    return dateTime.ToUnixTimeSeconds().ToString();

                case "unixtimemilliseconds":
                    return dateTime.ToUnixTimeMilliseconds().ToString();

                case "dayofweek":
                    return dateTime.DayOfWeek.ToString();
            }

            return null;
        }

        private DateTimeOffset GetDateFromOperationVariable(DateTimeOffset dateTime, string operation, string period, int value)
        {
            TimeSpan interval;

            switch (period)
            {
                case "milliseconds":
                case "millisecond":
                    interval = TimeSpan.FromMilliseconds(value);
                    break;

                case "seconds":
                case "second":
                    interval = TimeSpan.FromSeconds(value);                    
                    break;

                case "minutes":
                case "minute":
                    interval = TimeSpan.FromMinutes(value);                    
                    break;

                case "hours":
                case "hour":
                    interval = TimeSpan.FromHours(value);                    
                    break;

                case "days":
                case "day":
                    interval = TimeSpan.FromDays(value);                    
                    break;

                case "weeks":
                case "week":
                    interval = TimeSpan.FromDays(value * 7);
                    break;

                case "months":
                case "month":
                    interval = TimeSpan.FromDays(value * 30);
                    break;

                case "years":
                case "year":
                    interval = TimeSpan.FromDays(value * 365);
                    break;

                default:
                    return dateTime;
            }
           
            switch (operation)
            {
                case "plus":
                    return dateTime.Add(interval);

                case "minus":
                    return dateTime.Subtract(interval);
            }

            return dateTime;
        }
    }
}
