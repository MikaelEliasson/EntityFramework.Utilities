using Microsoft.EntityFrameworkCore;
using Tests.FakeDomain.Models;

namespace Tests.FakeDomain
{
    public class RenamedAndReorderedContext : DbContext
    {
        public RenamedAndReorderedContext()
            : base()
        {
            
        }

        public DbSet<RenamedAndReorderedBlogPost> BlogPosts { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionStringReader.ConnectionStrings.SqlServer);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
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
                db.SetupDb();
                db.Database.ExecuteSqlCommand("drop table dbo.RenamedAndReorderedBlogPosts;");
                db.Database.ExecuteSqlCommand(RenamedAndReorderedBlogPost.CreateTableSql());
            }
        }

    }
}
