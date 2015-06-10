using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests.FakeDomain.Models
{
    public class BlogPost
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string ShortTitle { get; set; }
        public DateTime Created { get; set; }
        public int Reads { get; set; }
        public AuthorInfo Author { get; set; }
        public ICollection<Comment> Comments { get; set; }


        public static BlogPost Create(string title, DateTime created)
        {
            return new BlogPost
            {
                Title = title,
                Created = created,
                Author = new AuthorInfo
                {
                    Email = "m@m.com",
                    Name = "name",
                    Address = new Address
                    {
                        Line1 = "Street",
                        Town = "Gothenburg",
                        ZipCode = "41654"
                    }
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
                    Name = "name",
                    Address = new Address
                    {
                        Line1 = "Street",
                        Town = "Gothenburg",
                        ZipCode = "41654"
                    }
                }
            };
        }

    }

    public class AuthorInfo
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public Address Address { get; set; }
    }

    public class Address
    {
        public string Line1 { get; set; }
        public string ZipCode { get; set; }
        public string Town { get; set; }
    }
}
