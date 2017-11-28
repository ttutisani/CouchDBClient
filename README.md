[![Build status](https://ci.appveyor.com/api/projects/status/480lej89839hjc39/branch/master?svg=true)](https://ci.appveyor.com/project/ttutisani/couchdbclient/branch/master)

# CouchDBClient (CouchDB .NET Client)
This is a client framework for working with CouchDB from .NET code. It abstracts and simplifies the usage of CouchDB, so that you can easily use it from your application code. All the complexity of working with plain json and http request/responses is burried under the framework. You will be able create, retrieve, save, and delete documents as plain objects, by using this framework.

## Key Strengths of CouchDBClient Framework

* Fully object oriented.
* Fully unit tested.
* Fully async (uses async/await).
* Designed for simplicity in usage.

PS: unit tested using TDD (Test Driven Development), which ensures minimum number of code lines to get the job done.


## Full Function Reference

* __Server__
  * Get server info (`CouchDBServer.GetInfoAsync`)
  * Get handler interface for raw request/response calls (`CouchDBServer.GetHandler`)
* __Databases__
  * List all databases (`CouchDBServer.GetAllDbNamesAsync`)
  * Create database (`CouchDBServer.CreateDbAsync`)
  * Delete database (`CouchDBServer.DeleteDbAsync`)
  * Get database object to work with documents in it (`CouchDBServer.SelectDatabase`)
* __Documents__
  * Create new or update existing document
    * As string (`CouchDBDatabase.SaveStringDocumentAsync`)
    * As JObject (JSON object) (`CouchDBDatabase.SaveJsonDocumentAsync` overload)
    * As object (System.Object) (`CouchDBDatabase.SaveObjectDocumentAsync` overload)
  * Retrieve document by ID
    * As string (`CouchDBDatabase.GetStringDocumentAsync`)
    * As JObject (JSON object) (`CouchDBDatabase.GetJsonDocumentAsync`)
    * As generic TDocument (any type) (`CouchDBDatabase.GetObjectDocumentAsync<>`)
  * Retrieve multiple documents by ID list
    * As string documents (`CouchDBDatabase.GetStringDocumentsAsync`)
    * As JObject documents (JSON object) (`CouchDBDatabase.GetJsonDocumentsAsync`)
    * As generic TDocument (any type) (`CouchDBDatabase.GetObjectDocumentsAsync<>`)
  * Retrieve all documents
    * As string documents (`CouchDBDatabase.GetAllStringDocumentsAsync`)
    * As JObject documents (JSON object) (`CouchDBDatabase.GetAllJsonDocumentsAsync`)
    * As generic TDocument (any type) (`CouchDBDatabase.GetAllObjectDocumentsAsync<>`)
  * Delete document
    * By ID and Revision (`CouchDBDatabase.DeleteDocumentAsync`)
    * Given Document as JObject (JSON object) (`CouchDBDatabase.DeleteJsonDocumentAsync` overload)
  * Create, Update, or Delete multiple documents
    * As string (`CouchDBDatabase.SaveStringDocumentsAsync`)
    * As JObject (JSON Object) (`CouchDBDatabase.SaveJsonDocumentsAsync` overload)
    * As object (System.Object) (`CouchDBDatabase.SaveObjectDocumentsAsync` overload)
* __Document Attachments__
  * Create or Update attachment
    * As raw byte array (`CouchDBDatabase.SaveAttachmentAsync`)
  * Retrieve attachment
    * As raw byte array (`CouchDBDatabase.GetAttachmentAsync`)
  * Delete attachment
    * By document ID, attachment name and revision (`CouchDBDatabase.DeleteAttachmentAsync`)
* __Entitites__ (reusable documents) - any type implementing `IEntity`
  * Convert database object into entity store for working with entities (`EntityStore` constructor)
  * Create new or update existing entity (`EntityStore.SaveEntityAsync`)
  * Retrieve entity by ID (`EntityStore.GetEntityAsync<>`)
  * Retrieve multiple entities by ID list (`EntityStore.GetEntitiesAsync<>`)
  * Retrieve all entities (`EntityStore.GetAllEntitiesAsync<>`)
  * Delete entity (`EntityStore.DeleteEntityAsync`)
  * Create, Update, or Delete multiple entities (`EntityStore.SaveEntitiesAsync`)
* __Entity Attachments__
  * Create or Update attachment
    * As raw byte array (`EntityStore.SaveAttachmentAsync`)
  * Retrieve attachment
    * As raw byte array (`EntityStore.GetAttachmentAsync`)
  * Delete attachment
    * By entity instance and attachment name (`EntityStore.DeleteAttachmentAsync`)
* __Raw Request / Response__
  * Get handler interface for raw request/response calls (`CouchDBServer.GetHandler`)
  * Send raw request to relative URL on CouchDB: (`ICouchDBHandler.SendRequestAsync`)

## Examples

### Documents

You can work with documents as you own types (any POCO object, no inheritence necessary). CouchDBClient functions will serialize and deserialize from and to your types. This is the recommended approach with CouchDBClient framework, since it aims to promote object usage over the raw request/response/JSON usage. 

You can also use JObject (from Newtonsoft.Json library), which is an object representation of JSON data. Since JObject allows writing into it, CouchDBClient functions will modify _id and _rev values after every save operation (including deletion, which results in new _rev). This will give you a feeling of the consistency for your data - you won't have to assign the right values into _id and _rev before trying to save it once again (CouchDB requires correct values before trying to update the record). Refer to note about consistency of _id and _rev values below.

Most low level approach allows usage of string documents.

**Note about consistency of _id and _rev values**: this feature does not cover object and string documents, since the framework cannot write into these without additional serialization or reflection hit. These were considered as side effects, so in these cases you will have to assign the resulting _id and _rev values back into your documents in case of strings and objects. JObject document will have _id and _rev values updated after save operation (including deletion, which results in new _rev). If you need consistency of _id and _rev together with the strongly typed object usage, consider turning your objects into _Entities_ (described below), which allows CouchDBClient framework to rely on existence of _id and _rev values in your data, so it will keep them consistent, similar to how it does for JObject.


``` C#
using (var server = new CouchDBServer("http://localhost:5984"))
{
    using (var db = server.SelectDatabase("my-db"))
    {
        // Create document in one of many ways.
        await db.SaveObjectDocumentAsync(new { city = "Austin" });
        await db.SaveObjectDocumentAsync(new Address { city = "Austin" });
        await db.SaveJsonDocumentAsync(new JObject { ["city"] = "Austin" });
        await db.SaveStringDocumentAsync("{ \"city\": \"Austin\" }");
        
        // Update document in one of many ways.
        // (just add _id and _rev from database or from Response object of creation function).
        await db.SaveObjectDocumentAsync(new { _id = "123", _rev = "1-rev", city = "Austin" });
        await db.SaveObjectDocumentAsync(new Address { _id = "123", _rev = "1-rev", city = "Austin" });
        await db.SaveJsonDocumentAsync(new JObject { ["_id"] = "123", ["_rev"] = "1-rev", ["city"] = "Austin" });
        await db.SaveStringDocumentAsync("{ \"_id\": \"123\", \"_rev\": \"1-rev\", \"city\": \"Austin\" }");
        
        // Retrieve document in one of many ways.
        Address document = await db.GetObjectDocumentAsync<Address>("123");
        JObject document = await db.GetJsonDocumentAsync("123");
        string document = await db.GetStringDocumentAsync("123");

        // Delete document in one of many ways.
        await db.DeleteDocumentAsync("123", "1-rev");
        await db.DeleteJsonDocumentAsync(new JObject { ["_id"] = "123", ["_rev"] = "1-rev", ["city"] = "Austin" });
        
        // Get all documents in one of many ways.
        DocListResponse<Address> documents = await db.GetAllObjectDocumentsAsync<Address>();
        DocListResponse<JObject> documents = await db.GetAllJsonDocumentsAsync();
        DocListResponse<string> documents = await db.GetAllStringDocumentsAsync();
        
        // Get multiple documents in one of many ways.
        DocListResponse<Address> documents = await db.GetObjectDocumentsAsync<Address>(new [] { "id-1", "id-2", "id=3" });
        DocListResponse<JObject> documents = await db.GetJsonDocumentsAsync(new [] { "id-1", "id-2", "id=3" });
        DocListResponse<string> documents = await db.GetStringDocumentsAsync(new [] { "id-1", "id-2", "id=3" });
        
        // Create, Update, or Delete multiple documents in one go.
        await db.SaveObjectDocumentsAsync(new [] {
            new { city = "Austin" }, /* will be created */
            new { _id = "123", _rev = "1-rev", city = "New York" }, /* will be updated */
            new { _id = "123", _rev = "1-rev", _deleted = true, city = "Lost" } /* will be deleted */
            });
        
        await db.SaveJsonDocumentsAsync(new [] {
            new JObject { ["city"] = "Austin" }, /* will be created */
            new JObject { ["_id"] = "123", ["_rev"] = "1-rev", ["city"] = "New York" }, /* will be updated */
            new JObject { ["_id"] = "123", ["_rev"] = "1-rev", ["_deleted"] = true, ["city"] = "Lost" } /* will be deleted */
            });
            
        await db.SaveStringDocumentsAsync(new [] {
            "{ \"city\": \"Austin\" }", /* will be created */
            "{ \"_id\": \"123\", \"_rev\": \"1-rev\", \"city\": \"New York\" }", /* will be updated */
            "{ \"_id\": \"123\", \"_rev\": \"1-rev\", \"_deleted\": true, \"city\": \"Lost\" }" /* will be deleted */
            });
    }
}
```

### Entities (reusable documents)

Entities are a culmination of object oriented abstractions in CouchDBClient framework. CouchDB itself does not have such notion as Entity. I introduced this concept to support auto-update of the _id and _rev values into the documents. This comes with the price that the document objects need to implement `IEntity` interface, which just requires existence of _id and _rev read-write properties. Once you implement `IEntity` interface, you can use the same object over and over again in all save operations (including deletion).

As you know, CouchDB requires correct _id and _rev values with every operation, which means you need to assign new values (at least to _rev) when trying to use the same document object in next save operations. Entities will have automatically updated _id and _rev values after every operation, so you won't have to keep them up-to-date for using the same document instance in next save operations.

For example:
``` C#
public sealed class SampleEntity : IEntity
{
    // IEntity members implemented.
    public string _id { get; set; }
    public string _rev { get; set; }
    
    // Additional properties of SampleEntity document.
    public string Text { get; set; }
    public int Number { get; set; }
}

using (var server = new CouchDBServer("http://localhost:5984"))
{
    using (var db = server.SelectDatabase("my-db"))
    {
        // Treat database as Entity store.
        var store = new EntityStore(db);
    
        // create entity (_id is optional, so let's skip it).
        var entity = new SampleEntity { Text = "This is text", Number = 123 };
        
        // save #1.
        await store.SaveEntityAsync(entity);
        
        // just change entity's properties.
        entity.Text = "This is AWESOME";
        entity.Number = 321;
        
        // and save #2.
        // if this was not Entity, you would end up creating new document again,
        // unless you assign same _id and _rev.
        // Now you don't need all that, this updates same document!
        await store.SaveEntityAsync(entity);
        
        // bored? just delete the entity.
        // if this was not Entity, you would receive error from CouchDB, asking for correct _id and _rev.
        // Now you don't need all that, this deletes same document!
        await store.DeleteEntityAsync(entity);
        
        // all calls succeeded, no CouchDB errors received, long live Entities!
    }
}
```

## Building & Running the Code

I'm currently using Visual Studio 2015 Community Edition. As long as you have that, you will be able to compile and run the code out of the box.

Solution contains a demo project in it, which is a console app.
Just run that demo, and it will ask you to enter one of the supported commands into the console.

Demo assumes couple of things by default:

* You should have CouchDB running under [http://localhost:5984/](http://localhost:5984/).
* There must be an existing CouchDB database named 'my-db'

If any of these assumptions is not met, demo app will still run, but the commands will fail when you run them.


## Extensibility Points

If you can't find a needed functionality implemented by the CouchDBClinet framework, you can still make a raw HTTP call to any relative URL on the CouchDB instance.

For example, if CouchDB server URL is http://localhost:5984/, then this is how you can send a GET request to http://localhost:5984/views URL (i.e. `/views` relative URL):

``` C#
var server = new CouchDBServer("http://localhost:5984");
var handler = server.GetHandler();

var response = await handler.SentRequestAsync("views", RequestMethod.GET, Request.Empty);
HttpResponseMessage rawResponse = response.GetHttpResponseMessage();
```

Resulting `HttpResponseMessage` type ships with .NET Framework, so you are free to do anything with it, such as read content as string or as byte array.


## New Feature Requests

If you think there is an important feature which you are looking for and I need to prioritize, just open an issue. Or you can directly send me an email: tengo_tutisani [at] hotmail [dot] com. 

Every comment or suggestion will be considered seriously.


## Contributing Rules

First, check the Projects tab. I have tasked out things that need to be done.
If what you intend to do is not in the existing todo list, contact me to align your goals with the overall goals. You can do so by opening an issue or directly writing to my email - tengo_tutisani [at] hotmail [dot] com.
Once the task is in place, and we both know what you want to contribute, you can go on with next steps.

Fork -> Write code -> Commit to your fork -> Pull request -> wait until I review and merge.

I value quality over quantity, so please be patient with the thorough code review process with me.
I appreciate your valuable contributions!
