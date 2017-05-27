# CouchDBClient
CouchDB .NET Client - object oriented, easy to use, intuitive, self-describing library.


## Features

* Server
  * Get server info (`CouchDBServer.GetInfoAsync`)
* Databases
  * List all databases (`CouchDBServer.GetAllDbNamesAsync`)
  * Create database (`CouchDBServer.CreateDbAsync`)
  * Delete database (`CouchDBServer.DeleteDbAsync`)
  * Get database object to work with documents in it (`CouchDBServer.SelectDatabase`)
* Documents
  * Create new or update existing document
    * As string (`CouchDBDatabase.SaveDocumentAsync`)
    * As JObject (JSON object) (`CouchDBDatabase.SaveDocumentAsync` overload)
    * As object (System.Object) (`CouchDBDatabase.SaveDocumentAsync` overload)
  * Retrieve document by ID
    * As string (`CouchDBDatabase.GetDocumentAsync`)
    * As JObject (JSON object) (`CouchDBDatabase.GetDocumentJsonAsync`)
    * As generic TDocument (any type) (`CouchDBDatabase.GetDocumentAsync<>`)
  * Retrieve multiple documents by ID list
    * As string documents (`CouchDBDatabase.GetStringDocumentsAsync`)
    * As JObject documents (JSON object) (`CouchDBDatabase.GetJsonDocumentsAsync`)
    * As generic TDocument (any type) (`CouchDBDatabase.GetObjectDocumentsAsync<>`)
  * Retrieve all documents
    * As string documents (`CouchDBDatabase.GetAllDocumentsAsync`)
    * As JObject documents (JSON object) (`CouchDBDatabase.GetAllJsonDocumentsAsync`)
    * As generic TDocument (any type) (`CouchDBDatabase.GetAllObjectDocumentsAsync<>`)
  * Delete document
    * By ID and Revision (`CouchDBDatabase.DeleteDocumentAsync`)
    * Given Document as JObject (JSON object) (`CouchDBDatabase.DeleteDocumentAsync` overload)
  * Create, Update, or Delete multiple documents
    * As string (`CouchDBDatabase.SaveDocumentsAsync`)
    * As JObject (JSON Object) (`CouchDBDatabase.SaveDocumentsAsync` overload)
    * As object (System.Object) (`CouchDBDatabase.SaveDocumentsAsync` overload)
* Entitites (reusable documents) - any type implementing `IEntity`
  * Convert database object into entity store for working with entities (`EntityStore` constructor)
  * Create new or update existing entity (`EntityStore.SaveEntityAsync`)
  * Retrieve entity by ID (`EntityStore.GetEntityAsync<>`)
  * Retrieve multiple entities by ID list (`EntityStore.GetEntitiesAsync<>`)
  * Retrieve all entities (`EntityStore.GetAllEntitiesAsync<>`)
  * Delete entity (`EntityStore.DeleteEntityAsync`)
  * Create, Update, or Delete multiple entities (`EntityStore.SaveEntitiesAsync`)

## Examples

### Databases

Create database:
``` C#
using (var server = new CouchDBServer("http://localhost:5984"))
{
    await server.CreateDbAsync("my-db");
}
```

Get list of all databases:
``` C#
using (var server = new CouchDBServer("http://localhost:5984"))
{
    string[] allDbs = await server.GetAllDbNamesAsync();

    Console.WriteLine($"Total count of DBs: {allDbs.Length}.");
}
```

Delete database:
``` C#
using (var server = new CouchDBServer("http://localhost:5984"))
{
    await server.DeleteDbAsync("my-db");
}
```

### Entities (reusable documents)

Documents (discussed below) are fine if you want to deal with ID and Revision values on your own. i.e. if you just saved a new object document, you need to maintain its ID and Revision for consecutive updates; otherwise you need to keep retrieving the document by ID every time you want to apply further changes to it.
Entities solve the aforementioned problem by updating the ID and Revision in your object, as long as you implement `IEntity` interface.

For example:
``` C#
public sealed class SampleEntity : IEntity
{
    public string _id { get; set; }
    public string _rev { get; set; }
    
    public string Text { get; set; }
    public int Number { get; set; }
}

using (var server = new CouchDBServer("http://localhost:5984"))
{
    using (var db = server.SelectDatabase("my-db"))
    {
        //Treat database as Entity store.
        var store = new EntityStore(db);
    
        // create entity (_id is optional).
        var entity = new SampleEntity { _id = "Sample-entity-1", Text = "This is text", Number = 123 };
        
        // save #1.
        await store.SaveEntityAsync(entity);
        
        //just change entity's properties, no hassle with ID and Revision anymore.
        entity.Text = "This is AWESOME";
        entity.Number = 321;
        
        // and save #2.
        await store.SaveEntityAsync(entity);
        
        // bored? just delete the entity.
        await store.DeleteEntityAsync(entity);
    }
}
```


### Documents

You can work with documents as strings, or JObject's (JSON objects), or your own custom objects (generics when loading, System.Object when saving).
All operations with objects will target to support this flexible approach.
Examples below are using one or another approach just for simplicity's sake.

Create new or save existing document (to save existing, _id and _rev is needed; to create new, none of these is required):
``` C#
using (var server = new CouchDBServer("http://localhost:5984"))
{
    using (var db = server.SelectDatabase("my-db"))
    {
        var newDoc = new { _id = "some-id", someProp = "some value" };
        
        var response = await db.SaveDocumentAsync(newDoc);
        
        Console.WriteLine($"Newly created document ID: {response.Id}.");
        Console.WriteLine($"Newly created document revision number: {response.Revision}.");
    }
}
```

Get document JSON string by ID (can get as JObject or your own custom type through generics as well):
``` C#
using (var server = new CouchDBServer("http://localhost:5984"))
{
    using (var db = server.SelectDatabase("my-db"))
    {
        string document = await db.GetDocumentAsync("some-id");
        
        Console.WriteLine($"Found document JSON string: {document}");
    }
}
```

Get all documents as strings (can get as JObject or your own custom type through generics as well):
``` C#
using (var server = new CouchDBServer("http://localhost:5984"))
{
    using (var db = server.SelectDatabase("my-db"))
    {
        var allDocsObject = await db.GetAllStringDocumentsAsync("some-id");
        
        Console.WriteLine($"Total count of docs found: {allDocsObject.Rows.Count}");
    }
}
```

Delete document:
``` C#
using (var server = new CouchDBServer("http://localhost:5984"))
{
    using (var db = server.SelectDatabase("my-db"))
    {
        var response = await db.DeleteDocumentAsync("some-id", "");
        
        Console.WriteLine($"Deleted document ID: {response.Id}.");
        Console.WriteLine($"Deleted document revision number: {response.Revision}.");
    }
}
```


## Building & Running the Code

I'm currently using Visual Studio 2015 Community Edition. As long as you have that, you will be able to compile and run the code out of the box.

Solution contains a demo project in it, which is a console app.
Just run that demo, and it will ask you to enter one of the supported commands into the console.

Demo assumes that you have CouchDB running under [http://localhost:5984/](http://localhost:5984/).


## Contributing Rules

First, check the Projects tab. I have tasked out things that need to be done.
If what you intend to do is not in the existing todo list, contact me to align your goals with the overall goals.
Once the task is in place, and we both know what you want to contribute, you can go on with next steps.

Fork -> Write code -> Commit to your fork -> Pull request -> wait until I review and merge.

I'm coming with ABOVE senior level engineering and software architecture backgrounds, so I will be reviewing every letter you wrote.
I welcome contributors, and I value quality over quantity.
