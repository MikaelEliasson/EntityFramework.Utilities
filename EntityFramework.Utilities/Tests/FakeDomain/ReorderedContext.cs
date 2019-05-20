using Microsoft.EntityFrameworkCore;
using Tests.FakeDomain.Models;

namespace Tests.FakeDomain
{
    public class ReorderedContext : DbContext
    {
        private string con;

        public ReorderedContext(string con)
            : base()
        {
            this.con = con;

        }
        public DbSet<ReorderedBlogPost> BlogPosts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(this.con);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ReorderedBlogPost>().ToTable("BlogPosts");
            modelBuilder.Owned<AuthorInfo>();
            modelBuilder.Owned<Address>();

        }

        public static ReorderedContext Sql()
        {
            //Database.SetInitializer<Context>(null);

            var ctx = new ReorderedContext("Data Source=MACHINEX;Initial Catalog=BatchTests;Integrated Security=SSPI;MultipleActiveResultSets=True;ConnectRetryCount=0");
            ctx.Database.EnsureCreated();

            return ctx;
        }

    }
}
