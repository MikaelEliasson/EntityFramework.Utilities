using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace Tests.FakeDomain
{
    [DbConfigurationType(typeof(MsSqlConnectionFactory))]
    public class MsSqlContext : Context
    {
        internal MsSqlContext(string connectionString)
            : base(connectionString)
        {

        }
    }
}
