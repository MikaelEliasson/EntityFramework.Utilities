
using System.Diagnostics;
namespace EntityFramework.Utilities
{
    [DebuggerDisplay("NameOnObject = {NameOnObject} NameInDatabase = {NameInDatabase}")]
    public class ColumnMapping
    {
        public string NameOnObject { get; set; }
        public string NameInDatabase { get; set; }
    }
}
