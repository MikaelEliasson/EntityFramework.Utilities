using EntityFramework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tests.FakeDomain;

namespace Tests
{
    [TestClass]
    public class IncludeTest
    {
        [TestMethod]
        public void LoadWithChildren_OneChildQuery_LoadsChildren()
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
                var result = db.Contacts.IncludeEFU(db, x => x.PhoneNumbers, p => p.ContactId).ToList();
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
        public void LoadWithChildren_OneChildQuerySortedParent_LoadsChildren()
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
                var result = db.Contacts.OrderByDescending(x => x.FirstName).IncludeEFU(db, x => x.PhoneNumbers, p => p.ContactId).ToList();
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
        public void LoadWithChildren_OneChildQuery_SortAfterInclude_LoadsChildren()
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
                var result = db.Contacts.IncludeEFU(db, x => x.PhoneNumbers, p => p.ContactId).OrderByDescending(x => x.FirstName).ToList();
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
                    }
            });
            db.Contacts.Add(new Contact
            {
                FirstName = "FN3",
                LastName = "LN3",
                Id = Guid.NewGuid(),
                BirthDate = DateTime.Today,
            });

            db.SaveChanges();
        }
    }
}
