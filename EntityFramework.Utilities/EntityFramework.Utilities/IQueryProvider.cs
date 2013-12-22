using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace EntityFramework.Utilities
{
    public interface IQueryProvider
    {
        bool CanDelete { get; }
        bool CanUpdate { get; }
        bool CanInsert { get; }

        string GetDeleteQuery(string orginalSql);
        string GetUpdateQuery(string orginalSql, IEnumerable<UpdateSpec> propertiesToUpdate);
        void InsertItems<T>(IEnumerable<T> items, string tableName, IList<ColumnMapping> properties, DbConnection storeConnection);

        bool CanHandle(DbConnection storeConnection);

    }
}
