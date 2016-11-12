# CouchDBClient
Couch DB .NET Client - easy to use, intuitive, self-describing library.     
Caution: work in progress! don't use in production code.

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

    Console.WriteLine($"Total cound of DBs: {allDbs.Length}.");
}
```

Delete database:
``` C#
using (var server = new CouchDBServer("http://localhost:5984"))
{
    await server.DeleteDbAsync("my-db");
}
```

### Documents

You can work with documents as strings, or JObject's (JSON objects), or your own custom objects (generics when loading, System.Object when saving).
All operations with objects will target to support this flexible approach.
Examples below are using one or another approach just for simplicity's sake.

Create document:
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

Get document by ID:
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


## Building & Running the Code

I'm currently using Visual Studio 2015 Community Edition.

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
