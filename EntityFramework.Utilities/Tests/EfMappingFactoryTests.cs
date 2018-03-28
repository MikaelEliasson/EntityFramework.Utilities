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
    public class EfMappingFactoryTests
    {
        [TestMethod]
        
        public void ShouldGetMappingsForContext()
        {
            var ctx = Context.Sql();
            var m = EfMappingFactory.GetMappingsForContext(ctx);
            Assert.IsNotNull(m);
        }
    }
}
