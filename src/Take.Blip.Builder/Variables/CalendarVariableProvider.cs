using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder.Variables
{
    public class CalendarVariableProvider : IVariableProvider
    {
        public VariableSource Source => VariableSource.Calendar;

        public Task<string> GetVariableAsync(string name, Identity user, CancellationToken cancellationToken) =>
            GetVariable(name).AsCompletedTask();

        private string GetVariable(string name)
        {
            var now = DateTimeOffset.UtcNow;

            switch (name)
            {
                case "datetime":
                    return now.ToString("s", CultureInfo.InvariantCulture);

                case "date":
                    return now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                case "day":
                    return now.Day.ToString();

                case "month":
                    return now.Month.ToString();

                case "year":
                    return now.Year.ToString();

                case "time":
                    return now.ToString("t", CultureInfo.InvariantCulture);

                case "hour":
                    return now.Hour.ToString();

                case "minute":
                    return now.Minute.ToString();

                case "second":
                    return now.Second.ToString();

                case "unixtime":
                    return now.ToUnixTimeSeconds().ToString();

                case "unixtimemilliseconds":
                    return now.ToUnixTimeMilliseconds().ToString();

                case "dayofweek":
                    return now.DayOfWeek.ToString();

            }

            return null;
        }
    }
}
