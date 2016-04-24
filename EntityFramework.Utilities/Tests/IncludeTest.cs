using EntityFramework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Tests.FakeDomain;
using Tests.FakeDomain.Models;

namespace Tests
{
    [TestClass]
    public class IncludeTest
    {
        [TestMethod]
        public void SingleInclude_LoadsChildren()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = db.Contacts.IncludeEFU(db, x => x.PhoneNumbers).ToList();
                var fn1 = result.First(x => x.FirstName == "FN1");
                var fn2 = result.First(x => x.FirstName == "FN2");
                var fn3 = result.First(x => x.FirstName == "FN3");

                Assert.AreEqual(2, fn1.PhoneNumbers.Count);
                Assert.AreEqual('1', fn1.PhoneNumbers.First().Number.First());
                Assert.AreEqual(2, fn2.PhoneNumbers.Count);
                Assert.AreEqual('2', fn2.PhoneNumbers.First().Number.First());
                Assert.AreEqual(0, fn3.PhoneNumbers.Count);
            }
        }

        [TestMethod]
        public void SingleInclude_LoadsChildren_BlogPost()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = db.BlogPosts.IncludeEFU(db, x => x.Comments).ToList();
                var bp1 = result.First(x => x.Title == "BP1");
                var bp2 = result.First(x => x.Title == "BP2");
                var bp3 = result.First(x => x.Title == "BP3");

                Assert.AreEqual(1, bp1.Comments.Count);
                Assert.AreEqual("C1", bp1.Comments.First().Text);
                Assert.AreEqual(2, bp2.Comments.Count);
                Assert.AreEqual("C2", bp2.Comments.First().Text);
                Assert.AreEqual(3, bp3.Comments.Count);
            }
        }

        [TestMethod]
        public void SingleInclude_SortedParent_LoadsChildren()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = db.Contacts.OrderByDescending(x => x.FirstName).IncludeEFU(db, x => x.PhoneNumbers).ToList();
                var fn3 = result[0];
                var fn2 = result[1];
                var fn1 = result[2];

                Assert.AreEqual("FN3", fn3.FirstName);
                Assert.AreEqual("FN2", fn2.FirstName);
                Assert.AreEqual("FN1", fn1.FirstName);

                Assert.AreEqual(2, fn1.PhoneNumbers.Count);
                Assert.AreEqual('1', fn1.PhoneNumbers.First().Number.First());
                Assert.AreEqual(2, fn2.PhoneNumbers.Count);
                Assert.AreEqual('2', fn2.PhoneNumbers.First().Number.First());
                Assert.AreEqual(0, fn3.PhoneNumbers.Count);
            }
        }

        [TestMethod]
        public void SingleInclude_SortAfterInclude_LoadsChildren()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = db.Contacts.IncludeEFU(db, x => x.PhoneNumbers).OrderByDescending(x => x.FirstName).ToList();
                var fn3 = result[0];
                var fn2 = result[1];
                var fn1 = result[2];
                Assert.AreEqual("FN3", fn3.FirstName);
                Assert.AreEqual("FN2", fn2.FirstName);
                Assert.AreEqual("FN1", fn1.FirstName);


                Assert.AreEqual(2, fn1.PhoneNumbers.Count);
                Assert.AreEqual('1', fn1.PhoneNumbers.First().Number.First());
                Assert.AreEqual(2, fn2.PhoneNumbers.Count);
                Assert.AreEqual('2', fn2.PhoneNumbers.First().Number.First());
                Assert.AreEqual(0, fn3.PhoneNumbers.Count);
            }
        }

        [TestMethod]
        public void SingleInclude_WithFirst_LoadsChildren()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = db.Contacts.IncludeEFU(db, x => x.PhoneNumbers).OrderBy(x => x.FirstName).First();
                Assert.AreEqual("FN1", result.FirstName);

                Assert.AreEqual(2, result.PhoneNumbers.Count);
                Assert.AreEqual('1', result.PhoneNumbers.First().Number.First());
            }
        }

        [TestMethod]
        public void SingleInclude_WithTake_LoadsChildren()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = db.Contacts.IncludeEFU(db, x => x.PhoneNumbers).OrderBy(x => x.FirstName).Take(1).ToList().Single();
                Assert.AreEqual("FN1", result.FirstName);

                Assert.AreEqual(2, result.PhoneNumbers.Count);
                Assert.AreEqual('1', result.PhoneNumbers.First().Number.First());
            }
        }

        [TestMethod]
        public void DoubleIncludes_LoadsChildren()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = db.Contacts
                    .IncludeEFU(db, x => x.PhoneNumbers)
                    .IncludeEFU(db, x => x.Emails)
                    .ToList();
                var fn1 = result.First(x => x.FirstName == "FN1");
                var fn2 = result.First(x => x.FirstName == "FN2");
                var fn3 = result.First(x => x.FirstName == "FN3");

                Assert.AreEqual(2, fn1.PhoneNumbers.Count);
                Assert.AreEqual(0, fn1.Emails.Count);
                Assert.AreEqual('1', fn1.PhoneNumbers.First().Number.First());

                Assert.AreEqual(2, fn2.PhoneNumbers.Count);
                Assert.AreEqual('2', fn2.PhoneNumbers.First().Number.First());
                Assert.AreEqual(2, fn2.Emails.Count);

                Assert.AreEqual(0, fn3.PhoneNumbers.Count);
                Assert.AreEqual(2, fn3.Emails.Count);

            }

        }

        [TestMethod]
        public void DoubleIncludes_PreSort_LoadsChildren()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = db.Contacts
                    .OrderByDescending(x => x.FirstName)
                    .IncludeEFU(db, x => x.PhoneNumbers)
                    .IncludeEFU(db, x => x.Emails)
                    .ToList();

                var fn3 = result[0];
                var fn2 = result[1];
                var fn1 = result[2];
                Assert.AreEqual("FN3", fn3.FirstName);
                Assert.AreEqual("FN2", fn2.FirstName);
                Assert.AreEqual("FN1", fn1.FirstName);

                Assert.AreEqual(2, fn1.PhoneNumbers.Count);
                Assert.AreEqual(0, fn1.Emails.Count);
                Assert.AreEqual('1', fn1.PhoneNumbers.First().Number.First());

                Assert.AreEqual(2, fn2.PhoneNumbers.Count);
                Assert.AreEqual('2', fn2.PhoneNumbers.First().Number.First());
                Assert.AreEqual(2, fn2.Emails.Count);

                Assert.AreEqual(0, fn3.PhoneNumbers.Count);
                Assert.AreEqual(2, fn3.Emails.Count);

            }
        }

        [TestMethod]
        public void DoubleIncludes_PostSort_LoadsChildren()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = db.Contacts        
                    .IncludeEFU(db, x => x.PhoneNumbers)
                    .IncludeEFU(db, x => x.Emails)
                    .OrderByDescending(x => x.FirstName)
                    .ToList();

                var fn3 = result[0];
                var fn2 = result[1];
                var fn1 = result[2];
                Assert.AreEqual("FN3", fn3.FirstName);
                Assert.AreEqual("FN2", fn2.FirstName);
                Assert.AreEqual("FN1", fn1.FirstName);

                Assert.AreEqual(2, fn1.PhoneNumbers.Count);
                Assert.AreEqual(0, fn1.Emails.Count);
                Assert.AreEqual('1', fn1.PhoneNumbers.First().Number.First());

                Assert.AreEqual(2, fn2.PhoneNumbers.Count);
                Assert.AreEqual('2', fn2.PhoneNumbers.First().Number.First());
                Assert.AreEqual(2, fn2.Emails.Count);

                Assert.AreEqual(0, fn3.PhoneNumbers.Count);
                Assert.AreEqual(2, fn3.Emails.Count);

            }
        }

        [TestMethod]
        public void DoubleIncludes_SortAndWhere_LoadsChildren()
        {
            SetupSmallTestSet();

            using (var db = Context.Sql())
            {
                var result = db.Contacts
                    .IncludeEFU(db, c => c.PhoneNumbers)
                    .IncludeEFU(db, c => c.Emails)
                    .Where(c => !c.FirstName.Contains("3"))
                    .OrderByDescending(c => c.FirstName)
                    .ToList();

                Assert.AreEqual(2, result.Count);

                var fn2 = result[0];
                var fn1 = result[1];
                Assert.AreEqual("FN2", fn2.FirstName);
                Assert.AreEqual("FN1", fn1.FirstName);

                Assert.AreEqual(2, fn1.PhoneNumbers.Count);
                Assert.AreEqual(0, fn1.Emails.Count);
                Assert.AreEqual('1', fn1.PhoneNumbers.First().Number.First());

                Assert.AreEqual(2, fn2.PhoneNumbers.Count);
                Assert.AreEqual('2', fn2.PhoneNumbers.First().Number.First());
                Assert.AreEqual(2, fn2.Emails.Count);
            }
        }

        [TestMethod]
        public void DoubleIncludes_WherePropEqualsSomething_LoadsChildren()
        {
            SetupSmallTestSet();

            using (var db = Context.Sql())
            {
                var result = db.Contacts
                    .IncludeEFU(db, c => c.PhoneNumbers)
                    .IncludeEFU(db, c => c.Emails)
                    .Where(c => c.FirstName == "FN2")
                    .OrderByDescending(c => c.FirstName)
                    .ToList();

                Assert.AreEqual(1, result.Count);

                var fn2 = result[0];
                Assert.AreEqual("FN2", fn2.FirstName);

                Assert.AreEqual(2, fn2.PhoneNumbers.Count);
                Assert.AreEqual('2', fn2.PhoneNumbers.First().Number.First());
                Assert.AreEqual(2, fn2.Emails.Count);
            }
        }

        [TestMethod]
        public void DoubleIncludes_DoubleWheres_LoadsChildren()
        {
            SetupSmallTestSet();

            using (var db = Context.Sql())
            {
                var result = db.Contacts
                    .Where(c => c.FirstName == "FN2")
                    .IncludeEFU(db, c => c.PhoneNumbers)
                    .IncludeEFU(db, c => c.Emails)
                    .Where(c => DbFunctions.Reverse(c.FirstName) == "2NF")
                    .OrderByDescending(c => c.FirstName)
                    .ToList();

                Assert.AreEqual(1, result.Count);

                var fn2 = result[0];
                Assert.AreEqual("FN2", fn2.FirstName);

                Assert.AreEqual(2, fn2.PhoneNumbers.Count);
                Assert.AreEqual('2', fn2.PhoneNumbers.First().Number.First());
                Assert.AreEqual(2, fn2.Emails.Count);
            }
        }

        [TestMethod]
        public void DoubleIncludes_WhereWithOr_LoadsChildren()
        {
            SetupSmallTestSet();

            using (var db = Context.Sql())
            {
                var result = db.Contacts
                    .IncludeEFU(db, c => c.PhoneNumbers)
                    .IncludeEFU(db, c => c.Emails)
                    .Where(c => c.FirstName == "FN2" || c.FirstName == "FN4")
                    .OrderByDescending(c => c.FirstName)
                    .ToList();

                Assert.AreEqual(1, result.Count);

                var fn2 = result[0];
                Assert.AreEqual("FN2", fn2.FirstName);

                Assert.AreEqual(2, fn2.PhoneNumbers.Count);
                Assert.AreEqual('2', fn2.PhoneNumbers.First().Number.First());
                Assert.AreEqual(2, fn2.Emails.Count);
            }
        }

        [TestMethod]
        public void DoubleIncludes_WhereWithDbFunction_LoadsChildren()
        {
            SetupSmallTestSet();

            using (var db = Context.Sql())
            {
                var result = db.Contacts
                    .IncludeEFU(db, c => c.PhoneNumbers)
                    .IncludeEFU(db, c => c.Emails)
                    .Where(c => DbFunctions.Reverse(c.FirstName) == "2NF")
                    .OrderByDescending(c => c.FirstName)
                    .ToList();

                Assert.AreEqual(1, result.Count);

                var fn2 = result[0];
                Assert.AreEqual("FN2", fn2.FirstName);

                Assert.AreEqual(2, fn2.PhoneNumbers.Count);
                Assert.AreEqual('2', fn2.PhoneNumbers.First().Number.First());
                Assert.AreEqual(2, fn2.Emails.Count);
            }
        }


        [TestMethod]
        public void SingleInclude_LoadsChildrenOrdered()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = db.Contacts.IncludeEFU(db, x => x.PhoneNumbers.OrderBy(p => p.Number)).ToList();
                var fn1 = result.First(x => x.FirstName == "FN1");

                Assert.AreEqual(2, fn1.PhoneNumbers.Count);
                Assert.AreEqual("10134", fn1.PhoneNumbers.First().Number);
            }

            using (var db = Context.Sql())
            {
                var result = db.Contacts.IncludeEFU(db, x => x.PhoneNumbers.OrderByDescending(p => p.Number)).ToList();
                var fn1 = result.First(x => x.FirstName == "FN1");

                Assert.AreEqual(2, fn1.PhoneNumbers.Count);
                Assert.AreEqual("15678", fn1.PhoneNumbers.First().Number);
            }
        }

        [TestMethod]
        public void SingleInclude_LoadsChildrenOrderedWithThenBy()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = db.Contacts.IncludeEFU(db, x => x.PhoneNumbers.OrderBy(p => p.ContactId).ThenBy(p => p.Number)).ToList();
                var fn1 = result.First(x => x.FirstName == "FN1");

                Assert.AreEqual(2, fn1.PhoneNumbers.Count);
                Assert.AreEqual("10134", fn1.PhoneNumbers.First().Number);
            }

            using (var db = Context.Sql())
            {
                var result = db.Contacts.IncludeEFU(db, x => x.PhoneNumbers.OrderBy(p => p.ContactId).ThenByDescending(p => p.Number)).ToList();
                var fn1 = result.First(x => x.FirstName == "FN1");

                Assert.AreEqual(2, fn1.PhoneNumbers.Count);
                Assert.AreEqual("15678", fn1.PhoneNumbers.First().Number);
            }
        }

        [TestMethod]
        public void SingleInclude_LoadsChildrenSortAndFilter()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = db.Contacts.IncludeEFU(db, x => x.PhoneNumbers.Where(n => n.Number == "10134").OrderBy(p => p.ContactId).ThenBy(p => p.Number)).ToList();
                var fn1 = result.First(x => x.FirstName == "FN1");

                Assert.AreEqual(1, fn1.PhoneNumbers.Count);
                Assert.AreEqual("10134", fn1.PhoneNumbers.First().Number);
            }

            using (var db = Context.Sql())
            {
                var result = db.Contacts.IncludeEFU(db, x => x.PhoneNumbers.Where(n => n.Number == "10134").OrderBy(p => p.ContactId).ThenByDescending(p => p.Number)).ToList();
                var fn1 = result.First(x => x.FirstName == "FN1");

                Assert.AreEqual(1, fn1.PhoneNumbers.Count);
                Assert.AreEqual("10134", fn1.PhoneNumbers.First().Number);
            }
        }

        [TestMethod]
        public void SingleInclude_LoadsChildrenFiltered()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = db.Contacts.IncludeEFU(db, x => x.PhoneNumbers.Where(p => p.Number == "10134")).ToList();
                var fn1 = result.First(x => x.FirstName == "FN1");

                Assert.AreEqual(1, fn1.PhoneNumbers.Count);
                Assert.AreEqual("10134", fn1.PhoneNumbers.First().Number);
            }
        }

        [TestMethod]
        public void SingleInclude_RefactoredWhereAsStaticMethod()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = Queries.FilterByName(db.Contacts.IncludeEFU(db, x => x.PhoneNumbers), "FN1").ToList();
                var fn1 = result.First(x => x.FirstName == "FN1");

                Assert.AreEqual(2, fn1.PhoneNumbers.Count);
            }
        }

        [TestMethod]
        public void SingleInclude_RefactoredWhereAsExtensionMethod()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = db.Contacts.IncludeEFU(db, x => x.PhoneNumbers).FilterByAsExtensionMethod("FN1").ToList();
                var fn1 = result.First(x => x.FirstName == "FN1");

                Assert.AreEqual(2, fn1.PhoneNumbers.Count);
            }
        }

        [TestMethod]
        public void SingleInclude_RefactoredWhereAsStaticMethod_WithIQueryableAsSecondArgument()
        {
            SetupSmallTestSet();
            using (var db = Context.Sql())
            {
                var result = Queries.FilterByNameInReversedOrder("FN1", db.Contacts.IncludeEFU(db, x => x.PhoneNumbers)).ToList();
                var fn1 = result.First(x => x.FirstName == "FN1");

                Assert.AreEqual(2, fn1.PhoneNumbers.Count);
            }
        }

        private static void SetupSmallTestSet()
        {
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.ForceDelete();
                }
                db.Database.Create();
                CreateSmallTestSet(db);
            }
        }

        private static void CreateSmallTestSet(Context db)
        {
            db.Contacts.Add(new Contact
            {
                FirstName = "FN1",
                LastName = "LN1",
                Title = "Director",
                Id = Guid.NewGuid(),
                BirthDate = DateTime.Today,
                PhoneNumbers = new List<PhoneNumber>(){
                       new PhoneNumber{
                           Id = Guid.NewGuid(),
                           Number = "10134"
                       },
                       new PhoneNumber{
                           Id = Guid.NewGuid(),
                           Number = "15678"
                       },
                    }
            });
            db.Contacts.Add(new Contact
            {
                FirstName = "FN2",
                LastName = "LN2",
                Title = "Associate",
                Id = Guid.NewGuid(),
                BirthDate = DateTime.Today,
                PhoneNumbers = new List<PhoneNumber>(){
                       new PhoneNumber{
                           Id = Guid.NewGuid(),
                           Number = "20134"
                       },
                       new PhoneNumber{
                           Id = Guid.NewGuid(),
                           Number = "25678"
                       },
                    },
                Emails = new List<Email>()
                {
                    new Email{Id = Guid.NewGuid(), Address = "m21@mail.com" },
                    new Email{Id = Guid.NewGuid(), Address = "m22@mail.com" },
                }
            });
            db.Contacts.Add(new Contact
            {
                FirstName = "FN3",
                LastName = "LN3",
                Title = "Vice President",
                Id = Guid.NewGuid(),
                BirthDate = DateTime.Today,
                Emails = new List<Email>()
                {
                    new Email{Id = Guid.NewGuid(), Address = "m31@mail.com" },
                    new Email{Id = Guid.NewGuid(), Address = "m32@mail.com" },
                }
            });

            var blogPost1 = BlogPost.Create("BP1");
            blogPost1.Comments = new List<Comment>()
                {
                    new Comment() { Text = "C1" }
                };
            db.BlogPosts.Add(blogPost1);

            var blogPost2 = BlogPost.Create("BP2");
            blogPost2.Comments = new List<Comment>()
                {
                    new Comment() { Text = "C2" },
                    new Comment() { Text = "C3" }
                };
            db.BlogPosts.Add(blogPost2);

            var blogPost3 = BlogPost.Create("BP3");
            blogPost3.Comments = new List<Comment>()
                {
                    new Comment() { Text = "C4" },
                    new Comment() { Text = "C5" },
                    new Comment() { Text = "C6" }
                };
            db.BlogPosts.Add(blogPost3);

            db.SaveChanges();
        }
    }
}
