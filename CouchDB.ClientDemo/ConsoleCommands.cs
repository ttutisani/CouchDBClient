using CouchDB.Client;
using System;

namespace CouchDB.ClientDemo
{
    public sealed class ConsoleCommands
    {
        public void info()
        {
            UsingServer(server =>
            {
                var serverInfo = server.GetInfo().Result;

                Console.WriteLine(Serialize(serverInfo));
            });
        }

        public void adddb()
        {
            UsingServer(server => 
            {
                try
                {
                    Console.WriteLine("Enter new DB name followed by <ENTER>:");
                    var dbName = Console.ReadLine();

                    server.CreateDb(dbName).GetAwaiter().GetResult();
                    Console.WriteLine("Just created DB named '{0}'.", dbName);
                }
                catch (CouchDBClientException ex)
                {
                    Console.WriteLine("Error: {0}, Response object: {1}.", ex.Message, Serialize(ex.ServerResponse));
                }
            });
        }

        public void alldbs()
        {
            UsingServer(server => 
            {
                var allDbs = server.GetAllDbNames().Result;

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
