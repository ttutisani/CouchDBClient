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

                Console.WriteLine("Info received:");
                Console.WriteLine("CouchDB --> {0}.", serverInfo.CouchDB);
                Console.WriteLine("Version --> {0}.", serverInfo.Version);
                Console.WriteLine("Vendor name --> {0}.", serverInfo.Vendor.Name);
                Console.WriteLine("End of message.");
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

                Console.WriteLine("Databases found:");
                foreach (var dbName in allDbs)
                {
                    Console.WriteLine("--> {0}", dbName);
                }
                Console.WriteLine("End of list.");
            });
        }

        public void alldbslimit()
        {
            UsingServer(server => 
            {
                Console.WriteLine("Enter limit:");
                int limit; int.TryParse(Console.ReadLine(), out limit);
                Console.WriteLine("Databases found with limit {0}:", limit);

                var allDbs = server.GetAllDbNames(new QueryParams { Limit = limit }).Result;
                foreach (var dbName in allDbs)
                {
                    Console.WriteLine("--> {0}", dbName);
                }
                Console.WriteLine("End of list.");
            });
        }

        public void deletedb()
        {
            UsingServer(server => 
            {
                try
                {
                    Console.WriteLine("Enter DB name followed by <ENTER>:");
                    var dbName = Console.ReadLine();

                    server.DeleteDb(dbName).GetAwaiter().GetResult();
                    Console.WriteLine("Just deleted DB '{0}'.", dbName);
                }
                catch (CouchDBClientException ex)
                {
                    Console.WriteLine("Error: {0}, Response object: {1}.", ex.Message, Serialize(ex.ServerResponse));
                }
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
