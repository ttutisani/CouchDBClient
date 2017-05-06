using CouchDB.Client;
using DotNetExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CouchDB.ClientDemo
{
    public sealed class ConsoleCommands
    {
        private void UsingServer(Action<CouchDBServer> body)
        {
            using (var server = new CouchDBServer(_serverUrl))
            {
                try
                {
                    body(server);
                }
                catch (CouchDBClientException ex)
                {
                    Console.WriteLine("Error: {0}, Response object: {1}.", ex.Message, SerializationHelper.Serialize(ex.ServerResponse));
                }
                catch (AggregateException ae) when (ae.InnerException is CouchDBClientException)
                {
                    var ex = ae.InnerException as CouchDBClientException;
                    Console.WriteLine("Error: {0}, Response object: {1}.", ex.Message, SerializationHelper.Serialize(ex.ServerResponse));
                }
            }
        }

        private string _serverUrl = "http://localhost:5984/";

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

        private void UsingEntityStore(Action<EntityStore> body)
        {
            UsingDatabase(db =>
            {
                var store = new EntityStore(db);
                body(store);
            });
        }

        private string _dbName = "my-db";

        private sealed class SampleDoc
        {
            public string _id { get; set; }

            public string author { get; set; }

            public string _rev { get; set; }
        }

        private sealed class AuthorInfo
        {
            public string _Id { get; set; }

            public string Author { get; set; }

            public string _Rev { get; set; }
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

        private sealed class SampleEntity : IEntity
        {
            public string _id { get; set; }

            public string _rev { get; set; }

            public string Author { get; set; }

            public int Age { get; set; }

            public void UpdateFromConsole()
            {
                Console.WriteLine("Id:");
                _id = Console.ReadLine().IfNull(_id);

                Console.WriteLine("Revision:");
                _rev = Console.ReadLine().IfNull(_rev);

                Console.WriteLine("Author:");
                Author = Console.ReadLine().IfNull(Author);

                Console.WriteLine("Age:");
                Age = Console.ReadLine().ToInt();
            }
        }

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
                Console.WriteLine("Enter new DB name followed by <ENTER>:");
                var dbName = Console.ReadLine();

                server.CreateDbAsync(dbName).GetAwaiter().GetResult();
                Console.WriteLine("Just created DB named '{0}'.", dbName);  
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
                Console.WriteLine("Enter DB name followed by <ENTER>:");
                var dbName = Console.ReadLine();

                server.DeleteDbAsync(dbName).GetAwaiter().GetResult();
                Console.WriteLine("Just deleted DB '{0}'.", dbName);
            });
        }

        public void SetServer()
        {
            Console.WriteLine("Enter server url:");
            var serverUrl = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(serverUrl))
                serverUrl = _serverUrl;

            _serverUrl = serverUrl;
            Console.WriteLine("Server url was set to '{0}'.", _serverUrl);
        }

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
            Console.WriteLine("Enter doc JSON:");
            var docJson = Console.ReadLine();

            UsingDatabase(db => 
            {
                
                var response = db.SaveDocumentAsync(docJson).Result;
                Console.WriteLine("Successfully saved document. Id: '{0}', Rev: '{1}'.", response.Id, response.Revision);
                
            });
        }

        public void SaveJson()
        {
            Console.WriteLine("Enter Id (optional):");
            var id = Console.ReadLine();
            Console.WriteLine("Enter Author:");
            var author = Console.ReadLine();
            Console.WriteLine("Enter revision (optional):");
            var revision = Console.ReadLine();

            JObject obj = new JObject();
            if (!string.IsNullOrWhiteSpace(id)) obj["_id"] = id;
            obj["author"] = author;
            if (!string.IsNullOrWhiteSpace(revision)) obj["_rev"] = revision;

            UsingDatabase(db => 
            {
                
                db.SaveDocumentAsync(obj).GetAwaiter().GetResult();
                Console.WriteLine("Successfully saved document '{0}'.", obj.ToString());
            });
        }

        public void GetDoc()
        {
            Console.WriteLine("Enter Document id:");
            var docId = Console.ReadLine();

            UsingDatabase(db => 
            {
                
                var doc = db.GetDocumentJsonAsync(docId).Result;
                Console.WriteLine("Found document:");
                Console.WriteLine(doc);
                Console.WriteLine("End of document.");
            });
        }

        public void GetObj()
        {
            Console.WriteLine("Enter document id:");
            var docId = Console.ReadLine();

            UsingDatabase(db => 
            {
                var obj = db.GetDocumentAsync<SampleDoc>(docId).Result;

                Console.WriteLine("Found document:");
                Console.WriteLine(SerializationHelper.Serialize(obj));
                Console.WriteLine("End of document.");
            });
        }

        public void GetDocWithRevs()
        {
            Console.WriteLine("Enter Document id:");
            var docId = Console.ReadLine();

            UsingDatabase(db => 
            {
                var doc = db.GetDocumentAsync(docId, new DocQueryParams { Revs = true, Revs_Info = true }).Result;
                Console.WriteLine("Found document:");
                Console.WriteLine(doc);
                Console.WriteLine("End of document.");
            });
        }

        public void GetDocOpenRevs()
        {
            Console.WriteLine("Enter Document id:");
            var docId = Console.ReadLine();

            UsingDatabase(db =>
            {
                var doc = db.GetDocumentAsync(docId, new DocQueryParams { Open_Revs = new DocQueryParams.OpenRevs() }).Result;
                Console.WriteLine("Found document:");
                Console.WriteLine(doc);
                Console.WriteLine("End of document.");
            });
        }

        public void GetAllDocs()
        {
            UsingDatabase(db => 
            {
                ListQueryParams qParams = new ListQueryParams { Include_Docs = true };

                var allDocs = db.GetAllStringDocumentsAsync(qParams).Result;
                Console.WriteLine("Found {0} docs:", allDocs.Rows.Count);

                foreach (var row in allDocs.Rows)
                {
                    Console.WriteLine(row.Document);
                    Console.WriteLine("----------");
                }

                Console.WriteLine("End of list.");
            });
        }

        public void GetAllDocsJson()
        {
            UsingDatabase(db =>
            {
                ListQueryParams qParams = new ListQueryParams { Include_Docs = true };

                var allDocs = db.GetAllJsonDocumentsAsync(qParams).Result;
                Console.WriteLine("Found {0} docs:", allDocs.Rows.Count);

                foreach (var row in allDocs.Rows)
                {
                    Console.WriteLine(row.Document.ToString());
                    Console.WriteLine("----------");
                }

                Console.WriteLine("End of list.");
            });
        }

        public void GetAllDocsObj()
        {
            UsingDatabase(db =>
            {
                var allDocs = db.GetAllObjectDocumentsAsync<AuthorInfo>(queryParams: new ListQueryParams { Include_Docs = true }).Result;
                Console.WriteLine("Found {0} docs:", allDocs.Rows.Count);

                foreach (var row in allDocs.Rows)
                {
                    Console.WriteLine("ID: {0}", row.Document._Id);
                    Console.WriteLine("Rev: {0}", row.Document._Rev);
                    Console.WriteLine("Author: {0}", row.Document.Author);
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
                var allDocs = db.GetAllObjectDocumentsAsync<AuthorInfo>(queryParams: new ListQueryParams { Include_Docs = true, Skip = skip, Limit = limit }).Result;
                Console.WriteLine("Found {0} docs:", allDocs.Rows.Count);

                foreach (var row in allDocs.Rows)
                {
                    Console.WriteLine("ID: {0}", row.Document._Id);
                    Console.WriteLine("Rev: {0}", row.Document._Rev);
                    Console.WriteLine("Author: {0}", row.Document.Author);
                    Console.WriteLine("----------");
                }

                Console.WriteLine("End of list.");
            });
        }

        public void GetAllDocsNice()
        {
            UsingDatabase(db =>
            {
                var allDocs = db.GetAllObjectDocumentsAsync<NiceAuthorInfo>(queryParams: new ListQueryParams { Include_Docs = true }, deserializer: jObject => new NiceAuthorInfo(jObject)).Result;
                Console.WriteLine("Found {0} docs:", allDocs.Rows.Count);

                foreach (var row in allDocs.Rows)
                {
                    Console.WriteLine("ID: {0}", row.Document.Id);
                    Console.WriteLine("Author: {0}", row.Document.Author);
                    Console.WriteLine("----------");
                }

                Console.WriteLine("End of list.");
            });
        }

        public void DeleteDoc()
        {
            Console.WriteLine("Enter doc id:");
            var docId = Console.ReadLine();
            Console.WriteLine("Enter revision:");
            var revision = Console.ReadLine();

            UsingDatabase(db => 
            {
                var result = db.DeleteDocumentAsync(docId, revision).GetAwaiter().GetResult();
                Console.WriteLine("Response: {0}", SerializationHelper.Serialize(result));
            });
        }

        public void GetDocsJson()
        {
            Console.WriteLine("Enter doc IDs, one at a time. Press <Enter> when done:");
            var docIdList = new List<string>();
            do
            {
                var id = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(id))
                    docIdList.Add(id);
                else
                    break;

            } while (true);

            UsingDatabase(db => 
            {
                ListQueryParams qParams = new ListQueryParams { Include_Docs = true };

                var allDocs = db.GetJsonDocumentsAsync(docIdList.ToArray(), qParams).GetAwaiter().GetResult();
                Console.WriteLine("Found {0} docs:", allDocs.Rows.Count);

                foreach (var row in allDocs.Rows)
                {
                    Console.WriteLine(SerializationHelper.Serialize(row));
                    Console.WriteLine("----------");
                }

                Console.WriteLine("End of list.");
            });
        }

        public void SaveStringDocs()
        {
            var docs = new List<string>();

            Console.WriteLine("Enter docs, one at a time. Empty to finish.");
            do
            {
                var doc = Console.ReadLine();
                if (doc.IsNullOrWhiteSpace())
                    break;

                docs.Add(doc);

            } while (true);

            if (docs.Count == 0)
                return;

            Console.WriteLine("New edits? true/false");
            var newEdits = Console.ReadLine().ToBool();

            UsingDatabase(db => 
            {
                var response = db.SaveDocumentsAsync(docs.ToArray(), newEdits).GetAwaiter().GetResult();
                Console.WriteLine($"Response: {SerializationHelper.Serialize(response)}");
            });
        }

        public void SaveJSONDocs()
        {
            var docs = new List<JObject>();

            Console.WriteLine("Enter docs, one at a time. Empty to finish.");
            do
            {
                var doc = Console.ReadLine();
                if (doc.IsNullOrWhiteSpace())
                    break;

                docs.Add(JObject.Parse(doc));

            } while (true);

            if (docs.Count == 0)
                return;

            Console.WriteLine("New edits? true/false");
            var newEdits = Console.ReadLine().ToBool();

            UsingDatabase(db =>
            {
                var response = db.SaveDocumentsAsync(docs.ToArray(), newEdits).GetAwaiter().GetResult();
                Console.WriteLine($"Response: {SerializationHelper.Serialize(response)}");
            });
        }

        public void SaveObjectDocs()
        {
            var docs = new List<object>();

            Console.WriteLine("Enter docs, one at a time. Empty to finish.");
            do
            {
                var doc = Console.ReadLine();
                if (doc.IsNullOrWhiteSpace())
                    break;

                var authorInfo = JsonConvert.DeserializeObject<AuthorInfo>(doc);

                docs.Add(new { _id = authorInfo._Id, _rev = authorInfo._Rev, author = authorInfo.Author });

            } while (true);

            if (docs.Count == 0)
                return;

            Console.WriteLine("New edits? true/false");
            var newEdits = Console.ReadLine().ToBool();

            UsingDatabase(db =>
            {
                var response = db.SaveDocumentsAsync(docs.ToArray(), newEdits).GetAwaiter().GetResult();
                Console.WriteLine($"Response: {SerializationHelper.Serialize(response)}");
            });
        }

        //=============== ENTITIES =================================

        public void PlayWithNewEntity()
        {
            UsingEntityStore(db =>
            {
                Console.WriteLine("Enter new entity info.");
                var entity = new SampleEntity();
                entity.UpdateFromConsole();

                db.SaveEntityAsync(entity).GetAwaiter().GetResult();

                Console.WriteLine("Saved to db:");
                Console.WriteLine(SerializationHelper.Serialize(entity));

                Console.WriteLine("Update? press <Enter> to skip.");
                if (!string.IsNullOrWhiteSpace(Console.ReadLine()))
                {
                    Console.WriteLine("Enter entity info to update.");
                    entity.UpdateFromConsole();

                    db.SaveEntityAsync(entity).GetAwaiter().GetResult();

                    Console.WriteLine("Saved to db:");
                    Console.WriteLine(SerializationHelper.Serialize(entity));
                }

                Console.WriteLine("Delete? press <Enter> to skip.");
                if (!string.IsNullOrWhiteSpace(Console.ReadLine()))
                {
                    db.DeleteEntityAsync(entity).GetAwaiter().GetResult();

                    Console.WriteLine("deleted:");
                    Console.WriteLine(SerializationHelper.Serialize(entity));
                }
            });
        }

        public void PlayWithExistingEntity()
        {
            UsingEntityStore(db =>
            {
                Console.WriteLine("Enter entity id:");
                var id = Console.ReadLine();

                var entity = db.GetEntityAsync<SampleEntity>(id).GetAwaiter().GetResult();
                Console.WriteLine("Found:");
                Console.WriteLine(SerializationHelper.Serialize(entity));

                Console.WriteLine("Update? press <Enter> to skip.");
                if (!string.IsNullOrWhiteSpace(Console.ReadLine()))
                {
                    Console.WriteLine("Enter entity info to update.");
                    entity.UpdateFromConsole();

                    db.SaveEntityAsync(entity).GetAwaiter().GetResult();

                    Console.WriteLine("Saved to db:");
                    Console.WriteLine(SerializationHelper.Serialize(entity));
                }

                Console.WriteLine("Delete? press <Enter> to skip.");
                if (!string.IsNullOrWhiteSpace(Console.ReadLine()))
                {
                    db.DeleteEntityAsync(entity).GetAwaiter().GetResult();

                    Console.WriteLine("deleted:");
                    Console.WriteLine(SerializationHelper.Serialize(entity));
                }
            });
        }

        public void GetAllEntities()
        {
            UsingEntityStore(db =>
            {
                Console.WriteLine("Enter skip (empty for 0):");
                int skip;
                int.TryParse(Console.ReadLine(), out skip);

                Console.WriteLine("Enter limit (empty for all):");
                int limit;
                if (!int.TryParse(Console.ReadLine(), out limit)) limit = int.MaxValue;

                var allEntities = db.GetAllEntitiesAsync<SampleEntity>(new ListQueryParams { Skip = skip, Limit = limit }).GetAwaiter().GetResult();

                Console.WriteLine($"Found {allEntities.Rows.Count} entities:");
                foreach (var entity in allEntities.Rows)
                {
                    Console.WriteLine(SerializationHelper.Serialize(entity));
                    Console.WriteLine("-------------------------------------");
                }
                Console.WriteLine("End of list.");
            });
        }

        public void GetEntities()
        {
            Console.WriteLine("Enter entity IDs, one at a time. Press <Enter> when done:");
            var entityIdList = new List<string>();
            do
            {
                var id = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(id))
                    entityIdList.Add(id);
                else
                    break;

            } while (true);

            UsingEntityStore(db =>
            {
                Console.WriteLine("Enter skip (empty for 0):");
                int skip;
                int.TryParse(Console.ReadLine(), out skip);

                Console.WriteLine("Enter limit (empty for all):");
                int limit;
                if (!int.TryParse(Console.ReadLine(), out limit)) limit = int.MaxValue;

                var entities = db.GetEntitiesAsync<SampleEntity>(entityIdList.ToArray(), new ListQueryParams { Skip = skip, Limit = limit }).GetAwaiter().GetResult();

                Console.WriteLine($"Found {entities.Rows.Count} entities:");
                foreach (var entity in entities.Rows)
                {
                    Console.WriteLine(SerializationHelper.Serialize(entity));
                    Console.WriteLine("-------------------------------------");
                }
                Console.WriteLine("End of list.");
            });
        }

        public void SaveEntities()
        {
            var entities = new List<IEntity>();

            Console.WriteLine("Enter entities, one at a time. Empty to finish.");
            do
            {
                var doc = Console.ReadLine();
                if (doc.IsNullOrWhiteSpace())
                    break;

                var entity = JsonConvert.DeserializeObject<SampleEntity>(doc);

                entities.Add(entity);

            } while (true);

            if (entities.Count == 0)
                return;

            Console.WriteLine("New edits? true/false");
            var newEdits = Console.ReadLine().ToBool();

            UsingDatabase(db =>
            {
                var response = db.SaveDocumentsAsync(entities.ToArray(), newEdits).GetAwaiter().GetResult();
                Console.WriteLine($"Response: {SerializationHelper.Serialize(response)}");
            });
        }
    }
}
