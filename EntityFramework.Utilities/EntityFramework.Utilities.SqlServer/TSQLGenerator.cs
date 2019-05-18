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
            var props = string.Join(",", properties.Where(p => !p.IsStoreGenerated).Select(x => "[" + x.NameInDatabase + "]"));
            var mergeCommand = $@"INSERT INTO [{tableName}] ({props})
                OUTPUT {string.Join(", ", output)}
                SELECT {props} FROM 
                    [{tempTableName}]";
            return mergeCommand;
        }

        


        public virtual string BuildDeleteQuery(QueryInformation queryInfo)
        {
            var schema = !string.IsNullOrWhiteSpace(queryInfo.Schema) ? $"[{queryInfo.Schema}]." : "";
            return $"DELETE FROM {schema}[{queryInfo.Table}] {queryInfo.WhereSql}";
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

            var schema = !string.IsNullOrWhiteSpace(predicateQueryInfo.Schema) ? $"[{predicateQueryInfo.Schema}]." : "";
            return string.Format("UPDATE {0}[{1}] SET {2} {3}", schema, predicateQueryInfo.Table, updateSql, predicateQueryInfo.WhereSql);
        }

    }
}
