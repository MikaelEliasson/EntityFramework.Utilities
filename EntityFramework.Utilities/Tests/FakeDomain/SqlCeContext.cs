using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace Tests.FakeDomain
{
    [DbConfigurationType(typeof(SqlCeConnectionFactory))]
    public class SqlCeContext : Context
    {
        internal SqlCeContext(string connectionString)
            : base(connectionString)
        {

        }
    }
}
