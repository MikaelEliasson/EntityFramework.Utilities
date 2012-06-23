using System.Linq;
using EntityFramework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.FakeDomain;

namespace Tests
{
    [TestClass]
    public class UpdateAllTest
    {

      [TestMethod]
      public void UpdateAll_Increment()
      {
          SetupBasePosts();

          int count;
          using (var db = new Context())
          {
              count = 1; db.UpdateAll<BlogPost>(b => b.Title == "T2", b => b.Reads + 5);
              Assert.AreEqual(1, count);
          }

          using (var db = new Context())
          {
              var post = db.BlogPosts.First(p => p.Title == "T2");
              Assert.AreEqual(5, post.Reads);
          }
      }

      [TestMethod]
      public void UpdateAll_ConcatStringValue()
      {
          SetupBasePosts();

          int count;
          using (var db = new Context())
          {
              count = 1; db.UpdateAll<BlogPost>(b => b.Title == "T2", b => b.Title + ".0");
              Assert.AreEqual(1, count);
          }

          using (var db = new Context())
          {
              var post = db.BlogPosts.First(p => p.Title == "T2.0");
          }
      }

      [TestMethod]
      public void UpdateAll_Decrement()
      {
          SetupBasePosts();

          int count;
          using (var db = new Context())
          {
              count = 1; db.UpdateAll<BlogPost>(b => b.Title == "T2", b => b.Reads - 5);
              Assert.AreEqual(1, count);
          }

          using (var db = new Context())
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
          using (var db = new Context())
          {
              count = 1; db.UpdateAll<BlogPost>(b => b.Title == "T1", b => b.Reads * 2);
              Assert.AreEqual(1, count);
          }

          using (var db = new Context())
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
          using (var db = new Context())
          {
              count = 1; db.UpdateAll<BlogPost>(b => b.Title == "T1", b => b.Reads / 2);
              Assert.AreEqual(1, count);
          }

          using (var db = new Context())
          {
              var post = db.BlogPosts.First(p => p.Title == "T1");
              Assert.AreEqual(1, post.Reads);
          }
      }

      private static void SetupBasePosts()
      {
          using (var db = new Context())
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
