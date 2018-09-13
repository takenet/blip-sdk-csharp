using System;

namespace Take.Blip.Builder.Diagnostics
{
    public class Trace
    {
        public Trace()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset Timestamp { get; set; }

        public long ElapsedMilliseconds { get; set; }

        public string Error { get; set; }
    }
}
