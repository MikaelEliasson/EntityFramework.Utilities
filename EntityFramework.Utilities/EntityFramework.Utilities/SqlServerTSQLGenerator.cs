using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntityFramework.Utilities
{
    public static class SqlServerTSQLGenerator
    {
        public static string BuildDropStatement(string schema, string tempTableName)
        {
            return string.Format("DROP table {0}.[{1}]", schema, tempTableName);
        }

        public static string BuildMergeCommand(string tableName, IList<ColumnMapping> properties, string tempTableName)
        {
            var setters = string.Join(",", properties.Where(c => !c.IsPrimaryKey).Select(c => "[" + c.NameInDatabase + "] = TEMP.[" + c.NameInDatabase + "]"));
            var pks = properties.Where(p => p.IsPrimaryKey).Select(x => "ORIG.[" + x.NameInDatabase + "] = TEMP.[" + x.NameInDatabase + "]");
            var filter = string.Join(",", pks);
            var mergeCommand = string.Format(@"UPDATE [{0}]
                SET
                    {3}
                FROM
                    [{0}] ORIG
                INNER JOIN
                     [{1}] TEMP
                ON 
                    {2}", tableName, tempTableName, filter, setters);
            return mergeCommand;
        }

        public static string BuildSelectIntoCommand(string tableName, IList<ColumnMapping> properties, string tempTableName)
        {
            var output = properties.Where(p => p.IsStoreGenerated).Select(x => "INSERTED.[" + x.NameInDatabase + "]");
            var mergeCommand = string.Format(@"INSERT INTO [{0}]
                OUTPUT {2}
                SELECT * FROM 
                    [{1}]", tableName, tempTableName, string.Join(", ", output));
            return mergeCommand;
        }

        public static string BuildCreateTableCommand(string schema, string tempTableName, IEnumerable<ColumnMapping> properties)
        {
            var pkConstraint = string.Join(", ", properties.Where(p => p.IsPrimaryKey).Select(c => "[" + c.NameInDatabase + "]"));
            var columns = properties.Select(c => "[" + c.NameInDatabase + "] " + c.DataType);
            var pk = properties.Where(p => p.IsPrimaryKey).Any() ? string.Format(", PRIMARY KEY ({0})", pkConstraint) : "";

            var str = string.Format("CREATE TABLE {0}.[{1}]({2} {3})", schema, tempTableName, string.Join(", ", columns), pk);

            return str;
        }
    }
}
