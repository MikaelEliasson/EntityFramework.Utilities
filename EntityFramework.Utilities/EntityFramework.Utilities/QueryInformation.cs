
namespace EntityFramework.Utilities
{
    public class QueryInformation
    {
        public string Schema { get; set; }
        public string Table { get; set; }
        public string Alias { get; set; }
        public string WhereSql { get; set; }
    }
}
