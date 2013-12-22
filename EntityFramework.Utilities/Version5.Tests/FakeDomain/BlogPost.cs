using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Version5.Tests.FakeDomain
{
    public class BlogPost
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public DateTime Created { get; set; }
        public int Reads { get; set; }

        public static BlogPost Create(string title, DateTime created)
        {
            return new BlogPost
            {
                Title = title,
                Created = created
            };
        }

        public static BlogPost Create(string title)
        {
            return new BlogPost
            {
                Title = title,
                Created = DateTime.Now
            };
        }

    }
}
