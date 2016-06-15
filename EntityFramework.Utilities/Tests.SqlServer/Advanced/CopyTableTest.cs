using EntityFramework.Utilities;
using EntityFramework.Utilities.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Tests.FakeDomain;
using Tests.FakeDomain.Models;

namespace Tests.SqlServer.Advanced
{
    [TestClass]
    public class CopyTableTest
    {
        [TestMethod]
        public void CopyNormalTable()
        {
            using (var db = Context.Sql())
            {
                db.SetupDb();

                var tableSpec = BulkTableSpec.Get<BlogPost, BlogPost>(db);

                var sql = new TableCreator().BuildCreateTableCommand(tableSpec.TableMapping.Schema, "tempTable111", tableSpec.Properties);
                db.Database.ExecuteSqlCommand(sql);

                var oldColumns = db.Database.SqlQuery<string>("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('" + tableSpec.TableMapping.TableName + "')").ToList();
                var newColumns = db.Database.SqlQuery<string>("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('tempTable111')").ToList();

                CollectionAssert.AreEqual(oldColumns.OrderBy(s => s).ToList(), newColumns.OrderBy(s => s).ToList());
            }
        }


        [TestMethod]
        public void CopyTphTable()
        {
            using (var db = Context.Sql())
            {
                db.SetupDb();

                var tableSpec = BulkTableSpec.Get<Contact, Person>(db);
                var sql = new TableCreator().BuildCreateTableCommand(tableSpec.TableMapping.Schema, "tempTable222", tableSpec.Properties);
                db.Database.ExecuteSqlCommand(sql);

                var oldColumns = db.Database.SqlQuery<string>("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('" + tableSpec.TableMapping.TableName + "')").ToList();
                var newColumns = db.Database.SqlQuery<string>("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('tempTable222')").ToList();

                CollectionAssert.AreEqual(oldColumns.OrderBy(s => s).ToList(), newColumns.OrderBy(s => s).ToList());
            }
        }
    }
}
