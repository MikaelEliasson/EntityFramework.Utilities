using Microsoft.EntityFrameworkCore;
using Tests.FakeDomain.Models;
using Tests.Models;

namespace Tests.FakeDomain
{
    public class Context : DbContext
    {
        private string con;

        private Context(string connectionString)
            : base()
        {
            this.con = connectionString;
        }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<PhoneNumber> PhoneNumbers { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<NumericTestObject> NumericTestsObjects { get; set; }
        public DbSet<MultiPKObject> MultiPKObjects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Owned<AuthorInfo>();
            modelBuilder.Owned<Address>();

            //Table per Type Hierarchy setup
            //modelBuilder.Entity<Person>()
            //    .Map<Person>(m => m.Requires("Type").HasValue("Person"))
            //    .Map<Contact>(m => m.Requires("Type").HasValue("Contact"));

            modelBuilder.Entity<MultiPKObject>().HasKey(x => new { x.PK1, x.PK2 });


            modelBuilder.Entity<BlogPost>().Property(x => x.ShortTitle).HasMaxLength(100);

            var n = modelBuilder.Entity<NumericTestObject>();
            n.Property(x => x.NumericType).HasColumnType("numeric");
        }

        public static Context Sql()
        {
            //Database.SetInitializer<Context>(null);

            var ctx = new Context("Data Source=DESKTOP-K2L5BEL;Initial Catalog=BatchTests;Integrated Security=SSPI;MultipleActiveResultSets=True");
            ctx.Database.EnsureCreated();

            return ctx;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(this.con);
        }

    }
}
