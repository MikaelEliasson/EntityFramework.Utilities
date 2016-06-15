using EntityFramework.Utilities.SqlServer;
using System.Data.Entity;

namespace Tests
{
    public static class TestExtensions
    {
        public static void SetupDb(this DbContext db)
        {
            if (db.Database.Exists())
            {
                db.Database.ForceDelete();
            }
            db.Database.Create();
        }
    }
}
