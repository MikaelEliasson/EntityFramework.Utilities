using System.Collections.Generic;
using System.Linq;
using EntityFramework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.FakeDomain;
using Tests.FakeDomain.Models;

namespace Tests
{
    [TestClass]
    public class UpdateBulkTests
    {
        [TestMethod]
        public void UpdateBulk_UpdatesAll()
        {
            Setup();

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.ToList();
                foreach (var post in posts)
	            {
                    post.Title = post.Title.Replace("1", "4").Replace("2", "8").Replace("3", "12");
	            }
                EFBatchOperation.For(db, db.BlogPosts).UpdateAll(posts, spec => spec.ColumnsToUpdate(p => p.Title));
            }

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.OrderBy(b => b.ID).ToList();
                Assert.AreEqual("T4", posts[0].Title);
                Assert.AreEqual("T8", posts[1].Title);
                Assert.AreEqual("T12", posts[2].Title);
            }
        }

        [TestMethod]
        public void UpdateBulk_CanUpdateNvarcharWithLength()
        {
            Setup();

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.ToList();
                foreach (var post in posts)
                {
                    post.ShortTitle = post.Title.Replace("1", "4").Replace("2", "8").Replace("3", "12");
                }
                EFBatchOperation.For(db, db.BlogPosts).UpdateAll(posts, spec => spec.ColumnsToUpdate(p => p.ShortTitle));
            }

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.OrderBy(b => b.ID).ToList();
                Assert.AreEqual("T4", posts[0].ShortTitle);
                Assert.AreEqual("T8", posts[1].ShortTitle);
                Assert.AreEqual("T12", posts[2].ShortTitle);
            }
        }

        private static void Setup()
        {
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.ForceDelete();
                }
                db.Database.Create();

                var list = new List<BlogPost>(){
                    BlogPost.Create("T1"),
                    BlogPost.Create("T2"),
                    BlogPost.Create("T3")
                };

                EFBatchOperation.For(db, db.BlogPosts).InsertAll(list);
            }
        }
    }
}
