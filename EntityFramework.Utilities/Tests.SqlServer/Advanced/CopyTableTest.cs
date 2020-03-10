using EntityFramework.Utilities;
using EntityFramework.Utilities.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Tests.FakeDomain;
using Tests.FakeDomain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;

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

                var oldColumns = GetColumns(db, tableSpec.TableMapping.TableName);
                var newColumns = GetColumns(db, "tempTable111");

                CollectionAssert.AreEqual(oldColumns.OrderBy(s => s).ToList(), newColumns.OrderBy(s => s).ToList());
            }
        }


        [TestMethod]
        [Ignore]
        public void CopyTphTable()
        {
            using (var db = Context.Sql())
            {
                db.SetupDb();

                var tableSpec = BulkTableSpec.Get<Contact, Person>(db);
                var sql = new TableCreator().BuildCreateTableCommand(tableSpec.TableMapping.Schema, "tempTable222", tableSpec.Properties);
                db.Database.ExecuteSqlCommand(sql);

                var oldColumns = GetColumns(db, tableSpec.TableMapping.TableName);
                var newColumns = GetColumns(db, "tempTable222");

                CollectionAssert.AreEqual(oldColumns.OrderBy(s => s).ToList(), newColumns.OrderBy(s => s).ToList());
            }
        }

        private List<string> GetColumns(DbContext db, string tableName)
        {
            var con = db.Database.GetDbConnection() as SqlConnection;
            if(con.State == ConnectionState.Closed)
            {
                con.Open();
            }

            var cmd = new SqlCommand("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID(@tableName)", con);
            cmd.Parameters.AddWithValue("tableName", tableName);

            var reader = cmd.ExecuteReader();
            var list = new List<string>();
            while (reader.Read())
            {
                list.Add(reader.GetString(0));
            }
            return list;
        }
    }
}
