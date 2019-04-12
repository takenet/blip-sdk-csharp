using System;
using Take.Elephant.Sql;

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

        public virtual TimeSpan OnDemandCacheExpiration => TimeSpan.FromMinutes(5);

        public virtual string SqlStorageConnectionString => @"Server=(localdb)\MSSQLLocalDB;Database=Builder;Integrated Security=true";

        public virtual string SqlStorageDriverTypeName => typeof(SqlDatabaseDriver).FullName;

        public virtual string ContextType => nameof(ExtensionContext);

        public virtual TimeSpan ContactCacheExpiration => TimeSpan.FromMinutes(30);

        public virtual TimeSpan DefaultActionExecutionTimeout => TimeSpan.FromSeconds(30);
    }
}