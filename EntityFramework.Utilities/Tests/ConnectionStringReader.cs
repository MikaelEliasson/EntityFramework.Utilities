using Newtonsoft.Json;
using System.IO;

namespace Tests
{
    public static class ConnectionStringReader
    {
        private static ConnectionStrings connectionsStrings;

        public static ConnectionStrings ConnectionStrings
        {
            get
            {
                if (connectionsStrings == null)
                {
                    connectionsStrings = JsonConvert.DeserializeObject<ConnectionStrings>(File.ReadAllText("connectionStrings.json"));
                }
                return connectionsStrings;
            }
        }

    }
}
