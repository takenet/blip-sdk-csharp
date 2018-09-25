using Lime.Protocol;
using System.Data;
using System.Runtime.Serialization;
using Take.Blip.Builder.Hosting;
using Take.Elephant.Sql;
using Take.Elephant.Sql.Mapping;

namespace Take.Blip.Builder.Storage.Sql
{
    public class OwnerCallerNameDocumentMap : ExpirableKeySqlMap<OwnerCallerName, StorageDocument>, ISourceOwnerCallerNameDocumentMap
    {
        public const string TABLE_NAME = "Contexts";
        public const string EXPIRATION_COLUMN_NAME = "Expiration";

        public static ITable ContextsTable = TableBuilder
            .WithName(TABLE_NAME)
            .WithKeyColumnsFromTypeDataMemberProperties<OwnerCallerName>()
            .WithColumnsFromTypeProperties<StorageDocument>()
            .WithColumn(nameof(StorageDocument.Document), new SqlType(DbType.String, int.MaxValue))
            .WithColumn(EXPIRATION_COLUMN_NAME, new SqlType(DbType.DateTimeOffset))
            .Build();

        public OwnerCallerNameDocumentMap(IDatabaseDriver databaseDriver, IConfiguration configuration)
            : base(
                  databaseDriver,
                  configuration.SqlStorageConnectionString,
                  ContextsTable,
                  new TypeMapper<OwnerCallerName>(ContextsTable),
                  new TypeMapper<StorageDocument>(ContextsTable),
                  EXPIRATION_COLUMN_NAME)
        {
        }

        [DataContract]
        private class CallerNameStorageDocument : StorageDocument
        {
            [DataMember]
            public Identity Caller { get; set; }

            [DataMember]
            public string Name { get; set; }
        }
    }
}
