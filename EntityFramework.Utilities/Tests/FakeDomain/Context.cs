using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace Tests.FakeDomain
{
    public class Context : DbContext
    {
        public Context() : base(ConnectionStringReader.ConnectionStrings.SqlServer)
        {
            Database.SetInitializer<Context>(null);
            this.Configuration.ValidateOnSaveEnabled = false;
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
            this.Configuration.AutoDetectChangesEnabled = false;
        }
        public IDbSet<BlogPost> BlogPosts { get; set; }

    }
}
