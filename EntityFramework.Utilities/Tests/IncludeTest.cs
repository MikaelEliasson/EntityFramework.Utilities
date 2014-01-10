using EntityFramework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Tests.FakeDomain;

namespace Tests
{
    [TestClass]
    public class IncludeTest
    {
        [TestMethod]
        public void SingleInclude_LoadsChildren()
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
        public void SingleInclude_SortedParent_LoadsChildren()
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
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.ForceDelete();
                }
                db.Database.Create();
                CreateSmallTestSet(db);
            }
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
        public void DoubleIncludes_LoadsChildren()
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
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.ForceDelete();
                }
                db.Database.Create();
                CreateSmallTestSet(db);
            }
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
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.ForceDelete();
                }
                db.Database.Create();
                CreateSmallTestSet(db);
            }
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
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.ForceDelete();
                }
                db.Database.Create();
                CreateSmallTestSet(db);
            }

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
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.ForceDelete();
                }
                db.Database.Create();
                CreateSmallTestSet(db);
            }

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
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.ForceDelete();
                }
                db.Database.Create();
                CreateSmallTestSet(db);
            }

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
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.ForceDelete();
                }
                db.Database.Create();
                CreateSmallTestSet(db);
            }

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
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.ForceDelete();
                }
                db.Database.Create();
                CreateSmallTestSet(db);
            }

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

        private static void CreateSmallTestSet(Context db)
        {
            db.Contacts.Add(new Contact
            {
                FirstName = "FN1",
                LastName = "LN1",
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
                Id = Guid.NewGuid(),
                BirthDate = DateTime.Today,
                Emails = new List<Email>()
                {
                    new Email{Id = Guid.NewGuid(), Address = "m31@mail.com" },
                    new Email{Id = Guid.NewGuid(), Address = "m32@mail.com" },
                }
            });

            db.SaveChanges();
        }
    }
}
