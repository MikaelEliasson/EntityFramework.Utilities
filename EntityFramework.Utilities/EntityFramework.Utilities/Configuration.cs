using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntityFramework.Utilities
{
    public static class Configuration
    {
        static Configuration(){
            Providers = new List<IQueryProvider>();
            Providers.Add(new SqlQueryProvider());

            Log = m => { };

            DisableDefaultFallback = true;
        
        }

        /// <summary>
        /// Add, Remove or replace query provider by modifing this collection
        /// </summary>
        public static ICollection<IQueryProvider> Providers { get; set; }

        /// <summary>
        /// Allows you to hook in a logger to see debug messages for example
        /// </summary>
        public static Action<string> Log { get; set; }

        /// <summary>
        /// If you want an exception to be thrown if the provider doesn't support the operation set this to true. 
        /// Otherwise it will fall back to the default EF behaviour meaning a performance penalty
        /// </summary>
        public static bool DisableDefaultFallback { get; set; }

    }
}
