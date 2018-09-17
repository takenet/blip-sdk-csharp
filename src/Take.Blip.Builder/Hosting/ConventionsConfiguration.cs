using System;
using Take.Elephant.Sql;

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

        public string RedisKeyPrefix => "builder";

        public TimeSpan OnDemandCacheExpiration => TimeSpan.FromMinutes(5);

        public string SqlStorageConnectionString => @"Server=(localdb)\MSSQLLocalDB;Database=Iris;Integrated Security=true";

        public string SqlStorageDriverTypeName => typeof(SqlDatabaseDriver).FullName;

        public string ContextType => nameof(StorageContext);
    }
}