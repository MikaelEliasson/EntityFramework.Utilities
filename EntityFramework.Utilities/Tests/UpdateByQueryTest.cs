using System.Linq;
using EntityFramework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.FakeDomain;
using System;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using Tests.FakeDomain.Models;

namespace Tests
{
    [TestClass]
    public class UpdateByQueryTest
    {

        [TestMethod]
        public void UpdateAll_Increment()
        {
            SetupBasePosts();

            int count;
            using (var db = Context.Sql())
            {
                count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").Update(b => b.Reads, b => b.Reads + 5);
                Assert.AreEqual(1, count);
            }

            using (var db = Context.Sql())
            {
                var post = db.BlogPosts.First(p => p.Title == "T2");
                Assert.AreEqual(5, post.Reads);
            }
        }

        [TestMethod]
        public void UpdateAll_Set()
        {
            SetupBasePosts();

            int count;
            using (var db = Context.Sql())
            {
                count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").Update(b => b.Reads, b => 10);
                Assert.AreEqual(1, count);
            }

            using (var db = Context.Sql())
            {
                var post = db.BlogPosts.First(p => p.Title == "T2");
                Assert.AreEqual(10, post.Reads);
            }
        }

        [TestMethod]
        public void UpdateAll_SetFromVariable()
        {
            SetupBasePosts();

            int count;
            using (var db = Context.Sql())
            {
                int reads = 20;
                count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").Update(b => b.Reads, b => reads);
                Assert.AreEqual(1, count);
            }

            using (var db = Context.Sql())
            {
                var post = db.BlogPosts.First(p => p.Title == "T2");
                Assert.AreEqual(20, post.Reads);
            }
        }
        private int Get20()
        {
            return 20;
        }
        [TestMethod]
        [Ignore]
        public void UpdateAll_SetFromMethod()
        {
            SetupBasePosts();

            int count;
            using (var db = Context.Sql())
            {
                count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").Update(b => b.Reads, b => Get20());
                Assert.AreEqual(1, count);
            }

            using (var db = Context.Sql())
            {
                var post = db.BlogPosts.First(p => p.Title == "T2");
                Assert.AreEqual(20, post.Reads);
            }
        }

        [TestMethod]
        [Ignore]
        public void UpdateAll_SetFromProperty()
        {
            SetupBasePosts();

            int count;
            using (var db = Context.Sql())
            {
                count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Created == DateTime.Now.AddDays(2)).Update(b => b.Created, b => DateTime.Now);
                Assert.AreEqual(1, count);
            }
        }

        [TestMethod]
        [Ignore]
        public void UpdateAll_ConcatStringValue()
        {
            SetupBasePosts();

            int count;
            using (var db = Context.Sql())
            {
                count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").Update(b => b.Title, b => b.Title + ".0");
                Assert.AreEqual(1, count);
            }

            using (var db = Context.Sql())
            {
                Assert.AreEqual(1, db.BlogPosts.Count(p => p.Title == "T2.0"));
                Assert.AreEqual(0, db.BlogPosts.Count(p => p.Title == "T2"));
            }
        }

        [TestMethod]
        public void UpdateAll_SetDateTimeValueFromVariable()
        {
            SetupBasePosts();

            int count;
            using (var db = Context.Sql())
            {
                count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").Update(b => b.Created, b => DateTime.Today);
                Assert.AreEqual(1, count);
            }

            using (var db = Context.Sql())
            {
                var post = db.BlogPosts.First(p => p.Title == "T2");
                Assert.AreEqual(DateTime.Today, post.Created);
            }
        }

        [TestMethod]
        public void UpdateAll_Decrement()
        {
            SetupBasePosts();

            int count;
            using (var db = Context.Sql())
            {
                count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").Update(b => b.Reads, b => b.Reads - 5);
                Assert.AreEqual(1, count);
            }

            using (var db = Context.Sql())
            {
                var post = db.BlogPosts.First(p => p.Title == "T2");
                Assert.AreEqual(-5, post.Reads);
            }
        }

        [TestMethod]
        public void UpdateAll_Multiply()
        {
            SetupBasePosts();

            int count;
            using (var db = Context.Sql())
            {
                count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T1").Update(b => b.Reads, b => b.Reads * 2);
                Assert.AreEqual(1, count);
            }

            using (var db = Context.Sql())
            {
                var post = db.BlogPosts.First(p => p.Title == "T1");
                Assert.AreEqual(4, post.Reads);
            }
        }

        [TestMethod]
        public void UpdateAll_Divide()
        {
            SetupBasePosts();

            int count;
            using (var db = Context.Sql())
            {
                count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T1").Update(b => b.Reads, b => b.Reads / 2);
                Assert.AreEqual(1, count);
            }

            using (var db = Context.Sql())
            {
                var post = db.BlogPosts.First(p => p.Title == "T1");
                Assert.AreEqual(1, post.Reads);
            }
        }

        [TestMethod]
        public void UpdateAll_NoProvider_UsesDefaultDelete()
        {
            string fallbackText = null;
            Configuration.DisableDefaultFallback = false;
            Configuration.Log = str => fallbackText = str;

            using (var db = Context.SqlCe())
            {
                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.Create();

                db.BlogPosts.Add(BlogPost.Create("T1", DateTime.Today.AddDays(-2)));
                db.BlogPosts.Add(BlogPost.Create("T2", DateTime.Today.AddDays(0)));
                db.BlogPosts.Add(BlogPost.Create("T3", DateTime.Today.AddDays(2)));

                db.SaveChanges();
            }

            using (var db = Context.SqlCe())
            {
                var lower = DateTime.Today.AddDays(-1);
                var upper = DateTime.Today.AddDays(1);

                var count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").Update(b => b.Title, b => b.Title + ".0");
                Assert.AreEqual(1, count);
            }

            using (var db = Context.SqlCe())
            {
                Assert.AreEqual(1, db.BlogPosts.Count(p => p.Title == "T2.0"));
                Assert.AreEqual(0, db.BlogPosts.Count(p => p.Title == "T2"));
            }

            Assert.IsNotNull(fallbackText);
        }

        private static void SetupBasePosts()
        {
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.Create();

                var p = BlogPost.Create("T1");
                p.Reads = 2;
                db.BlogPosts.Add(p);
                db.BlogPosts.Add(BlogPost.Create("T2"));

                db.SaveChanges();
            }
        }

    }
}
