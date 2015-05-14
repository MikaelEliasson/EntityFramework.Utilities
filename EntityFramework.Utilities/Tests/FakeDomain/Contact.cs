using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests.FakeDomain
{
    public class Contact : Person
    {
        public string Title { get; set; }
        public ICollection<PhoneNumber> PhoneNumbers { get; set; }
        public ICollection<Email> Emails { get; set; }

        public static Contact Build(string firstname, string lastname, string title, DateTime? birthdate = null)
        {
            return new Contact
            {
                Id = Guid.NewGuid(),
                FirstName = firstname,
                LastName = lastname,
                Title = title,
                BirthDate = birthdate ?? DateTime.Today
            };
        }
    }
}
