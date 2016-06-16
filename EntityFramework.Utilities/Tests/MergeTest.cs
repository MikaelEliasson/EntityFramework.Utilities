using System.Collections.Generic;
using System.Linq;
using EntityFramework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.FakeDomain;
using Tests.FakeDomain.Models;

namespace Tests
{
    [TestClass]
    public class MergeTest
    {
        [TestMethod]
        public void Merge_Default()
        {
            Setup();

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.ToList();
                foreach (var post in posts)
                {
                    post.Title = post.Title.Replace("1", "4").Replace("2", "8").Replace("3", "12");
                }
                var insert = BlogPost.Create("TNew");
                posts.Add(insert);
                EFBatchOperation.For(db, db.BlogPosts).MergeAll(posts);
            }

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.OrderBy(b => b.ID).ToList();
                Assert.AreEqual("T4", posts[0].Title);
                Assert.AreEqual("T8", posts[1].Title);
                Assert.AreEqual("T12", posts[2].Title);
                Assert.AreEqual("TNew", posts[3].Title);
            }
        }

        [TestMethod]
        public void Merge_With_Condition()
        {
            Setup();

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.ToList();
                foreach (var post in posts)
                {
                    post.Title = post.Title.Replace("1", "4").Replace("2", "8").Replace("3", "12");
                }
                var insert = BlogPost.Create("TNew");
                posts.Add(insert);
                EFBatchOperation.For(db, db.BlogPosts).MergeAll(posts, c => c.ColumnsToIdentity(p => p.ID));
            }

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.OrderBy(b => b.ID).ToList();
                Assert.AreEqual("T4", posts[0].Title);
                Assert.AreEqual("T8", posts[1].Title);
                Assert.AreEqual("T12", posts[2].Title);
                Assert.AreEqual("TNew", posts[3].Title);
            }
        }

        [TestMethod]
        public void Merge_With_Specific_Update_Column()
        {
            Setup();

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.ToList();
                foreach (var post in posts)
                {
                    post.Title = post.Title.Replace("1", "4").Replace("2", "8").Replace("3", "12");
                    post.Reads = 99;
                }
                var insert = BlogPost.Create("TNew");
                insert.Reads = 99;
                posts.Add(insert);
                EFBatchOperation.For(db, db.BlogPosts).MergeAll(posts, null,c=>c.ColumnsToUpdate(p=>p.Reads));
            }

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.OrderBy(b => b.ID).ToList();
                posts.ForEach(p => Assert.AreEqual(99, p.Reads));
                Assert.AreEqual("T1", posts[0].Title);
                Assert.AreEqual("T2", posts[1].Title);
                Assert.AreEqual("T3", posts[2].Title);
                Assert.AreEqual("TNew", posts[3].Title);
            }
        }

        [TestMethod]
        public void Merge_With_Condition_And_Specific_Update_Column()
        {
            Setup();

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.ToList();
                foreach (var post in posts)
                {
                    post.Title = post.Title.Replace("1", "4").Replace("2", "8").Replace("3", "12");
                    post.Reads = 99;
                }
                EFBatchOperation.For(db, db.BlogPosts).MergeAll(posts,
                    c => c.ColumnsToIdentity(p => p.Title), 
                    c => c.ColumnsToUpdate(p => p.Reads));
            }

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.ToList();
                Assert.IsFalse(posts.Where(p => p.Reads == 99).Select(p => p.Title).Except(new[] { "T4", "T8", "T12" }).Any());
                Assert.IsFalse(posts.Where(p => p.Reads == 0).Select(p => p.Title).Except(new[] { "T1", "T2", "T3" }).Any());
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
