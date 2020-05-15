using System;

namespace Take.Blip.Builder.Hosting
{
    public class ConventionsConfiguration : IConfiguration
    {
        public virtual TimeSpan InputProcessingTimeout => TimeSpan.FromMinutes(1);

        public virtual string RedisStorageConfiguration => "localhost";

        public virtual int RedisDatabase => 0;

        public virtual int MaxTransitionsByInput => 10;

        public virtual int TraceQueueBoundedCapacity => 512;

        public virtual int TraceQueueMaxDegreeOfParallelism => 512;

        public virtual TimeSpan TraceTimeout => TimeSpan.FromSeconds(5);

        public virtual string RedisKeyPrefix => "builder";

        public bool ContactCacheEnabled => true;

        public virtual TimeSpan ContactCacheExpiration => TimeSpan.FromMinutes(30);

        public virtual TimeSpan DefaultActionExecutionTimeout => TimeSpan.FromSeconds(30);

        public int ExecuteScriptLimitRecursion => 50;

        public int ExecuteScriptMaxStatements => 1000;

        public long ExecuteScriptLimitMemory => 100_000_000; // Nearly 100MB

        public long ExecuteScriptLimitMemoryWarning => 10_000_000; // Nearly 10MB

        public TimeSpan ExecuteScriptTimeout => TimeSpan.FromSeconds(5);
    }
}