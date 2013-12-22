using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace Tests.FakeDomain
{
    
    public class Context : DbContext
    {
        protected Context(string connectionString)
            : base(connectionString)
        {

        }

        public IDbSet<BlogPost> BlogPosts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<BlogPost>().Property(x => x.Created);
        }

        protected static Context ConfigureContext(Context contextToConfigure)
        {
            Database.SetInitializer<Context>(null);

            var ctx = new Context(ConnectionStringReader.ConnectionStrings.SqlServer);
            ctx.Configuration.ValidateOnSaveEnabled = false;
            ctx.Configuration.LazyLoadingEnabled = false;
            ctx.Configuration.ProxyCreationEnabled = false;
            ctx.Configuration.AutoDetectChangesEnabled = false;
            return ctx;
        }

        public static MsSqlContext Sql()
        {
            var context = new MsSqlContext(ConnectionStringReader.ConnectionStrings.SqlServer);
            ConfigureContext(context);

            return context;
        }

        public static SqlCeContext SqlCe()
        {
            var context = new SqlCeContext(ConnectionStringReader.ConnectionStrings.SqlServer);
            ConfigureContext(context);

            return context;
        }
    }
}
