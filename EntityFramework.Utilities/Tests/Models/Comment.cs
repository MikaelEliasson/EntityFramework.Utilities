using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Tests.FakeDomain.Models
{
    [Table("Comment")]
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public int PostId { get; set; }
        public BlogPost Post { get; set; }
    }

    [Table("ApprovedComment")]
    public class ApprovedComment:Comment
    {
        public DateTime ApprovedOn { get; set; }
    }
}
