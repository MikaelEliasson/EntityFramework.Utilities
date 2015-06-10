using System.Collections.Generic;
using System.Linq;
using EntityFramework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.FakeDomain;
using Tests.FakeDomain.Models;
using Tests.Models;
using System;

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
        public void UpdateBulk_CanUpdateTPH()
        {
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.Create();

                List<Contact> people = new List<Contact>();
                people.Add(Contact.Build("FN1", "LN1", "Director"));
                people.Add(Contact.Build("FN2", "LN2", "Associate"));
                people.Add(Contact.Build("FN3", "LN3", "Vice President"));

                EFBatchOperation.For(db, db.People).InsertAll(people);
            }

            using (var db = Context.Sql())
            {
                var contacts = db.Contacts.ToList();

                foreach (var contact in contacts)
                {
                    contact.FirstName = contact.Title + " " + contact.FirstName;
                }

                EFBatchOperation.For(db, db.People).UpdateAll(contacts, x => x.ColumnsToUpdate(p => p.FirstName));
            }

            using (var db = Context.Sql())
            {
                var contacts = db.People.OfType<Contact>().OrderBy(c => c.LastName).ToList();
                Assert.AreEqual(3, contacts.Count);
                Assert.AreEqual("Director FN1", contacts.First().FirstName);
            }
        }

        [TestMethod]
        public void UpdateBulk_CanUpdateNumerics()
        {
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.ForceDelete();
                }
                db.Database.Create();

                var list = new List<NumericTestObject>(){
                    new NumericTestObject{ }
                };

                EFBatchOperation.For(db, db.NumericTestsObjects).InsertAll(list);
            }

            using (var db = Context.Sql())
            {
                var items = db.NumericTestsObjects.ToList();
                foreach (var item in items)
                {
                    item.DecimalType = 1.1m;
                    item.FloatType = 2.1f;
                    item.NumericType = 3.1m;
                }
                EFBatchOperation.For(db, db.NumericTestsObjects).UpdateAll(items, spec => spec.ColumnsToUpdate(p => p.DecimalType, p => p.FloatType, p => p.NumericType));
            }

            using (var db = Context.Sql())
            {
                var item = db.NumericTestsObjects.First();
                Assert.AreEqual(1.1m, item.DecimalType);
                Assert.AreEqual(2.1f, item.FloatType);
                Assert.AreEqual(3.1m, item.NumericType);
            }
        }

        [TestMethod]
        public void UpdateBulk_CanUpdateMultiPK()
        {
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.ForceDelete();
                }
                db.Database.Create();

                var guid = Guid.NewGuid();
                var list = new List<MultiPKObject>(){
                    new MultiPKObject{ PK1 = guid, PK2 = 0 },
                    new MultiPKObject{ PK1 = guid, PK2 = 1 }
                };

                EFBatchOperation.For(db, db.MultiPKObjects).InsertAll(list);
            }

            using (var db = Context.Sql())
            {
                var items = db.MultiPKObjects.ToList();
                var index = 1;
                foreach (var item in items)
                {
                    item.Text = "#" + index++;
                }
                EFBatchOperation.For(db, db.MultiPKObjects).UpdateAll(items, spec => spec.ColumnsToUpdate(p => p.Text));
            }

            using (var db = Context.Sql())
            {
                var items = db.MultiPKObjects.OrderBy(x => x.PK2).ToList();
                Assert.AreEqual("#1", items[0].Text);
                Assert.AreEqual("#2", items[1].Text);
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
