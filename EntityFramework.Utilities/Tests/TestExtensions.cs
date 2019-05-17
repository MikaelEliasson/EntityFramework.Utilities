using EntityFramework.Utilities.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public static class TestExtensions
    {
        public static void SetupDb(this DbContext db)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}
