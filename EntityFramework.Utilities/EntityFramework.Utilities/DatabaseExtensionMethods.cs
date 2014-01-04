using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace EntityFramework.Utilities
{
    public static class DatabaseExtensionMethods
    {
        public static void ForceDrop(this Database db, string name = null)
        {
            name = name ?? GetDatabaseName(db.Connection);
            using (SqlConnection sqlconnection = new SqlConnection(db.Connection.ConnectionString)) //Need to run this under other transaction
            {
                sqlconnection.Open();
                // if you used master db as Initial Catalog, there is no need to change database
                sqlconnection.ChangeDatabase("master");

                string rollbackCommand = @"ALTER DATABASE " + name + " SET  SINGLE_USER WITH ROLLBACK IMMEDIATE;";

                SqlCommand deletecommand = new SqlCommand(rollbackCommand, sqlconnection);

                deletecommand.ExecuteNonQuery();

                string deleteCommand = @"DROP DATABASE " + name + ";";

                deletecommand = new SqlCommand(deleteCommand, sqlconnection);

                deletecommand.ExecuteNonQuery();
            }
        }

        public static string GetDatabaseName(System.Data.Common.DbConnection dbConnection)
        {
            return new SqlConnectionStringBuilder(dbConnection.ConnectionString).InitialCatalog;
        }

    }
}
