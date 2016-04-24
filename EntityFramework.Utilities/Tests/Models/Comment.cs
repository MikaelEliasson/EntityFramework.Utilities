using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests.FakeDomain.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int PostId { get; set; }
        public virtual BlogPost Post { get; set; }

        public static Comment Create(BlogPost blogPost, string comment)
        {
            return new Comment
            {
                Post = blogPost,
                PostId = blogPost.ID,
                Text = comment
            };
        }
    }
}
