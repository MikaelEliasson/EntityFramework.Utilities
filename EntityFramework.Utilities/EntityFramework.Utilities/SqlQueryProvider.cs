using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EntityFramework.Utilities
{
    public class SqlQueryProvider : IQueryProvider
    {
        public bool CanDelete { get { return true; } }
        public bool CanUpdate { get { return true; } }
        public bool CanInsert { get { return true; } }
        public bool CanBulkUpdate { get { return true; } }

        public string GetDeleteQuery(QueryInformation queryInfo)
        {
            return string.Format("DELETE FROM [{0}].[{1}] {2}", queryInfo.Schema, queryInfo.Table, queryInfo.WhereSql);
        }

        public string GetUpdateQuery(QueryInformation predicateQueryInfo, QueryInformation modificationQueryInfo)
        {
            var msql = modificationQueryInfo.WhereSql.Replace("WHERE ", "");
            var indexOfAnd = msql.IndexOf("AND");
            var update = indexOfAnd == -1 ? msql : msql.Substring(0, indexOfAnd).Trim();

            var updateRegex = new Regex(@"(\[[^\]]+\])[^=]+=(.+)", RegexOptions.IgnoreCase);
            var match = updateRegex.Match(update);
            string updateSql;
            if (match.Success)
            {
                var col = match.Groups[1];
                var rest = match.Groups[2].Value;

                rest = SqlStringHelper.FixParantheses(rest);

                updateSql = col.Value + " = " + rest;
            }
            else
            {
                updateSql = string.Join(" = ", update.Split(new string[]{" = "}, StringSplitOptions.RemoveEmptyEntries).Reverse());
            }
           

            return string.Format("UPDATE [{0}].[{1}] SET {2} {3}", predicateQueryInfo.Schema, predicateQueryInfo.Table, updateSql, predicateQueryInfo.WhereSql);
        }

        public void InsertItems<T>(IEnumerable<T> items, string schema, string tableName, IList<ColumnMapping> properties, DbConnection storeConnection, int? batchSize)
        {
            using (var reader = new EFDataReader<T>(items, properties))
            {
                var con = storeConnection as SqlConnection;
                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                using (SqlBulkCopy copy = new SqlBulkCopy(con))
                {
                    copy.BatchSize = Math.Min(reader.RecordsAffected, batchSize ?? 15000); //default batch size
                    if (!string.IsNullOrWhiteSpace(schema))
                    {
                        copy.DestinationTableName = string.Format("[{0}].[{1}]", schema, tableName);
                    }
                    else
                    {
                        copy.DestinationTableName = "[" + tableName + "]";
                    }
                    
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


        public void UpdateItems<T>(IEnumerable<T> items, string schema, string tableName, IList<ColumnMapping> properties, DbConnection storeConnection, int? batchSize, UpdateSpecification<T> updateSpecification)
        {
            var tempTableName = "temp_" + tableName + "_" + DateTime.Now.Ticks;
            var columnsToUpdate = updateSpecification.Properties.Select(p => p.GetPropertyName()).ToDictionary(x => x);
            var filtered = properties.Where(p => columnsToUpdate.ContainsKey(p.NameOnObject) || p.IsPrimaryKey).ToList();
            var columns = filtered.Select(c => "[" + c.NameInDatabase + "] " + c.DataType);
            var pkConstraint = string.Join(", ", properties.Where(p => p.IsPrimaryKey).Select(c => "[" + c.NameInDatabase + "]"));

            var str = string.Format("CREATE TABLE {0}.[{1}]({2}, PRIMARY KEY ({3}))", schema, tempTableName, string.Join(", ", columns), pkConstraint);

            var con = storeConnection as SqlConnection;
            if (con.State != System.Data.ConnectionState.Open)
            {
                con.Open();
            }

            var setters = string.Join(",", filtered.Where(c => !c.IsPrimaryKey).Select(c => "[" + c.NameInDatabase + "] = TEMP.[" + c.NameInDatabase + "]"));
            var pks = properties.Where(p => p.IsPrimaryKey).Select(x => "ORIG.[" + x.NameInDatabase + "] = TEMP.[" + x.NameInDatabase + "]");
            var filter = string.Join(" and ",  pks);
            var mergeCommand =  string.Format(@"UPDATE [{0}]
                SET
                    {3}
                FROM
                    [{0}] ORIG
                INNER JOIN
                     [{1}] TEMP
                ON 
                    {2}", tableName, tempTableName, filter, setters);

            using (var createCommand = new SqlCommand(str, con))
            using (var mCommand = new SqlCommand(mergeCommand, con))
            using (var dCommand = new SqlCommand(string.Format("DROP table {0}.[{1}]", schema, tempTableName), con))
            {
                createCommand.ExecuteNonQuery();
                InsertItems(items, schema, tempTableName, filtered, storeConnection, batchSize);
                mCommand.ExecuteNonQuery();
                dCommand.ExecuteNonQuery();
            }

            
        }


        public bool CanHandle(System.Data.Common.DbConnection storeConnection)
        {
            return storeConnection is SqlConnection;
        }


        public QueryInformation GetQueryInformation<T>(System.Data.Entity.Core.Objects.ObjectQuery<T> query)
        {
            var fromRegex = new Regex(@"FROM \[([^\]]+)\]\.\[([^\]]+)\] AS (\[[^\]]+\])", RegexOptions.IgnoreCase);

            var queryInfo = new QueryInformation();

            var str = query.ToTraceString();
            var match = fromRegex.Match(str);
            queryInfo.Schema = match.Groups[1].Value;
            queryInfo.Table = match.Groups[2].Value;
            queryInfo.Alias = match.Groups[3].Value;

            var i = str.IndexOf("WHERE");
            if (i > 0)
            {
                var whereClause = str.Substring(i);
                queryInfo.WhereSql = whereClause.Replace(queryInfo.Alias + ".", "");
            }
            return queryInfo;
        }

    }
}
