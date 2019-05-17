using Microsoft.EntityFrameworkCore;
using Tests.FakeDomain.Models;

namespace Tests.FakeDomain
{
    public class RenamedAndReorderedContext : DbContext
    {
        private string con;

        public RenamedAndReorderedContext(string con)
            : base()
        {
            this.con = con;

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(this.con);
        }

        public DbSet<RenamedAndReorderedBlogPost> BlogPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<RenamedAndReorderedBlogPost>().ToTable("RenamedAndReorderedBlogPosts");
            modelBuilder.Entity<RenamedAndReorderedBlogPost>().Property(x => x.Created).HasColumnName("Created2");
            modelBuilder.Entity<RenamedAndReorderedBlogPost>().Property(x => x.Reads).HasColumnName("Reads2");
        }

        public static RenamedAndReorderedContext Sql()
        {
            //Database.SetInitializer<Context>(null);

            var ctx = new RenamedAndReorderedContext("Data Source=MACHINEX;Initial Catalog=BatchTests;Integrated Security=SSPI;MultipleActiveResultSets=True");
            ctx.Database.EnsureCreated();

            return ctx;
        }

    }
}
