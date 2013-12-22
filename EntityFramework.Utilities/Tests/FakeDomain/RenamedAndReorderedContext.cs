using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace Tests.FakeDomain
{
    public class RenamedAndReorderedContext : DbContext
    {
        public RenamedAndReorderedContext()
            : base(ConnectionStringReader.ConnectionStrings.SqlServer)
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<ReorderedContext>());
            this.Configuration.ValidateOnSaveEnabled = false;
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
            this.Configuration.AutoDetectChangesEnabled = false;
        }
        public IDbSet<RenamedAndReorderedBlogPost> BlogPosts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<RenamedAndReorderedBlogPost>().ToTable("RenamedAndReorderedBlogPosts");
            modelBuilder.Entity<RenamedAndReorderedBlogPost>().Property(x => x.Created).HasColumnName("Created2");
        }

    }
}
