﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Version5.Tests.FakeDomain
{
    public class ReorderedBlogPost
    {
        public int ID { get; set; }
        public DateTime Created { get; set; }
        public string Title { get; set; } //<--- Reversed order of this and created for Batch Insert testing
        public int Reads { get; set; }

    }
}
