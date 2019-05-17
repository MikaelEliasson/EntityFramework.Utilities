using System.Collections.Generic;
using System.Linq;
using EntityFramework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.FakeDomain;
using Tests.FakeDomain.Models;
using System;
using System.Threading.Tasks;
using EntityFramework.Utilities.SqlServer;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    [TestClass]
    public class InsertTests
    {
        [TestMethod]
        [Ignore]
        public async Task InsertAll_InsertItems_WithTypeHierarchy()
        {
            using (var db = Context.Sql())
            {
                db.SetupDb();

                List<Contact> people = new List<Contact>();
                people.Add(Contact.Build("FN1", "LN1", "Director"));
                people.Add(Contact.Build("FN2", "LN2", "Associate"));
                people.Add(Contact.Build("FN3", "LN3", "Vice President"));

                await EFBatchOperation.For(db, db.People).InsertAllAsync(people);
            }

            using (var db = Context.Sql())
            {
                var contacts = db.People.OfType<Contact>().OrderBy(c => c.FirstName).ToList();
                Assert.AreEqual(3, contacts.Count);
                Assert.AreEqual("FN1", contacts.First().FirstName);
                Assert.AreEqual("Director", contacts.First().Title);
            }
        }

        [TestMethod]
        [Ignore]
        public async Task InsertAll_InsertItems_WithTypeHierarchyBase()
        {
            using (var db = Context.Sql())
            {
                db.SetupDb();

                List<Person> people = new List<Person>();
                people.Add(Person.Build("FN1", "LN1"));
                people.Add(Person.Build("FN2", "LN2"));
                people.Add(Person.Build("FN3", "LN3"));

                await EFBatchOperation.For(db, db.People).InsertAllAsync(people);
            }

            using (var db = Context.Sql())
            {
                var contacts = db.People.OrderBy(c => c.FirstName).ToList();
                Assert.AreEqual(3, contacts.Count);
                Assert.AreEqual("FN1", contacts.First().FirstName);
            }
        }

        [TestMethod]
        public async Task InsertAll_InsertsItems()
        {
            using (var db = Context.Sql())
            {
                db.SetupDb();

                var list = new List<BlogPost>(){
                    BlogPost.Create("T1"),
                    BlogPost.Create("T2"),
                    BlogPost.Create("T3")
                };

                await EFBatchOperation.For(db, db.BlogPosts).InsertAllAsync(list);
            }

            using (var db = Context.Sql())
            {
                Assert.AreEqual(3, db.BlogPosts.Count());
                Assert.AreEqual("m@m.com", db.BlogPosts.First().Author.Email);
            }
        }


        [TestMethod]
        public async Task InsertAll_WithIdReturn_InsertsItemsAndSetIds()
        {
            var list = new List<BlogPost>(){
                    BlogPost.Create("T1"),
                    BlogPost.Create("T2"),
                    BlogPost.Create("T3")
                };
            using (var db = Context.Sql())
            {
                db.SetupDb();

                await EFBatchOperation.For(db, db.BlogPosts).InsertAllAsync(list, new SqlServerBulkSettings
                {
                    ReturnIdsOnInsert = true,
                });
            }

            using (var db = Context.Sql())
            {
                var ids = list.Select(x => x.ID).Distinct();
                Assert.AreEqual(3, ids.Count());

                var first = list.First();
                var firstInDb = db.BlogPosts.Find(first.ID);

                Assert.AreEqual(first.Title, firstInDb.Title);
            }
        }

        [TestMethod]
        public async Task InsertAll_WithExplicitConnection_InsertsItems()
        {
            using (var db = Context.Sql())
            {
                db.SetupDb();

                var list = new List<BlogPost>(){
                    BlogPost.Create("T1"),
                    BlogPost.Create("T2"),
                    BlogPost.Create("T3")
                };
                await EFBatchOperation.For(db, db.BlogPosts).InsertAllAsync(list, new SqlServerBulkSettings { Connection = db.Database.GetDbConnection() as SqlConnection });
                await EFBatchOperation.For(db, db.BlogPosts).InsertAllAsync(list, new SqlServerBulkSettings { Connection = db.Database.GetDbConnection() as SqlConnection });
            }

            using (var db = Context.Sql())
            {
                Assert.AreEqual(3, db.BlogPosts.Count());
            }
        }

        [TestMethod]
        public async Task InsertAll_WrongColumnOrder_InsertsItems()
        {
            using (var db = ReorderedContext.Sql())
            {
                db.SetupDb();
            }

            using (var db = Context.Sql())
            {

                var list = new List<BlogPost>(){
                    BlogPost.Create("T1"),
                    BlogPost.Create("T2"),
                    BlogPost.Create("T3")
                };

                await EFBatchOperation.For(db, db.BlogPosts).InsertAllAsync(list);
            }

            using (var db = Context.Sql())
            {
                Assert.AreEqual(3, db.BlogPosts.Count());
            }
        }

        [TestMethod]
        public async Task InsertAll_WrongColumnOrderAndRenamedColumn_InsertsItems()
        {
            using (var db = RenamedAndReorderedContext.Sql())
            {
                db.SetupDb();
                db.Database.ExecuteSqlCommand("drop table dbo.RenamedAndReorderedBlogPosts;");
                db.Database.ExecuteSqlCommand(RenamedAndReorderedBlogPost.CreateTableSql());
            }

            using (var db = RenamedAndReorderedContext.Sql())
            {

                var list = new List<RenamedAndReorderedBlogPost>(){
                    RenamedAndReorderedBlogPost.Create("T1"),
                    RenamedAndReorderedBlogPost.Create("T2"),
                    RenamedAndReorderedBlogPost.Create("T3")
                };

                await EFBatchOperation.For(db, db.BlogPosts).InsertAllAsync(list);
            }

            using (var db = RenamedAndReorderedContext.Sql())
            {
                Assert.AreEqual(3, db.BlogPosts.Count());
            }
        }


        [TestMethod]
        public async Task InsertAll_WithForeignKey()
        {
            int postId = -1;
            using (var db = Context.Sql())
            {
                db.SetupDb();

                var bp = BlogPost.Create("B1");
                db.BlogPosts.Add(bp);
                db.SaveChanges();
                postId = bp.ID;

                var comments = new List<Comment>(){
                    new Comment{Text = "C1", PostId = bp.ID },
                    new Comment{Text = "C2", PostId = bp.ID },
                };

                await EFBatchOperation.For(db, db.Comments).InsertAllAsync(comments);
            }

            using (var db = Context.Sql())
            {
                Assert.AreEqual(2, db.Comments.Count());
                Assert.AreEqual(2, db.Comments.Count(c => c.PostId == postId));
            }
        }
    }
}
