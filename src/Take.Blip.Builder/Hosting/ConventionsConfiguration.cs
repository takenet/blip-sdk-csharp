using System;

namespace Take.Blip.Builder.Hosting
{
    public sealed class ConventionsConfiguration : IConfiguration
    {
        public TimeSpan InputProcessingTimeout => TimeSpan.FromMinutes(1);

        public string RedisStorageConfiguration => "localhost";

        public int RedisDatabase => 0;

        public int MaxTransitionsByInput => 10;

        public TimeSpan WarningElapsedTimeThreshold => TimeSpan.FromMilliseconds(500);

        public TimeSpan ErrorElapsedTimeThreshold => TimeSpan.FromMilliseconds(1000);
    }
}