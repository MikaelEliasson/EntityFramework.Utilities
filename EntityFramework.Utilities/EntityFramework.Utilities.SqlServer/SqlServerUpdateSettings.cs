namespace EntityFramework.Utilities.SqlServer
{
    public class SqlServerUpdateSettings
    {
        public SqlServerUpdateSettings()
        {
            Analyzer = new SqlQueryAnalyzer();
            SqlGenerator = new TSQLGenerator();
        }

        public SqlQueryAnalyzer Analyzer { get; private set; }
        public TSQLGenerator SqlGenerator { get; private set; }
    }
}
