using System.Linq;
using EntityFramework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.FakeDomain;
using System;

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
          using (var db = Context.Sql())
          {
              count = db.UpdateAll<BlogPost>(b => b.Title == "T2", b => b.Reads + 5);
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
              count = db.UpdateAll<BlogPost>(b => b.Title == "T2", b => b.Reads.SetTo(10));
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
              count = db.UpdateAll<BlogPost>(b => b.Title == "T2", b => b.Reads.SetTo(reads));
              Assert.AreEqual(1, count);
          }

          using (var db = Context.Sql())
          {
              var post = db.BlogPosts.First(p => p.Title == "T2");
              Assert.AreEqual(20, post.Reads);
          }
      }

      [TestMethod]
      public void UpdateAll_ConcatStringValue()
      {
          SetupBasePosts();

          int count;
          using (var db = Context.Sql())
          {
              count = db.UpdateAll<BlogPost>(b => b.Title == "T2", b => b.Title + ".0");
              Assert.AreEqual(1, count);
          }

          using (var db = Context.Sql())
          {
              var post = db.BlogPosts.First(p => p.Title == "T2.0");
          }
      }

      [TestMethod]
      public void UpdateAll_SetDateTimeValueFromVariable()
      {
          SetupBasePosts();

          int count;
          using (var db = Context.Sql())
          {
              count = db.UpdateAll<BlogPost>(b => b.Title == "T2", b => b.Created.SetTo(DateTime.Today));
              Assert.AreEqual(1, count);
          }

          using (var db = Context.Sql())
          {
              var post = db.BlogPosts.First(p => p.Created == DateTime.Today);
          }
      }

      [TestMethod]
      public void UpdateAll_SetDateTimeValueFromVariable_RenamedColumn()
      {
          RenamedAndReorderedContext.SetupTestDb();
          using (var db = new RenamedAndReorderedContext())
          {
              db.BlogPosts.Add(new RenamedAndReorderedBlogPost { Title = "T1", Created = new DateTime(2013, 01, 01) });
              db.BlogPosts.Add(new RenamedAndReorderedBlogPost { Title = "T2", Created = new DateTime(2013, 02, 01) });
              db.BlogPosts.Add(new RenamedAndReorderedBlogPost { Title = "T3", Created = new DateTime(2013, 03, 01) });

              db.SaveChanges();
          }

          int count;
          using (var db = new RenamedAndReorderedContext())
          {
              count = db.UpdateAll<RenamedAndReorderedBlogPost>(b => b.Title == "T2", b => b.Created.SetTo(DateTime.Today));
              Assert.AreEqual(1, count);
          }

          using (var db = new RenamedAndReorderedContext())
          {
              var post = db.BlogPosts.First(p => p.Created == DateTime.Today);
          }
      }

      [TestMethod]
      public void UpdateAll_IncrementIntValue_RenamedColumn()
      {
          RenamedAndReorderedContext.SetupTestDb();
          using (var db = new RenamedAndReorderedContext())
          {
              db.BlogPosts.Add(new RenamedAndReorderedBlogPost { Title = "T1", Created = new DateTime(2013, 01, 01) });
              db.BlogPosts.Add(new RenamedAndReorderedBlogPost { Title = "T2", Created = new DateTime(2013, 02, 01) });
              db.BlogPosts.Add(new RenamedAndReorderedBlogPost { Title = "T3", Created = new DateTime(2013, 03, 01) });

              db.SaveChanges();
          }

          int count;
          using (var db = new RenamedAndReorderedContext())
          {
              count = db.UpdateAll<RenamedAndReorderedBlogPost>(b => b.Title == "T2", b => b.Reads + 100);
              Assert.AreEqual(1, count);
          }

          using (var db = new RenamedAndReorderedContext())
          {
              var post = db.BlogPosts.First(p => p.Reads == 100);
          }
      }

      [TestMethod]
      public void UpdateAll_Decrement()
      {
          SetupBasePosts();

          int count;
          using (var db = Context.Sql())
          {
              count = db.UpdateAll<BlogPost>(b => b.Title == "T2", b => b.Reads - 5);
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
              count = db.UpdateAll<BlogPost>(b => b.Title == "T1", b => b.Reads * 2);
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
              count = db.UpdateAll<BlogPost>(b => b.Title == "T1", b => b.Reads / 2);
              Assert.AreEqual(1, count);
          }

          using (var db = Context.Sql())
          {
              var post = db.BlogPosts.First(p => p.Title == "T1");
              Assert.AreEqual(1, post.Reads);
          }
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
