using System;
using System.Linq;
using EntityFramework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.FakeDomain;
using System.Collections.Generic;
using Tests.FakeDomain.Models;

namespace Tests
{
    [TestClass]
    public class DeleteByQueryTest
    {
        [TestMethod]
        public void DeleteAll_PropertyEquals_DeletesAllMatchesAndNothingElse()
        {
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.Create();

                db.BlogPosts.Add(BlogPost.Create("T1")); 
                db.BlogPosts.Add(BlogPost.Create("T2"));
                db.BlogPosts.Add(BlogPost.Create("T2")); 
                db.BlogPosts.Add(BlogPost.Create("T3"));

                db.SaveChanges();
            }

            int count;
            using (var db = Context.Sql())
            {
                count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").Delete();
                Assert.AreEqual(2, count);
            }

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.ToList();
                Assert.AreEqual(2, posts.Count);
                Assert.AreEqual(0, posts.Count(p => p.Title == "T2"));
            }
        }

        [TestMethod]
        public void DeleteAll_DateIsSmallerThan_DeletesAllMatchesAndNothingElse()
        {
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.Create();

                db.BlogPosts.Add(BlogPost.Create("T1", DateTime.Today.AddDays(-2)));
                db.BlogPosts.Add(BlogPost.Create("T2", DateTime.Today.AddDays(-1)));
                db.BlogPosts.Add(BlogPost.Create("T3", DateTime.Today.AddDays(1)));

                db.SaveChanges();
            }

            int count;
            using (var db = Context.Sql())
            {
                var limit = DateTime.Today;
                count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Created < limit).Delete();
                Assert.AreEqual(2, count);
            }

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.ToList();
                Assert.AreEqual(1, posts.Count);
                Assert.AreEqual("T3", posts.First().Title);

            }
        }

        [TestMethod]
        public void DeleteAll_DateIsInRange_DeletesAllMatchesAndNothingElse()
        {
            using (var db = Context.Sql())
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

            int count;
            using (var db = Context.Sql())
            {
                var lower = DateTime.Today.AddDays(-1);
                var upper = DateTime.Today.AddDays(1);
                count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Created < upper && b.Created > lower).Delete();
                Assert.AreEqual(1, count);
            }

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.ToList();
                Assert.AreEqual(2, posts.Count);
                Assert.AreEqual(0, posts.Count(p => p.Title == "T2"));
            }
        }

        [TestMethod]
        public void DeleteAll_DateIsInRangeAndTitleEquals_DeletesAllMatchesAndNothingElse()
        {
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.Create();

                db.BlogPosts.Add(BlogPost.Create("T1", DateTime.Today.AddDays(-2)));
                db.BlogPosts.Add(BlogPost.Create("T2.0", DateTime.Today.AddDays(0)));
                db.BlogPosts.Add(BlogPost.Create("T2.1", DateTime.Today.AddDays(0)));
                db.BlogPosts.Add(BlogPost.Create("T3", DateTime.Today.AddDays(2)));

                db.SaveChanges();
            }

            int count;
            using (var db = Context.Sql())
            {
                var lower = DateTime.Today.AddDays(-1);
                var upper = DateTime.Today.AddDays(1);

                count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Created < upper && b.Created > lower && b.Title == "T2.0").Delete();
                Assert.AreEqual(1, count);
            }

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.ToList();
                Assert.AreEqual(3, posts.Count);
                Assert.AreEqual(0, posts.Count(p => p.Title == "T2.0"));
            }
        }

        [TestMethod]
        public void DeleteAll_NoProvider_UsesDefaultDelete()
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
                db.BlogPosts.Add(BlogPost.Create("T2.0", DateTime.Today.AddDays(0)));
                db.BlogPosts.Add(BlogPost.Create("T2.1", DateTime.Today.AddDays(0)));
                db.BlogPosts.Add(BlogPost.Create("T3", DateTime.Today.AddDays(2)));

                db.SaveChanges();
            }

            using (var db = Context.SqlCe())
            {
                var lower = DateTime.Today.AddDays(-1);
                var upper = DateTime.Today.AddDays(1);

                var count = EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Created < upper && b.Created > lower && b.Title == "T2.0").Delete();
                Assert.AreEqual(1, count);
            }

            using (var db = Context.SqlCe())
            {
                Assert.AreEqual(3, db.BlogPosts.Count());
            }

            Assert.IsNotNull(fallbackText);
        }

    }
}
