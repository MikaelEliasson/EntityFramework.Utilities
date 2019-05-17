using System.Text.RegularExpressions;

namespace EntityFramework.Utilities.SqlServer
{
    public class SqlQueryAnalyzer
    {
        //public QueryInformation Analyze<T>(ObjectQuery<T> query) where T : class
        //{
        //    var fromRegex = new Regex(@"FROM \[([^\]]+)\]\.\[([^\]]+)\] AS (\[[^\]]+\])", RegexOptions.IgnoreCase);

        //    var queryInfo = new QueryInformation();

        //    var str = query.ToTraceString();
        //    var match = fromRegex.Match(str);
        //    queryInfo.Schema = match.Groups[1].Value;
        //    queryInfo.Table = match.Groups[2].Value;
        //    queryInfo.Alias = match.Groups[3].Value;

        //    var i = str.IndexOf("WHERE");
        //    if (i > 0)
        //    {
        //        var whereClause = str.Substring(i);
        //        queryInfo.WhereSql = whereClause.Replace(queryInfo.Alias + ".", "");
        //    }
        //    return queryInfo;
        //}
    }
}
