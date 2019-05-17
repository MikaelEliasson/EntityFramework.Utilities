using EntityFramework.Utilities.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tests.FakeDomain;
using Tests.FakeDomain.Models;
using EntityFramework.Utilities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
namespace Tests.SqlServer.Advanced
{
    [TestClass]
    public class InsertIntoTempTableTest
    {
        [TestMethod]
        public async Task InsertIntoTempTable()
        {
            using (var db = Context.Sql())
            {
                db.SetupDb();

                var list = new List<BlogPost>(){
                    BlogPost.Create("T1"),
                    BlogPost.Create("T2"),
                    BlogPost.Create("T3")
                };

                var tableSpec = BulkTableSpec.Get<BlogPost, BlogPost>(db);
                var creator = new TableCreator();
                creator.IgnoreIdentity = false;
                creator.IgnoreIdentityColumns = false;
                var sql = creator.BuildCreateTableCommand(tableSpec.TableMapping.Schema, "dummy", tableSpec.Properties);
                db.Database.ExecuteSqlCommand(sql);

                var s = new SqlServerBulkSettings();
                s.Factory.Inserter = () => new TempTableInserter("dummy");
                await EFBatchOperation.For(db, db.BlogPosts).InsertAllAsync(list, s);
            }

            using (var db = Context.Sql())
            {
                Assert.AreEqual(0, db.BlogPosts.Count());
                var blogposts = db.BlogPosts.FromSql("select * from dummy").ToList();
                Assert.AreEqual(3, blogposts.Count);
                Assert.AreEqual("m@m.com", blogposts.First().Author.Email);
            }
        }

        private class TempTableInserter : BulkInserter
        {
            private string tableName;

            public TempTableInserter(string tableName)
            {
                this.tableName = tableName;
            }

            public override Task InsertItemsAsync<T>(IEnumerable<T> items, BulkTableSpec tableSpec, SqlServerBulkSettings settings)
            {
                tableSpec.TableMapping.TableName = tableName;
                return base.InsertItemsAsync<T>(items, tableSpec, settings);
            }
        }
    }
}
