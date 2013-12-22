﻿using System.Collections.Generic;
using System.Linq;
using EntityFramework.Utilities;
using EntityFramework.Utilities.Version6.EntityFramework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Version6.Tests.FakeDomain;

namespace Version6.Tests
{
    [TestClass]
    public class InsertTests 
    {
        [TestMethod]
        public void InsertAll_InsertsItems()
        {
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.Create();

                var list = new List<BlogPost>(){
                    BlogPost.Create("T1"),
                    BlogPost.Create("T2"),
                    BlogPost.Create("T3")
                };

                db.InsertAll(list);
            }

            using (var db = Context.Sql())
            {
                Assert.AreEqual(3, db.BlogPosts.Count());
            }
        }

        [TestMethod]
        public void InsertAll_WrongColumnOrder_InsertsItems()
        {
            using (var db = new ReorderedContext())
            {
                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.Create();
            }

            using (var db = Context.Sql())
            {

                var list = new List<BlogPost>(){
                    BlogPost.Create("T1"),
                    BlogPost.Create("T2"),
                    BlogPost.Create("T3")
                };

                db.InsertAll(list);
            }

            using (var db = Context.Sql())
            {
                Assert.AreEqual(3, db.BlogPosts.Count());
            }
        }

        [TestMethod]
        public void InsertAll_WrongColumnOrderAndRenamedColumn_InsertsItems()
        {
            using (var db = new RenamedAndReorderedContext())
            {
                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.Create();
                db.Database.ExecuteSqlCommand("drop table dbo.RenamedAndReorderedBlogPosts;");
                db.Database.ExecuteSqlCommand(RenamedAndReorderedBlogPost.CreateTableSql());
            }

            using (var db = new RenamedAndReorderedContext())
            {

                var list = new List<RenamedAndReorderedBlogPost>(){
                    RenamedAndReorderedBlogPost.Create("T1"),
                    RenamedAndReorderedBlogPost.Create("T2"),
                    RenamedAndReorderedBlogPost.Create("T3")
                };

                db.InsertAll(list);
            }

            using (var db = new RenamedAndReorderedContext())
            {
                Assert.AreEqual(3, db.BlogPosts.Count());
            }
        }

        [TestMethod]
        public void InsertAll_NoProvider_UsesDefaultInsert()
        {
            Configuration.Providers.Clear();
            string fallbackText = null;

            Configuration.Log = str => fallbackText = str;

            using (var db = Context.SqlCe())
            {
                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.Create();
            }

            using (var db = Context.SqlCe())
            {

                var list = new List<BlogPost>(){
                    BlogPost.Create("T1"),
                    BlogPost.Create("T2"),
                    BlogPost.Create("T3")
                };

                db.InsertAll(list);
            }

            using (var db = Context.SqlCe())
            {
                Assert.AreEqual(3, db.BlogPosts.Count());
            }

            Assert.IsNotNull(fallbackText);
        }
    }
}
