using CouchDB.Client;
using Newtonsoft.Json.Linq;
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

                var allDbs = server.GetAllDbNamesAsync(new ListQueryParams { Limit = limit }).Result;
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

        public void GetDoc()
        {
            Console.WriteLine("Enter Document id:");
            var docId = Console.ReadLine();

            UsingDatabase(db => 
            {
                try
                {
                    var doc = db.GetDocumentJsonAsync(docId).Result;
                    Console.WriteLine("Found document:");
                    Console.WriteLine(doc);
                    Console.WriteLine("End of document.");
                }
                catch (AggregateException ae) when (ae.InnerException is CouchDBClientException)
                {
                    var ex = ae.InnerException as CouchDBClientException;
                    Console.WriteLine("Error: {0}, Response object: {1}.", ex.Message, SerializationHelper.Serialize(ex.ServerResponse));
                }
            });
        }

        private sealed class SampleDoc
        {
            public string _id { get; set; }

            public string author { get; set; }

            public string _rev { get; set; }
        }

        public void GetObj()
        {
            Console.WriteLine("Enter document id:");
            var docId = Console.ReadLine();

            UsingDatabase(db => 
            {
                try
                {
                    var obj = db.GetDocumentAsync<SampleDoc>(docId).Result;

                    Console.WriteLine("Found document:");
                    Console.WriteLine(SerializationHelper.Serialize(obj));
                    Console.WriteLine("End of document.");
                }
                catch (AggregateException ae) when (ae.InnerException is CouchDBClientException)
                {
                    var ex = ae.InnerException as CouchDBClientException;
                    Console.WriteLine("Error: {0}, Response object: {1}.", ex.Message, SerializationHelper.Serialize(ex.ServerResponse));
                }
            });
        }

        public void GetDocWithRevs()
        {
            Console.WriteLine("Enter Document id:");
            var docId = Console.ReadLine();

            UsingDatabase(db => 
            {
                try
                {
                    var doc = db.GetDocumentAsync(docId, new DocQueryParams { Revs = true, Revs_Info = true }).Result;
                    Console.WriteLine("Found document:");
                    Console.WriteLine(doc);
                    Console.WriteLine("End of document.");
                }
                catch (AggregateException ae) when (ae.InnerException is CouchDBClientException)
                {
                    var ex = ae.InnerException as CouchDBClientException;
                    Console.WriteLine("Error: {0}, Response object: {1}.", ex.Message, SerializationHelper.Serialize(ex.ServerResponse));
                }
            });
        }

        public void GetDocOpenRevs()
        {
            Console.WriteLine("Enter Document id:");
            var docId = Console.ReadLine();

            UsingDatabase(db =>
            {
                try
                {
                    var doc = db.GetDocumentAsync(docId, new DocQueryParams { Open_Revs = new DocQueryParams.OpenRevs() }).Result;
                    Console.WriteLine("Found document:");
                    Console.WriteLine(doc);
                    Console.WriteLine("End of document.");
                }
                catch (AggregateException ae) when (ae.InnerException is CouchDBClientException)
                {
                    var ex = ae.InnerException as CouchDBClientException;
                    Console.WriteLine("Error: {0}, Response object: {1}.", ex.Message, SerializationHelper.Serialize(ex.ServerResponse));
                }
            });
        }

        public void GetAllDocs()
        {
            UsingDatabase(db => 
            {
                var allDocs = db.GetAllStringDocumentsAsync().Result;
                Console.WriteLine("Found {0} docs:", allDocs.Rows.Count);

                foreach (var doc in allDocs.Rows)
                {
                    Console.WriteLine(doc);
                    Console.WriteLine("----------");
                }

                Console.WriteLine("End of list.");
            });
        }

        public void GetAllDocsJson()
        {
            UsingDatabase(db =>
            {
                var allDocs = db.GetAllJsonDocumentsAsync().Result;
                Console.WriteLine("Found {0} docs:", allDocs.Rows.Count);

                foreach (var doc in allDocs.Rows)
                {
                    Console.WriteLine(doc.ToString());
                    Console.WriteLine("----------");
                }

                Console.WriteLine("End of list.");
            });
        }

        private sealed class AuthorInfo
        {
            public string _Id { get; set; }

            public string Author { get; set; }

            public string _Rev { get; set; }
        }

        public void GetAllDocsObj()
        {
            UsingDatabase(db =>
            {
                var allDocs = db.GetAllObjectDocumentsAsync<AuthorInfo>(extractDocumentAsObject: true, queryParams: new ListQueryParams { Include_Docs = true }).Result;
                Console.WriteLine("Found {0} docs:", allDocs.Rows.Count);

                foreach (var doc in allDocs.Rows)
                {
                    Console.WriteLine("ID: {0}", doc._Id);
                    Console.WriteLine("Rev: {0}", doc._Rev);
                    Console.WriteLine("Author: {0}", doc.Author);
                    Console.WriteLine("----------");
                }

                Console.WriteLine("End of list.");
            });
        }

        public void GetAllDocsLimit()
        {
            int skip;
            Console.WriteLine("Enter SKIP count or empty for default:");
            if (!int.TryParse(Console.ReadLine(), out skip)) skip = 0;

            int limit;
            Console.WriteLine("Enter LIMIT count or empty for default:");
            if (!int.TryParse(Console.ReadLine(), out limit)) limit = int.MaxValue;

            UsingDatabase(db =>
            {
                var allDocs = db.GetAllObjectDocumentsAsync<AuthorInfo>(extractDocumentAsObject: true, queryParams: new ListQueryParams { Include_Docs = true, Skip = skip, Limit = limit }).Result;
                Console.WriteLine("Found {0} docs:", allDocs.Rows.Count);

                foreach (var doc in allDocs.Rows)
                {
                    Console.WriteLine("ID: {0}", doc._Id);
                    Console.WriteLine("Rev: {0}", doc._Rev);
                    Console.WriteLine("Author: {0}", doc.Author);
                    Console.WriteLine("----------");
                }

                Console.WriteLine("End of list.");
            });
        }

        private sealed class NiceAuthorInfo
        {
            public string Id { get; }

            public string Author { get; }

            public NiceAuthorInfo(JObject from)
            {
                Id = from["_id"].ToString();
                Author = from["author"].ToString();
            }
        }

        public void GetAllDocsNice()
        {
            UsingDatabase(db =>
            {
                var allDocs = db.GetAllObjectDocumentsAsync<NiceAuthorInfo>(extractDocumentAsObject: true, queryParams: new ListQueryParams { Include_Docs = true }, deserializer: jObject => new NiceAuthorInfo(jObject)).Result;
                Console.WriteLine("Found {0} docs:", allDocs.Rows.Count);

                foreach (var doc in allDocs.Rows)
                {
                    Console.WriteLine("ID: {0}", doc.Id);
                    Console.WriteLine("Author: {0}", doc.Author);
                    Console.WriteLine("----------");
                }

                Console.WriteLine("End of list.");
            });
        }
    }
}
