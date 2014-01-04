using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace Tests.FakeDomain
{
    public class ReorderedContext : DbContext
    {
        public ReorderedContext()
            : base(ConnectionStringReader.ConnectionStrings.SqlServer)
        {
            Database.DefaultConnectionFactory = new SqlConnectionFactory("System.Data.SqlServer");
            Database.SetInitializer(new CreateDatabaseIfNotExists<ReorderedContext>());
            this.Configuration.ValidateOnSaveEnabled = false;
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
            this.Configuration.AutoDetectChangesEnabled = false;
        }
        public IDbSet<ReorderedBlogPost> BlogPosts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ReorderedBlogPost>().ToTable("BlogPosts");
            modelBuilder.ComplexType<AuthorInfo>();
            modelBuilder.ComplexType<Address>();

        }

    }
}
