using System;

namespace Take.Blip.Builder.Hosting
{
    public sealed class ConventionsConfiguration : IConfiguration
    {
        public TimeSpan InputProcessingTimeout => TimeSpan.FromMinutes(1);

        public string RedisStorageConfiguration => "localhost";

        public int RedisDatabase => 0;

        public int MaxTransitionsByInput => 10;

        public int TraceQueueBoundedCapacity => 512;

        public int TraceQueueMaxDegreeOfParalelism => 512;

        public TimeSpan TraceProcessingTimeout => TimeSpan.FromSeconds(5);
    }
}