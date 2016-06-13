//using System;
//using System.Collections.Generic;
//using System.Data.Common;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace EntityFramework.Utilities
//{
//    public class SqlQueryProvider : IQueryProvider
//    {
//        public bool CanDelete { get { return true; } }
//        public bool CanUpdate { get { return true; } }
//        public bool CanInsert { get { return true; } }
//        public bool CanBulkUpdate { get { return true; } }

//        public string GetDeleteQuery(QueryInformation queryInfo)
//        {
//            return string.Format("DELETE FROM [{0}].[{1}] {2}", queryInfo.Schema, queryInfo.Table, queryInfo.WhereSql);
//        }

//        public string GetUpdateQuery(QueryInformation predicateQueryInfo, QueryInformation modificationQueryInfo)
//        {
//            var msql = modificationQueryInfo.WhereSql.Replace("WHERE ", "");
//            var indexOfAnd = msql.IndexOf("AND");
//            var update = indexOfAnd == -1 ? msql : msql.Substring(0, indexOfAnd).Trim();

//            var updateRegex = new Regex(@"(\[[^\]]+\])[^=]+=(.+)", RegexOptions.IgnoreCase);
//            var match = updateRegex.Match(update);
//            string updateSql;
//            if (match.Success)
//            {
//                var col = match.Groups[1];
//                var rest = match.Groups[2].Value;

//                rest = SqlStringHelper.FixParantheses(rest);

//                updateSql = col.Value + " = " + rest;
//            }
//            else
//            {
//                updateSql = string.Join(" = ", update.Split(new string[] { " = " }, StringSplitOptions.RemoveEmptyEntries).Reverse());
//            }


//            return string.Format("UPDATE [{0}].[{1}] SET {2} {3}", predicateQueryInfo.Schema, predicateQueryInfo.Table, updateSql, predicateQueryInfo.WhereSql);
//        }

//        public void InsertItems<T>(IEnumerable<T> items, string schema, string tableName, IList<ColumnMapping> properties, DbConnection storeConnection, int? batchSize)
//        {
//            InsertItems(items, schema, tableName, properties, new BulkSettings { BatchSize = batchSize, Connection = storeConnection });
//        }

//        public void InsertItems<T>(IEnumerable<T> items, string schema, string tableName, IList<ColumnMapping> properties, BulkSettings settings)
//        {
//            if (settings.ReturnIdsOnInsert && properties.Any(p => p.IsPrimaryKey && p.IsStoreGenerated))
//            {
//                var tempTableName = "temp_" + tableName + "_" + DateTime.Now.Ticks;
//                var str = SqlServerTSQLGenerator.BuildCreateTableCommand(schema, tempTableName, properties.Where(p => !p.IsStoreGenerated));
//                var mergeCommand = SqlServerTSQLGenerator.BuildSelectIntoCommand(tableName, properties, tempTableName);

//                var con = settings.Connection as SqlConnection;
//                if (con.State != System.Data.ConnectionState.Open)
//                {
//                    con.Open();
//                }

//                var setters = properties.Where(p => p.IsStoreGenerated).Select((p, i) => new { i, setter = ExpressionHelper.PropertyNameToSetter<T>(p.NameOnObject) }).ToList();


//                using (var createCommand = new SqlCommand(str, con))
//                using (var mCommand = new SqlCommand(mergeCommand, con))
//                using (var dCommand = new SqlCommand(SqlServerTSQLGenerator.BuildDropStatement(schema, tempTableName), con))
//                {
//                    createCommand.ExecuteNonQuery();
//                    InsertItemsWithoutIdInsert(items, schema, tempTableName, properties.Where(p => !p.IsStoreGenerated).ToList(), settings);
//                    using (var reader = mCommand.ExecuteReader())
//                    {
//                        foreach (var item in items)
//                        {
//                            reader.Read();

//                            foreach (var setter in setters)
//                            {
//                                setter.setter(item, reader.GetValue(setter.i));
//                            }
//                        }
//                    }
//                    dCommand.ExecuteNonQuery();
//                }
//            }
//            else
//            {
//                InsertItemsWithoutIdInsert(items, schema, tableName, properties, settings);
//            }

//        }


