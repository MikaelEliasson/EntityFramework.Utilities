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
            }
            using (var db = Context.Sql())
            {
                var result = db.Contacts.IncludeEFU(db, x => x.PhoneNumbers).ToList();
            }
        }
    }
}
