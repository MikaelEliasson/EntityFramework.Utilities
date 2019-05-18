using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace EntityFramework.Utilities.SqlServer
{
    public class BulkInserter
    {
        public Func<string, string> TempTableNameGenerator = tableName => "temp_" + tableName + "_" + DateTime.Now.Ticks;

        public virtual async Task InsertItemsAsync<T>(IEnumerable<T> items, BulkTableSpec tableSpec, SqlServerBulkSettings settings)
        {
            var tableName = tableSpec.TableMapping.TableName;
            var schema = tableSpec.TableMapping.Schema;

            if (settings.ReturnIdsOnInsert && tableSpec.Properties.Any(p => p.IsPrimaryKey && p.IsStoreGenerated))
            {
                var tempTableName = TempTableNameGenerator(tableName);
                var tempSpec = tableSpec.Copy();
                tempSpec.TableMapping.TableName = tempTableName;

                var mergeCommand = settings.TempSettings.SqlGenerator.BuildSelectIntoCommand(tableName, tableSpec.Properties, tempTableName);

                var setters = tableSpec.Properties.Where(p => p.IsStoreGenerated).Select((p, i) => new { i, setter = ExpressionHelper.PropertyNameToSetter<T>(p.NameOnObject) }).ToList();
                var tempSettings = settings.TempSettings;
                tempSettings.PreInsert = async (connection, tran) =>
                {
                    await settings.TempSettings.Factory.TableCreator().CreateTable(connection, tran, tempSpec);
                };

                tempSettings.PostInsert = async (connection, tran) =>
                {
                    using (var mCommand = new SqlCommand(mergeCommand, connection, tran))
                    using (var dCommand = new SqlCommand(settings.TempSettings.SqlGenerator.BuildDropStatement(schema, tempTableName), connection, tran))
                    {
                        using (var reader = await mCommand.ExecuteReaderAsync())
                        {
                            foreach (var item in items)
                            {
                                await reader.ReadAsync();

                                foreach (var setter in setters)
                                {
                                    setter.setter(item, reader.GetValue(setter.i));
                                }
                            }
                        }
                        await dCommand.ExecuteNonQueryAsync();
                    };
                };

                var copy = tableSpec.Copy();
                copy.Properties = copy.Properties.Where(p => !p.IsStoreGenerated).ToList();
                copy.TableMapping.TableName = tempTableName;
                await InsertItemsAsyncInternal(items, copy, tempSettings);
            }
            else
            {
                await InsertItemsAsyncInternal(items, tableSpec, settings);
            }
        }

        protected virtual async Task InsertItemsAsyncInternal<T>(IEnumerable<T> items, BulkTableSpec tableSpec, SqlServerBulkSettings settings)
        {
            using (var reader = new EFDataReader<T>(items, tableSpec.Properties))
            {
                var con = settings.Connection;
                if (con.State != System.Data.ConnectionState.Open)
                {
                    await con.OpenAsync();
                }

                if(settings.PreInsert != null)
                {
                    await settings.PreInsert.Invoke(con, settings.Transaction);
                }
                using (SqlBulkCopy copy = new SqlBulkCopy(con, settings.SqlBulkCopyOptions, settings.Transaction))
                {
                    ApplyBulkCopyOptions(tableSpec.TableMapping.Schema, tableSpec.TableMapping.TableName, tableSpec.Properties, settings, reader, copy);
                    await copy.WriteToServerAsync(reader);
                    if (settings.PostInsert != null)
                    {
                        await settings.PostInsert.Invoke(con, settings.Transaction);
                    }
                }
            }
        }

        public virtual async Task UpdateItemsAsync<T>(IEnumerable<T> items, BulkTableSpec tableSpec, SqlServerBulkSettings settings, UpdateSpecification<T> updateSpecification)
        {
            var tableName = tableSpec.TableMapping.TableName;
            var schema = tableSpec.TableMapping.Schema;
            var tempTableName = TempTableNameGenerator(tableName);
            var tempSpec = tableSpec.Copy();
            tempSpec.TableMapping.TableName = tempTableName;

            var columnsToUpdate = updateSpecification.Properties.Select(p => p.GetPropertyName()).ToDictionary(x => x);
            var filtered = tableSpec.Properties.Where(p => (p.NameOnObject != null && columnsToUpdate.ContainsKey(p.NameOnObject)) || p.IsPrimaryKey).ToList();
            tempSpec.Properties = filtered;

            var con = settings.Connection as SqlConnection;
            if (con.State != System.Data.ConnectionState.Open)
            {
                await con.OpenAsync();
            }

            var mergeCommand = settings.TempSettings.SqlGenerator.BuildMergeCommand(tableName, filtered, tempTableName);
            var copy = tableSpec.Copy();
            copy.Properties = filtered;
            copy.TableMapping.TableName = tempTableName;

            var tempTableCreator = settings.TempSettings.Factory.TableCreator();
            tempTableCreator.IgnoreIdentityColumns = false;
            await tempTableCreator.CreateTable(con, settings.Transaction, tempSpec);


            using (var mCommand = new SqlCommand(mergeCommand, con, settings.Transaction))
            using (var dCommand = new SqlCommand(settings.TempSettings.SqlGenerator.BuildDropStatement(schema, tempTableName), con, settings.Transaction))
            {
                await InsertItemsAsync(items, copy, settings);
                await mCommand.ExecuteNonQueryAsync();
                await dCommand.ExecuteNonQueryAsync();
            }
        }

        private static void ApplyBulkCopyOptions<T>(string schema, string tableName, IList<ColumnMapping> properties, SqlServerBulkSettings settings, EFDataReader<T> reader, SqlBulkCopy copy)
        {
            copy.BatchSize = Math.Min(reader.RecordsAffected, settings.BatchSize ?? 15000); //default batch size
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

            settings.CustomSettingsApplier?.Invoke(copy);
        }
    }
}
