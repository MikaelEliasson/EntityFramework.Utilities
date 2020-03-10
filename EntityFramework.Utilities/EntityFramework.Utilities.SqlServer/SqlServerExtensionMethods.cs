using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace EntityFramework.Utilities.SqlServer
{
    public static class SqlServerExtensionMethods
    {
        /// <summary>
        /// Deletes the database even if there are open connections. Like from Management Studio for example
        /// </summary>
        /// <param name="db"></param>
        /// <param name="name">The name of the database to drop. Should normally not be needed as that is read from the connection string</param>
        public static void ForceDelete(this DbContext db, string name = null)
        {
            name = name ?? db.Database.GetDbConnection().Database;
            using (SqlConnection sqlconnection = new SqlConnection(db.Database.GetDbConnection().ConnectionString)) //Need to run this under other transaction
            {
                sqlconnection.Open();
                // if you used master db as Initial Catalog, there is no need to change database
                sqlconnection.ChangeDatabase("master");

                string rollbackCommand = @"ALTER DATABASE [" + name + "] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE;";

                SqlCommand deletecommand = new SqlCommand(rollbackCommand, sqlconnection);

                deletecommand.ExecuteNonQuery();

                string deleteCommand = @"DROP DATABASE [" + name + "];";

                deletecommand = new SqlCommand(deleteCommand, sqlconnection);

                deletecommand.ExecuteNonQuery();
            }
        }

    }
}
