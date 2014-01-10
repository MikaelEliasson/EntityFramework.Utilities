using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests.FakeDomain
{
    public class PhoneNumber
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public Guid ContactId { get; set; }
        public Contact Contact { get; set; }
    }
}
