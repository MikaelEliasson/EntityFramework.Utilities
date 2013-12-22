using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace EntityFramework.Utilities
{
    public class SqlQueryProvider : IQueryProvider
    {
        public bool CanDelete { get { return true; } }
        public bool CanUpdate { get { return true; } }
        public bool CanInsert { get { return true; } }

        public string GetDeleteQuery(string orginalSql)
        {
            throw new NotImplementedException();
        }

        public string GetUpdateQuery(string orginalSql, IEnumerable<UpdateSpec> propertiesToUpdate)
        {
            throw new NotImplementedException();
        }

        public void InsertItems<T>(IEnumerable<T> items, string tableName, IList<ColumnMapping> properties, DbConnection storeConnection)
        {
            using (var reader = new EFDataReader<T>(items, properties))
            {
                using (SqlBulkCopy copy = new SqlBulkCopy(storeConnection.ConnectionString, SqlBulkCopyOptions.Default))
                {
                    copy.BatchSize = Math.Min(reader.RecordsAffected, 5000); //default batch size
                    copy.DestinationTableName = tableName;
                    copy.NotifyAfter = 0;

                    foreach (var i in Enumerable.Range(0, reader.FieldCount))
                    {
                        copy.ColumnMappings.Add(i, properties[i].NameInDatabase);
                    }
                    copy.WriteToServer(reader);
                    copy.Close();
                }
            }
        }


        public bool CanHandle(System.Data.Common.DbConnection storeConnection)
        {
            return storeConnection is SqlConnection;
        }
    }
}
