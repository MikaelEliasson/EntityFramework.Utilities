using System.Linq;

namespace Tests.FakeDomain
{
    public static class Queries
    {
        public static IQueryable<Contact> FilterByName(IQueryable<Contact> contact, string firstName)
        {
            return contact.Where(x => x.FirstName == firstName);
        }

        public static IQueryable<Contact> FilterByAsExtensionMethod(this IQueryable<Contact> contact, string firstName)
        {
            return contact.Where(x => x.FirstName == firstName);
        }

        public static IQueryable<Contact> FilterByNameInReversedOrder(string firstName, IQueryable<Contact> contact)
        {
            return contact.Where(x => x.FirstName == firstName);
        }
    }
}
