using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace Version6.Tests.FakeDomain
{
    public class SqlCeConnectionFactory : DbConfiguration
    {
        public SqlCeConnectionFactory()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new DefaultExecutionStrategy());
            SetDefaultConnectionFactory(new SqlConnectionFactory("System.Data.SqlServer"));
        }
    }
}
