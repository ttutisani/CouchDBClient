﻿using CouchDB.Client;
using System;

namespace CouchDB.ClientDemo
{
    public sealed class ConsoleCommands
    {
        public void info()
        {
            UsingServer(server =>
            {
                var serverInfo = server.GetInfoAsync().Result;

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

                    server.CreateDbAsync(dbName).GetAwaiter().GetResult();
                    Console.WriteLine("Just created DB named '{0}'.", dbName);
                }
                catch (CouchDBClientException ex)
                {
                    Console.WriteLine("Error: {0}, Response object: {1}.", ex.Message, SerializationHelper.Serialize(ex.ServerResponse));
                }
            });
        }

        public void alldbs()
        {
            UsingServer(server => 
            {
                var allDbs = server.GetAllDbNamesAsync().Result;

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

                var allDbs = server.GetAllDbNamesAsync(new QueryParams { Limit = limit }).Result;
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

                    server.DeleteDbAsync(dbName).GetAwaiter().GetResult();
                    Console.WriteLine("Just deleted DB '{0}'.", dbName);
                }
                catch (CouchDBClientException ex)
                {
                    Console.WriteLine("Error: {0}, Response object: {1}.", ex.Message, SerializationHelper.Serialize(ex.ServerResponse));
                }
            });
        }

        private void UsingServer(Action<CouchDBServer> body)
        {
            using (var server = new CouchDBServer(_serverUrl))
            {
                body(server);
            }
        }

        private string _serverUrl = "http://localhost:5984/";

        public void SetServer()
        {
            Console.WriteLine("Enter server url:");
            var serverUrl = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(serverUrl))
                serverUrl = _serverUrl;

            _serverUrl = serverUrl;
            Console.WriteLine("Server url was set to '{0}'.", _serverUrl);
        }

        private void UsingDatabase(Action<CouchDBDatabase> body)
        {
            UsingServer(server => 
            {
                using (var db = server.SelectDatabase(_dbName))
                {
                    body(db);
                }
            });
        }

        private string _dbName = "my-db";

        public void SetDb()
        {
            Console.WriteLine("Enter db name:");
            var dbName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(dbName))
                dbName = _dbName;

            _dbName = dbName;
            Console.WriteLine("DB name was set to '{0}'.", _dbName);
        }

        public void SaveDoc()
        {
            Console.WriteLine("Enter document id:");
            var docId = Console.ReadLine();

            Console.WriteLine("Enter doc JSON:");
            var docJson = Console.ReadLine();

            UsingDatabase(db => 
            {
                try
                {
                    var response = db.SaveDocumentAsync(docId, docJson).Result;
                    Console.WriteLine("Successfully saved document '{0}'. Id: '{1}', Rev: '{2}'.", docId, response.Id, response.Revision);
                }
                catch (CouchDBClientException ex)
                {
                    Console.WriteLine("Error: {0}, Response object: {1}.", ex.Message, SerializationHelper.Serialize(ex.ServerResponse));
                }
            });
        }

        public void SaveObj()
        {
            Console.WriteLine("Enter Id:");
            var id = Console.ReadLine();
            Console.WriteLine("Enter Author:");
            var author = Console.ReadLine();
            Console.WriteLine("Enter revision:");
            var revision = Console.ReadLine();

            UsingDatabase(db => 
            {
                object obj = !string.IsNullOrWhiteSpace(revision)
                    ? (object)new
                    {
                        _id = id,
                        author = author,
                        _rev = revision
                    }
                    : new
                    {
                        _id = id,
                        author = author
                    };

                try
                {
                    var response = db.SaveDocumentAsync(obj).GetAwaiter().GetResult();
                    Console.WriteLine("Successfully saved document '{0}'. Id: '{1}', Rev: '{2}'.", id, response.Id, response.Revision);
                }
                catch (CouchDBClientException ex)
                {
                    Console.WriteLine("Error: {0}, Response object: {1}.", ex.Message, SerializationHelper.Serialize(ex.ServerResponse));
                }
            });
        }
    }
}
