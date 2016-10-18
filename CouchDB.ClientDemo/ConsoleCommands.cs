using CouchDB.Client;
using System;
using System.Linq;

namespace CouchDB.ClientDemo
{
    public sealed class ConsoleCommands
    {
        public void info()
        {
            UsingServer(server =>
            {
                var serverInfo = server.GetInfo();

                Console.WriteLine(Serialize(serverInfo));
            });
        }

        public void alldbs()
        {
            UsingServer(server => 
            {
                var allDbs = server.GetAllDbNames();

                Console.WriteLine(Serialize(allDbs));
            });
        }

        private static string Serialize(object any)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(any);
        }

        private static void UsingServer(Action<CouchDBServer> body)
        {
            using (var server = new CouchDBServer("http://localhost:5984"))
            {
                body(server);
            }
        }
    }
}
