using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntityFramework.Utilities
{
    public static class Configuration
    {
        static Configuration(){
            Log = m => { };
        }


        /// <summary>
        /// Allows you to hook in a logger to see debug messages for example
        /// </summary>
        public static Action<string> Log { get; set; }

    }
}
