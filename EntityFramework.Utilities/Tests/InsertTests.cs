using System.Collections.Generic;
using System.Linq;
using EntityFramework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.FakeDomain;

namespace Tests
{
    [TestClass]
    public class InsertTests 
    {
        [TestMethod]
        public void InsertAll_InsertsItems()
        {
            using (var db = new Context())
            {
                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.Create();

                var list = new List<BlogPost>(){
                    BlogPost.Create("T1"),
                    BlogPost.Create("T2"),
                    BlogPost.Create("T3")
                };

                db.InsertAll(list);
            }

            using (var db = new Context())
            {
                Assert.AreEqual(3, db.BlogPosts.Count());
            }
        }
    }
}