//        public async Task InsertItemsAsync<T>(IEnumerable<T> items, string schema, string tableName, IList<ColumnMapping> properties, BulkSettings settings)
//        {
//            if (settings.ReturnIdsOnInsert && properties.Any(p => p.IsPrimaryKey && p.IsStoreGenerated))
//            {
//                var tempTableName = "temp_" + tableName + "_" + DateTime.Now.Ticks;
//                var str = SqlServerTSQLGenerator.BuildCreateTableCommand(schema, tempTableName, properties.Where(p => !p.IsStoreGenerated));
//                var mergeCommand = SqlServerTSQLGenerator.BuildSelectIntoCommand(tableName, properties, tempTableName);

//                var con = settings.Connection as SqlConnection;
//                if (con.State != System.Data.ConnectionState.Open)
//                {
//                    await con.OpenAsync();
//                }

//                var setters = properties.Where(p => p.IsStoreGenerated).Select((p, i) => new { i, setter = ExpressionHelper.PropertyNameToSetter<T>(p.NameOnObject) }).ToList();


//                using (var createCommand = new SqlCommand(str, con))
//                using (var mCommand = new SqlCommand(mergeCommand, con))
//                using (var dCommand = new SqlCommand(SqlServerTSQLGenerator.BuildDropStatement(schema, tempTableName), con))
//                {
//                    await createCommand.ExecuteNonQueryAsync();
//                    await InsertItemsWithoutIdInsertAsync(items, schema, tempTableName, properties.Where(p => !p.IsStoreGenerated).ToList(), settings);
//                    using (var reader = await mCommand.ExecuteReaderAsync())
//                    {
//                        foreach (var item in items)
//                        {
//                            await reader.ReadAsync();

//                            foreach (var setter in setters)
//                            {
//                                setter.setter(item, reader.GetValue(setter.i));
//                            }
//                        }
//                    }
//                    await dCommand.ExecuteNonQueryAsync();
//                }
//            }
//            else
//            {
//                await InsertItemsWithoutIdInsertAsync(items, schema, tableName, properties, settings);
//            }

//        }

//        private async Task InsertItemsWithoutIdInsertAsync<T>(IEnumerable<T> items, string schema, string tableName, IList<ColumnMapping> properties, BulkSettings settings)
//        {
//            using (var reader = new EFDataReader<T>(items, properties))
//            {
//                var con = settings.Connection as SqlConnection;
//                if (con.State != System.Data.ConnectionState.Open)
//                {
//                    await con.OpenAsync();
//                }
//                using (SqlBulkCopy copy = new SqlBulkCopy(con))
//                {
//                    ApplyBulkCopyOptions<T>(schema, tableName, properties, settings, reader, copy);
//                    await copy.WriteToServerAsync(reader);
//                    copy.Close();
//                }
//            }
//        }

//        private void InsertItemsWithoutIdInsert<T>(IEnumerable<T> items, string schema, string tableName, IList<ColumnMapping> properties, BulkSettings settings)
//        {
//            using (var reader = new EFDataReader<T>(items, properties))
//            {
//                var con = settings.Connection as SqlConnection;
//                if (con.State != System.Data.ConnectionState.Open)
//                {
//                    con.Open();
//                }
//                using (SqlBulkCopy copy = new SqlBulkCopy(con))
//                {
//                    ApplyBulkCopyOptions<T>(schema, tableName, properties, settings, reader, copy);
//                    copy.WriteToServer(reader);
//                    copy.Close();
//                }
//            }
//        }

//        private static void ApplyBulkCopyOptions<T>(string schema, string tableName, IList<ColumnMapping> properties, BulkSettings settings, EFDataReader<T> reader, SqlBulkCopy copy)
//        {
//            copy.BatchSize = Math.Min(reader.RecordsAffected, settings.BatchSize ?? 15000); //default batch size
//            if (!string.IsNullOrWhiteSpace(schema))
//            {
//                copy.DestinationTableName = string.Format("[{0}].[{1}]", schema, tableName);
//            }
//            else
//            {
//                copy.DestinationTableName = "[" + tableName + "]";
//            }

//            copy.NotifyAfter = 0;

