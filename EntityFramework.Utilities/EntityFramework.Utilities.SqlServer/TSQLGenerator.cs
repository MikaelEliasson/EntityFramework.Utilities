using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EntityFramework.Utilities
{
    public class TSQLGenerator
    {
        public virtual string BuildDropStatement(string schema, string tempTableName)
        {
            return string.Format("DROP table {0}.[{1}]", schema, tempTableName);
        }

        public virtual string BuildMergeCommand(string tableName, IList<ColumnMapping> properties, string tempTableName)
        {
            var setters = string.Join(",", properties.Where(c => !c.IsPrimaryKey).Select(c => "[" + c.NameInDatabase + "] = TEMP.[" + c.NameInDatabase + "]"));
            var pks = properties.Where(p => p.IsPrimaryKey).Select(x => "ORIG.[" + x.NameInDatabase + "] = TEMP.[" + x.NameInDatabase + "]");
            var filter = string.Join(" and ", pks);
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

        public virtual string BuildSelectIntoCommand(string tableName, IList<ColumnMapping> properties, string tempTableName)
        {
            var output = properties.Where(p => p.IsStoreGenerated).Select(x => "INSERTED.[" + x.NameInDatabase + "]");
            var mergeCommand = string.Format(@"INSERT INTO [{0}]
                OUTPUT {2}
                SELECT * FROM 
                    [{1}]", tableName, tempTableName, string.Join(", ", output));
            return mergeCommand;
        }

        


        public virtual string BuildDeleteQuery(QueryInformation queryInfo)
        {
            return string.Format("DELETE FROM [{0}].[{1}] {2}", queryInfo.Schema, queryInfo.Table, queryInfo.WhereSql);
        }

        public virtual string BuildUpdateQuery(QueryInformation predicateQueryInfo, QueryInformation modificationQueryInfo)
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
                updateSql = string.Join(" = ", update.Split(new string[] { " = " }, StringSplitOptions.RemoveEmptyEntries).Reverse());
            }


            return string.Format("UPDATE [{0}].[{1}] SET {2} {3}", predicateQueryInfo.Schema, predicateQueryInfo.Table, updateSql, predicateQueryInfo.WhereSql);
        }

    }
}
