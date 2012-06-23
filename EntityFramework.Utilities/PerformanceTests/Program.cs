using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tests.FakeDomain;
using EntityFramework.Utilities;

namespace PerformanceTests
{
    public class Program
    {
        /// <summary>
        /// Warning! Running these tests might take quite some time
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var limit = DateTime.Today.AddDays(-5000);
            Console.WriteLine("Adding 10000 posts");
            PerfTest(() => SetupPosts(), 1);


            Console.WriteLine("Traditional Delete");
            PerfTest(() =>
            {
                using (var db = new Context())
                {
                    foreach (var item in db.BlogPosts.Where(p => p.Created > limit))
                    {
                        db.BlogPosts.Remove(item);
                    }
                    db.SaveChanges();
                }
            }, 1);

            Console.WriteLine("Adding 10000 posts");
            PerfTest(() => SetupPosts(), 1);

            Console.WriteLine("Batch Delete");
            PerfTest(() =>
            {
                using (var db = new Context())
                {

                    db.DeleteAll<BlogPost>(p => p.Created > limit);
                }
            }, 1);

            Console.WriteLine("Adding 10000 posts batch");
            PerfTest(() => SetupPostsBatch(), 1);

            Console.WriteLine("Batch Delete");
            PerfTest(() =>
            {
                using (var db = new Context())
                {

                    db.DeleteAll<BlogPost>(p => p.Created > limit);
                }
            }, 1);
        }

        private static void SetupPosts()
        {
            using (var db = new Context())
            {
                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.Create();

                for (int i = 0; i < 10000; i++)
                {
                    var p = BlogPost.Create("T" + i, DateTime.Today.AddDays(i - 10000));
                    db.BlogPosts.Add(p);
                }



                db.SaveChanges();
            }
        }

        private static void SetupPostsBatch()
        {
            using (var db = new Context())
            {
                var list = new List<BlogPost>(10000);
                for (int i = 0; i < 10000; i++)
                {
                    var p = BlogPost.Create("T" + i, DateTime.Today.AddDays(i - 10000));
                    list.Add(p);
                }
                db.InsertAll(list);
            }
        }

        private static void PerfTest(Action action, int iterations)
        {
            var watch = new Stopwatch();
            for (int i = 0; i < iterations; i++)
            {
                watch.Reset();
                watch.Start();

                action();

                watch.Stop();
                Console.WriteLine("Iteration {0} took {1} ms", i, watch.ElapsedMilliseconds);
            }



        }
    }
}
