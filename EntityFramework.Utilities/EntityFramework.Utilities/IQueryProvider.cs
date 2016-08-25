using System.Collections.Generic;
using System.Data.Common;

namespace EntityFramework.Utilities
{
    public interface IQueryProvider
    {
        bool CanDelete { get; }
        bool CanUpdate { get; }
        bool CanInsert { get; }
        bool CanBulkUpdate { get; }

        string GetDeleteQuery(QueryInformation queryInformation);
        string GetUpdateQuery(QueryInformation predicateQueryInfo, QueryInformation modificationQueryInfo);
        void InsertItems<T>(IEnumerable<T> items, string schema, string tableName, IList<ColumnMapping> properties, DbConnection storeConnection, int? batchSize);
        void UpdateItems<T>(IEnumerable<T> items, string schema, string tableName, IList<ColumnMapping> properties, DbConnection storeConnection, int? batchSize, UpdateSpecification<T> updateSpecification);
        void UpsertItems<T>(IEnumerable<T> items, string schema, string tableName, IList<ColumnMapping> properties, DbConnection storeConnection, int? batchSize, HashSet<string> identitySpecification, HashSet<string> updateSpecification);

        bool CanHandle(DbConnection storeConnection);


        QueryInformation GetQueryInformation<T>(System.Data.Entity.Core.Objects.ObjectQuery<T> query);


    }
}
