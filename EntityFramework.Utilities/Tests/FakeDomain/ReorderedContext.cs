using Microsoft.EntityFrameworkCore;
using Tests.FakeDomain.Models;

namespace Tests.FakeDomain
{
    public class ReorderedContext : DbContext
    {
        public ReorderedContext()
            : base()
        {
        }
        public DbSet<ReorderedBlogPost> BlogPosts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionStringReader.ConnectionStrings.SqlServer);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ReorderedBlogPost>().ToTable("BlogPosts");
            modelBuilder.Owned<AuthorInfo>();
            modelBuilder.Owned<Address>();

        }

    }
}
