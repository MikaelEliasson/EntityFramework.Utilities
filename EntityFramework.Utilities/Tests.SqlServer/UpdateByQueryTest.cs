using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.FakeDomain;
using System;
using Tests.FakeDomain.Models;
using System.Threading.Tasks;
using EntityFramework.Utilities.SqlServer;

namespace Tests
{
    [TestClass]
    [Ignore] // TODO: Full EF Core 3 support
    public class UpdateByQueryTest
    {

        [TestMethod]
        public async Task UpdateAll_Increment()
        {
            //SetupBasePosts();

            //int count;
            //using (var db = Context.Sql())
            //{
            //    count = await EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").UpdateAsync(b => b.Reads, b => b.Reads + 5);
            //    Assert.AreEqual(1, count);
            //}

            //using (var db = Context.Sql())
            //{
            //    var post = db.BlogPosts.First(p => p.Title == "T2");
            //    Assert.AreEqual(5, post.Reads);
            //}
        }

        [TestMethod]
        public async Task UpdateAll_Set()
        {
            //SetupBasePosts();

            //int count;
            //using (var db = Context.Sql())
            //{
            //    count = await EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").UpdateAsync(b => b.Reads, b => 10);
            //    Assert.AreEqual(1, count);
            //}

            //using (var db = Context.Sql())
            //{
            //    var post = db.BlogPosts.First(p => p.Title == "T2");
            //    Assert.AreEqual(10, post.Reads);
            //}
        }

        [TestMethod]
        public async Task UpdateAll_SetFromVariable()
        {
            //SetupBasePosts();

            //int count;
            //using (var db = Context.Sql())
            //{
            //    int reads = 20;
            //    count = await EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").UpdateAsync(b => b.Reads, b => reads);
            //    Assert.AreEqual(1, count);
            //}

            //using (var db = Context.Sql())
            //{
            //    var post = db.BlogPosts.First(p => p.Title == "T2");
            //    Assert.AreEqual(20, post.Reads);
            //}
        }
        private int Get20()
        {
            return 20;
        }
        [TestMethod]
        [Ignore]
        public async Task UpdateAll_SetFromMethod()
        {
            //SetupBasePosts();

            //int count;
            //using (var db = Context.Sql())
            //{
            //    count = await EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").UpdateAsync(b => b.Reads, b => Get20());
            //    Assert.AreEqual(1, count);
            //}

            //using (var db = Context.Sql())
            //{
            //    var post = db.BlogPosts.First(p => p.Title == "T2");
            //    Assert.AreEqual(20, post.Reads);
            //}
        }

        [TestMethod]
        [Ignore]
        public async Task UpdateAll_SetFromProperty()
        {
            //SetupBasePosts();

            //int count;
            //using (var db = Context.Sql())
            //{
            //    count = await EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Created == DateTime.Now.AddDays(2)).UpdateAsync(b => b.Created, b => DateTime.Now);
            //    Assert.AreEqual(1, count);
            //}
        }

        [TestMethod]
        [Ignore]
        public async Task UpdateAll_ConcatStringValue()
        {
            //SetupBasePosts();

            //int count;
            //using (var db = Context.Sql())
            //{
            //    count = await EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").UpdateAsync(b => b.Title, b => b.Title + ".0");
            //    Assert.AreEqual(1, count);
            //}

            //using (var db = Context.Sql())
            //{
            //    Assert.AreEqual(1, db.BlogPosts.Count(p => p.Title == "T2.0"));
            //    Assert.AreEqual(0, db.BlogPosts.Count(p => p.Title == "T2"));
            //}
        }

        [TestMethod]
        public async Task UpdateAll_SetDateTimeValueFromVariable()
        {
            //SetupBasePosts();

            //int count;
            //using (var db = Context.Sql())
            //{
            //    count = await EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").UpdateAsync(b => b.Created, b => DateTime.Today);
            //    Assert.AreEqual(1, count);
            //}

            //using (var db = Context.Sql())
            //{
            //    var post = db.BlogPosts.First(p => p.Title == "T2");
            //    Assert.AreEqual(DateTime.Today, post.Created);
            //}
        }

        [TestMethod]
        public async Task UpdateAll_Decrement()
        {
            //SetupBasePosts();

            //int count;
            //using (var db = Context.Sql())
            //{
            //    count = await EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T2").UpdateAsync(b => b.Reads, b => b.Reads - 5);
            //    Assert.AreEqual(1, count);
            //}

            //using (var db = Context.Sql())
            //{
            //    var post = db.BlogPosts.First(p => p.Title == "T2");
            //    Assert.AreEqual(-5, post.Reads);
            //}
        }

        [TestMethod]
        public async Task UpdateAll_Multiply()
        {
            //SetupBasePosts();

            //int count;
            //using (var db = Context.Sql())
            //{
            //    count = await EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T1").UpdateAsync(b => b.Reads, b => b.Reads * 2);
            //    Assert.AreEqual(1, count);
            //}

            //using (var db = Context.Sql())
            //{
            //    var post = db.BlogPosts.First(p => p.Title == "T1");
            //    Assert.AreEqual(4, post.Reads);
            //}
        }

        [TestMethod]
        public async Task UpdateAll_Divide()
        {
            //SetupBasePosts();

            //int count;
            //using (var db = Context.Sql())
            //{
            //    count = await EFBatchOperation.For(db, db.BlogPosts).Where(b => b.Title == "T1").UpdateAsync(b => b.Reads, b => b.Reads / 2);
            //    Assert.AreEqual(1, count);
            //}

            //using (var db = Context.Sql())
            //{
            //    var post = db.BlogPosts.First(p => p.Title == "T1");
            //    Assert.AreEqual(1, post.Reads);
            //}
        }

        private static void SetupBasePosts()
        {
            using (var db = Context.Sql())
            {
                db.SetupDb();

                var p = BlogPost.Create("T1");
                p.Reads = 2;
                db.BlogPosts.Add(p);
                db.BlogPosts.Add(BlogPost.Create("T2"));

                db.SaveChanges();
            }
        }
    }
}