//            foreach (var i in Enumerable.Range(0, reader.FieldCount))
//            {
//                copy.ColumnMappings.Add(i, properties[i].NameInDatabase);
//            }
//        }



//        public void UpdateItems<T>(IEnumerable<T> items, string schema, string tableName, IList<ColumnMapping> properties, DbConnection storeConnection, int? batchSize, UpdateSpecification<T> updateSpecification)
//        {
//            var tempTableName = "temp_" + tableName + "_" + DateTime.Now.Ticks;
//            var columnsToUpdate = updateSpecification.Properties.Select(p => p.GetPropertyName()).ToDictionary(x => x);
//            var filtered = properties.Where(p => columnsToUpdate.ContainsKey(p.NameOnObject) || p.IsPrimaryKey).ToList();

//            var str = SqlServerTSQLGenerator.BuildCreateTableCommand(schema, tempTableName, filtered);


//            var con = storeConnection as SqlConnection;
//            if (con.State != System.Data.ConnectionState.Open)
//            {
//                con.Open();
//            }

//            var mergeCommand = SqlServerTSQLGenerator.BuildMergeCommand(tableName, filtered, tempTableName);

//            using (var createCommand = new SqlCommand(str, con))
//            using (var mCommand = new SqlCommand(mergeCommand, con))
//            using (var dCommand = new SqlCommand(SqlServerTSQLGenerator.BuildDropStatement(schema, tempTableName), con))
//            {
//                createCommand.ExecuteNonQuery();
//                InsertItems(items, schema, tempTableName, filtered, storeConnection, batchSize);
//                mCommand.ExecuteNonQuery();
//                dCommand.ExecuteNonQuery();
//            }
//        }

//        public async Task UpdateItemsAsync<T>(IEnumerable<T> items, string schema, string tableName, List<ColumnMapping> properties, BulkSettings settings, UpdateSpecification<T> updateSpecification)
//        {
//            var tempTableName = "temp_" + tableName + "_" + DateTime.Now.Ticks;
//            var columnsToUpdate = updateSpecification.Properties.Select(p => p.GetPropertyName()).ToDictionary(x => x);
//            var filtered = properties.Where(p => columnsToUpdate.ContainsKey(p.NameOnObject) || p.IsPrimaryKey).ToList();

//            var str = SqlServerTSQLGenerator.BuildCreateTableCommand(schema, tempTableName, filtered);


//            var con = settings.Connection as SqlConnection;
//            if (con.State != System.Data.ConnectionState.Open)
//            {
//                await con.OpenAsync();
//            }

//            var mergeCommand = SqlServerTSQLGenerator.BuildMergeCommand(tableName, filtered, tempTableName);

//            using (var createCommand = new SqlCommand(str, con))
//            using (var mCommand = new SqlCommand(mergeCommand, con))
//            using (var dCommand = new SqlCommand(SqlServerTSQLGenerator.BuildDropStatement(schema, tempTableName), con))
//            {
//                await createCommand.ExecuteNonQueryAsync();
//                await InsertItemsAsync(items, schema, tempTableName, filtered, settings);
//                await mCommand.ExecuteNonQueryAsync();
//                await dCommand.ExecuteNonQueryAsync();
//            }
//        }

//        public bool CanHandle(System.Data.Common.DbConnection storeConnection)
//        {
//            return storeConnection is SqlConnection;
//        }


//        public QueryInformation GetQueryInformation<T>(System.Data.Entity.Core.Objects.ObjectQuery<T> query)
//        {
//            var fromRegex = new Regex(@"FROM \[([^\]]+)\]\.\[([^\]]+)\] AS (\[[^\]]+\])", RegexOptions.IgnoreCase);

//            var queryInfo = new QueryInformation();

//            var str = query.ToTraceString();
//            var match = fromRegex.Match(str);
//            queryInfo.Schema = match.Groups[1].Value;
//            queryInfo.Table = match.Groups[2].Value;
//            queryInfo.Alias = match.Groups[3].Value;

//            var i = str.IndexOf("WHERE");
//            if (i > 0)
//            {
//                var whereClause = str.Substring(i);
//                queryInfo.WhereSql = whereClause.Replace(queryInfo.Alias + ".", "");
//            }
//            return queryInfo;
//        }
//    }
//}
