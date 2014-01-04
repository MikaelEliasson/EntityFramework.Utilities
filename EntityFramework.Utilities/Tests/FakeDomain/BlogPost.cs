using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests.FakeDomain
{
    public class BlogPost
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public DateTime Created { get; set; }
        public int Reads { get; set; }
        public AuthorInfo Author { get; set; }

        public static BlogPost Create(string title, DateTime created)
        {
            return new BlogPost
            {
                Title = title,
                Created = created,
                Author = new AuthorInfo
                {
                    Email = "m@m.com",
                    Name = "name"
                }

            };
        }

        public static BlogPost Create(string title)
        {
            return new BlogPost
            {
                Title = title,
                Created = DateTime.Now,
                Author = new AuthorInfo
                {
                    Email = "m@m.com",
                    Name = "name"
                }
            };
        }

    }

    public class AuthorInfo
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
