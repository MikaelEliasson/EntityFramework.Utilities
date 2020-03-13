using Microsoft.Data.SqlClient;

namespace EntityFramework.Utilities
{
    public static class DatabaseExtensionMethods
    {
        public static string GetDatabaseName(System.Data.Common.DbConnection dbConnection)
        {
            return new SqlConnectionStringBuilder(dbConnection.ConnectionString).InitialCatalog;
        }

    }
}
