using System;

namespace Take.Blip.Builder.Hosting
{
    public sealed class ConventionsConfiguration : IConfiguration
    {
        public TimeSpan ExecutionSemaphoreExpiration => TimeSpan.FromMinutes(5);

        public string RedisStorageConfiguration => "localhost";

        public int RedisDatabase => 0;
    }
}