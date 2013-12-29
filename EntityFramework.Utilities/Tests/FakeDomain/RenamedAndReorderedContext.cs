using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;

namespace Tests.FakeDomain
{
    public class RenamedAndReorderedContext : DbContext
    {
        public RenamedAndReorderedContext()
            : base(ConnectionStringReader.ConnectionStrings.SqlServer)
        {
            Database.DefaultConnectionFactory = new SqlConnectionFactory("System.Data.SqlServer");
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
            modelBuilder.Entity<RenamedAndReorderedBlogPost>().Property(x => x.Reads).HasColumnName("Reads2");
        }

        public static void SetupTestDb()
        {
            using (var db = new RenamedAndReorderedContext())
            {
                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.Create();
                db.Database.ExecuteSqlCommand("drop table dbo.RenamedAndReorderedBlogPosts;");
                db.Database.ExecuteSqlCommand(RenamedAndReorderedBlogPost.CreateTableSql());
            }
        }

    }
}
