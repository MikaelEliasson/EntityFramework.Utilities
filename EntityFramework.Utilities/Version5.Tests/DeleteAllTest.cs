using System;
using System.Linq;
using EntityFramework.Utilities;
using EntityFramework.Utilities.Version4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Version5.Tests.FakeDomain;

namespace Version5.Tests
{
    [TestClass]
    public class DeleteAllTest
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
                count = db.DeleteAll<BlogPost>(b => b.Title == "T2");
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
                count = db.DeleteAll<BlogPost>(b => b.Created < limit);
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
                count = db.DeleteAll<BlogPost>(b => b.Created < upper && b.Created > lower);
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
                count = db.DeleteAll<BlogPost>(b => b.Created < upper && b.Created > lower && b.Title == "T2.0");
                Assert.AreEqual(1, count);
            }

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.ToList();
                Assert.AreEqual(3, posts.Count);
                Assert.AreEqual(0, posts.Count(p => p.Title == "T2.0"));
            }
        }

    }
}
