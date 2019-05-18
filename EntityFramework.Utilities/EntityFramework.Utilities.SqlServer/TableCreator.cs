using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Utilities.SqlServer
{
    public class TableCreator
    {
        /// <summary>
        /// Create the table without Identity column even if the mappings specify it. Default: true
        /// </summary>
        public bool IgnoreIdentity { get; set; } = true;
        /// <summary>
        /// Skips creating PK columns
        /// </summary>
        public bool IgnoreIdentityColumns { get; set; } = true;
        public string IdentityDefinition { get; set; } = "identity(1,1)";

        public virtual async Task CreateTable(SqlConnection connection, SqlTransaction transaction, BulkTableSpec spec)
        {
            var sql = BuildCreateTableCommand(spec.TableMapping.Schema, spec.TableMapping.TableName, spec.Properties.Where(p => !(IgnoreIdentityColumns && p.IsStoreGenerated)));

            using (var createCommand = new SqlCommand(sql, connection, transaction))
            {
                await createCommand.ExecuteNonQueryAsync();
            }
        }

        public virtual string BuildCreateTableCommand(string schema, string tempTableName, IEnumerable<ColumnMapping> properties)
        {
            var pkConstraint = string.Join(", ", properties.Where(p => p.IsPrimaryKey).Select(c => "[" + c.NameInDatabase + "]"));
            Func<ColumnMapping, string> getType = c => (c.StaticValue != null ? "[nvarchar(128)]" : c.DataType);
            Func<ColumnMapping, string> getIdentity = c => (IgnoreIdentity || !c.IsStoreGenerated ? "" : IdentityDefinition);
            var columns = properties.Select(c => $"[{c.NameInDatabase}] {getType(c)} {getIdentity(c)}");
            var pk = properties.Where(p => p.IsPrimaryKey).Any() ? string.Format(", PRIMARY KEY ({0})", pkConstraint) : "";

            var str = string.Format("CREATE TABLE {0}.[{1}]({2} {3})", schema, tempTableName, string.Join(", ", columns), pk);

            return str;
        }
    }
}
