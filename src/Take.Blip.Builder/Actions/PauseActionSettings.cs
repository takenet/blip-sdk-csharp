using System;

namespace Take.Blip.Builder.Actions
{
    public class PauseActionSettings
    {
        private const int DEFAULT_INTERVAL = 2000;
        private static readonly Random Random  = new Random();

        public int Interval { get; set; }

        public bool SendComposing { get; set; }

        public bool Randomize { get; set; }

        public TimeSpan GetIntervalTimeSpan()
        {
            var interval = Interval;
            if (interval == 0) interval = DEFAULT_INTERVAL;
            if (Randomize) interval = Random.Next(interval / 2, interval * 2);
            return TimeSpan.FromMilliseconds(interval);
        }
    }
}