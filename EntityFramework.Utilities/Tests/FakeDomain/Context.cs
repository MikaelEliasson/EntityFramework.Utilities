using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using Tests.FakeDomain.Models;

namespace Tests.FakeDomain
{
    public class Context : DbContext
    {
        private Context(string connectionString)
            : base(connectionString)
        {

        }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<PhoneNumber> PhoneNumbers { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ComplexType<AuthorInfo>();
            modelBuilder.ComplexType<Address>();

            //Table per Type Hierarchy setup
            modelBuilder.Entity<Person>().ToTable("Person");
            modelBuilder.Entity<Contact>().ToTable("Contact");
        }

        public static Context Sql()
        {
            Database.SetInitializer<Context>(null);
            Database.DefaultConnectionFactory = new SqlConnectionFactory("System.Data.SqlServer");

            var ctx = new Context(ConnectionStringReader.ConnectionStrings.SqlServer);
            ctx.Configuration.ValidateOnSaveEnabled = false;
            ctx.Configuration.LazyLoadingEnabled = false;
            ctx.Configuration.ProxyCreationEnabled = false;
            ctx.Configuration.AutoDetectChangesEnabled = false;


            return ctx;
        }

        public static Context SqlCe()
        {
            Database.SetInitializer<Context>(null);
            var def = Database.DefaultConnectionFactory;
            Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");

            var ctx = new Context(ConnectionStringReader.ConnectionStrings.SqlCe);
            ctx.Configuration.ValidateOnSaveEnabled = false;
            ctx.Configuration.LazyLoadingEnabled = false;
            ctx.Configuration.ProxyCreationEnabled = false;
            ctx.Configuration.AutoDetectChangesEnabled = false;


            return ctx;
        }

    }
}
