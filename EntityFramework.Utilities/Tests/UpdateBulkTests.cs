using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.FakeDomain;
using Tests.FakeDomain.Models;
using System.Threading.Tasks;
using EntityFramework.Utilities.SqlServer;

namespace Tests
{
    [TestClass]
    public class UpdateBulkTests
    {
        [TestMethod]
        public async Task UpdateBulk_UpdatesAll()
        {
            await Setup();

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.ToList();
                foreach (var post in posts)
	            {
                    post.Title = post.Title.Replace("1", "4").Replace("2", "8").Replace("3", "12");
	            }
                await EFBatchOperation.For(db, db.BlogPosts).UpdateAllAsync(posts, spec => spec.ColumnsToUpdate(p => p.Title));
            }

            using (var db = Context.Sql())
            {
                var posts = db.BlogPosts.OrderBy(b => b.ID).ToList();
                Assert.AreEqual("T4", posts[0].Title);
                Assert.AreEqual("T8", posts[1].Title);
                Assert.AreEqual("T12", posts[2].Title);
            }
        }

        private static async Task Setup()
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

                await EFBatchOperation.For(db, db.BlogPosts).InsertAllAsync(list);
            }
        }
    }
}
