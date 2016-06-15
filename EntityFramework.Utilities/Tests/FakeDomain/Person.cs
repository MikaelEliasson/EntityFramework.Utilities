using System;

namespace Tests.FakeDomain
{
    public class Person
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }

        public static Person Build(string firstname, string lastname, DateTime? birthdate = null)
        {
            return new Person
            {
                Id = Guid.NewGuid(),
                FirstName = firstname,
                LastName = lastname,
                BirthDate = birthdate ?? DateTime.Today
            };
        }
    }
}
