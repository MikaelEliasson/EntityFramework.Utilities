using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;

namespace Tests.FakeDomain
{
    public class MsSqlConnectionFactory : DbConfiguration
    {
        public MsSqlConnectionFactory()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new DefaultExecutionStrategy());
            SetDefaultConnectionFactory(new SqlConnectionFactory("System.Data.SqlServer"));
        }
    }
}
