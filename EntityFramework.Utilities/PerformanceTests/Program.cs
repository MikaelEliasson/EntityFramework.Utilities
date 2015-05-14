using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tests.FakeDomain;
using EntityFramework.Utilities;
using Tests;
using System.Data.Entity;

namespace PerformanceTests
{
    class Program
    {
        static void Main(string[] args)
        {
            BatchIteration(25);
            BatchIteration(25);
            NormalIteration(25);
            NormalIteration(25);
            BatchIteration(2500);
            NormalIteration(2500);
            BatchIteration(25000);
            NormalIteration(25000);
            BatchIteration(50000);
            //NormalIteration(50000);
            BatchIteration(100000);
            NormalIteration(100000);
        }


        private static void NormalIteration(int count)
        {
            Console.WriteLine("Standard iteration with " + count + " entities");
            CreateAndWarmUp();
            var stop = new Stopwatch();

            using (var db = new Context())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;
                var comments = GetEntities(count).ToList();
                stop.Start();
                foreach (var comment in comments)
                {
                    db.Comments.Add(comment);
                }
                db.SaveChanges();
                stop.Stop();
                Console.WriteLine("Insert entities: " + stop.ElapsedMilliseconds + "ms");
            }

            using (var db = new Context())
            {
                db.Configuration.AutoDetectChangesEnabled = true;
                db.Configuration.ValidateOnSaveEnabled = false;
                stop.Restart();
                var toUpdate = db.Comments.Where(c => c.Text == "a").ToList();
                foreach (var item in toUpdate)
                {
                    item.Reads++;
                }
                db.SaveChanges();
                stop.Stop();
                Console.WriteLine("Update all entities with a: " + stop.ElapsedMilliseconds + "ms");
            }

            using (var db = new Context())
            {
                db.Configuration.AutoDetectChangesEnabled = true;
                db.Configuration.ValidateOnSaveEnabled = false;
                var toUpdate = db.Comments.ToList();
                var rand = new Random();
                foreach (var item in toUpdate)
                {
                    item.Reads = rand.Next(0, 9999999);
                }
                stop.Restart();
                db.SaveChanges();
                
                stop.Stop();
                Console.WriteLine("Update all with a random read: " + stop.ElapsedMilliseconds + "ms");
            }

            using (var db = new Context())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;
                stop.Restart();
                var toDelete = db.Comments.Where(c => c.Text == "a").ToList();
                foreach (var item in toDelete)
                {
                    db.Comments.Remove(item);
                }
                db.SaveChanges();
                stop.Stop();
                Console.WriteLine("delete all entities with a: " + stop.ElapsedMilliseconds + "ms");
            }

            using (var db = new Context())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;
                stop.Restart();
                var all = db.Comments.ToList();
                foreach (var item in all)
                {
                    db.Comments.Remove(item);
                }
                db.SaveChanges();
                stop.Stop();
                Console.WriteLine("delete all entities: " + stop.ElapsedMilliseconds + "ms");

            }
        }

        private static void BatchIteration(int count)
        {
            Console.WriteLine("Batch iteration with " + count + " entities");
            CreateAndWarmUp();
            using (var db = new Context())
            {

                var stop = new Stopwatch();
                var comments = GetEntities(count).ToList();                
                stop.Start();
                EFBatchOperation.For(db, db.Comments).InsertAll(comments);
                stop.Stop();
                Console.WriteLine("Insert entities: " + stop.ElapsedMilliseconds + "ms");

                stop.Restart();
                EFBatchOperation.For(db, db.Comments).Where(x => x.Text == "a").Update(x => x.Reads, x => x.Reads + 1);
                stop.Stop();
                Console.WriteLine("Update all entities with a: " + stop.ElapsedMilliseconds + "ms");

                var commentsFromDb = db.Comments.AsNoTracking().ToList();
                var rand = new Random();
                foreach (var item in commentsFromDb)
                {
                    item.Reads = rand.Next(0, 9999999);
                }
                stop.Restart();
                EFBatchOperation.For(db, db.Comments).UpdateAll(commentsFromDb, x => x.ColumnsToUpdate(c => c.Reads));
                stop.Stop();
                Console.WriteLine("Bulk update all with a random read: " + stop.ElapsedMilliseconds + "ms");

                stop.Restart();
                EFBatchOperation.For(db, db.Comments).Where(x => x.Text == "a").Delete();
                stop.Stop();
                Console.WriteLine("delete all entities with a: " + stop.ElapsedMilliseconds + "ms");

                stop.Restart();
                EFBatchOperation.For(db, db.Comments).Where(x => true).Delete();
                stop.Stop();
                Console.WriteLine("delete all entities: " + stop.ElapsedMilliseconds + "ms");

            }
        }

        private static void CreateAndWarmUp()
        {
            using (var db = new Context())
            {
                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.Create();

                //warmup
                db.Comments.Add(new Comment { Date = DateTime.Now, Address = new Address() });
                db.SaveChanges();
                db.Comments.Remove(db.Comments.First());
                db.SaveChanges();
            }
        }

        private static IEnumerable<Comment> GetEntities(int count)
        {
            var comments = Enumerable.Repeat('a', count).Select((c, i) => new Comment
            {
                Text = ((char)(c + (i % 25))).ToString(),
                Date = DateTime.Now.AddDays(i),
                Address = new Address
                {
                    Line1 = "Street",
                    ZipCode = "12345",
                    Town = "Town"
                }
            });
            return comments;
        }
    }

    public class Context : DbContext
    {
        public Context()
            : base("Data Source=./; Initial Catalog=EFUTest; Integrated Security=SSPI; MultipleActiveResultSets=True")
        {

        }

        public IDbSet<Comment> Comments { get; set; }
        public DbSet<Publication> Publications { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ComplexType<Address>();
        }
    }

    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public int Reads { get; set; }
        public Address Address { get; set; }
        public int? PublicationId { get; set; }
        public Publication Publication { get; set; }
    }

    public class Publication
    {
        public int Id { get; set; }
        public string  Title { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }

    public class Address
    {
        public string Line1 { get; set; }
        public string ZipCode { get; set; }
        public string Town { get; set; }
    }
}
