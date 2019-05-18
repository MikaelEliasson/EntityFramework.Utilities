using EntityFramework.Utilities.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tests.FakeDomain;
using Tests.FakeDomain.Models;
using EntityFramework.Utilities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;

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
                using (var command = db.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "SELECT ID, Title, ShortTitle, Created, Reads, Author_Name, Author_Email, Author_Address_Line1, Author_Address_ZipCode, Author_Address_Town From dummy order by Title";
                    db.Database.OpenConnection();
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();

                        Assert.AreEqual("T1", reader.GetString(1));
                        Assert.AreEqual(DateTime.Today, reader.GetDateTime(3).Date);
                        Assert.AreEqual("m@m.com", reader.GetString(6));

                        Assert.IsTrue(reader.Read()); //Check count
                        Assert.IsTrue(reader.Read());
                        Assert.IsFalse(reader.Read());
                    }
                };


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
                var copy = tableSpec.Copy();
                copy.TableMapping.TableName = tableName;
                return base.InsertItemsAsync<T>(items, copy, settings);
            }
        }
    }
}
