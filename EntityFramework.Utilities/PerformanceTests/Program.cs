using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Tests.FakeDomain;
using EntityFramework.Utilities;
using Tests;

namespace PerformanceTests
{
    public class Program
    {

        private const int iterations = 3;
        private const int insertSize = 40000;
        /// <summary>
        /// Warning! Running these tests might take quite some time
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var limit = DateTime.Today.AddDays(-5000);
            Console.WriteLine("Adding " + insertSize + " posts");
            PerfTest(() => SetupPosts(), iterations);


            Console.WriteLine("Traditional Delete");
            PerfTest(() =>
            {
                using (var db = Context.Sql())
                {
                    foreach (var item in db.BlogPosts.Where(p => p.Created > limit))
                    {
                        db.BlogPosts.Remove(item);
                    }
                    db.SaveChanges();
                }
            }, iterations);

            Console.WriteLine("Adding " + insertSize + " posts");
            PerfTest(() => SetupPosts(), iterations);

            Console.WriteLine("Batch Delete");
            PerfTest(() =>
            {
                using (var db = Context.Sql())
                {
                    db.DeleteAll<BlogPost>(p => p.Created > limit);
                }
            }, iterations);

            Console.WriteLine("Adding " + insertSize + " posts batch");
            PerfTest(() => SetupPostsBatch(), iterations);

            Console.WriteLine("Batch Delete");
            PerfTest(() =>
            {
                using (var db = Context.Sql())
                {

                    db.DeleteAll<BlogPost>(p => p.Created > limit);
                }
            }, iterations);
        }

        private static void SetupPosts()
        {
            using (var db = Context.Sql())
            {
                if (db.Database.Exists())
                {
                    db.Database.Delete();
                }
                db.Database.Create();

                for (int i = 0; i < insertSize; i++)
                {
                    var p = BlogPost.Create("T" + i, DateTime.Today.AddDays(i - 10000));
                    db.BlogPosts.Add(p);
                }



                db.SaveChanges();
            }
        }

        private static void SetupPostsBatch()
        {
            using (var db = Context.Sql())
            {
                var list = new List<BlogPost>(insertSize);
                for (int i = 0; i < insertSize; i++)
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
