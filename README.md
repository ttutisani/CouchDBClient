# CouchDBClient
Couch DB .NET Client - easy to use, intuitive, self-describing library.     
So far, only few functions available - work in progress!

# Examples

Create database:
``` C#
using (var server = new CouchDBServer("http://localhost:5984"))
{
    await server.CreateDb("my-db");
}
```

Get list of all databases:
``` C#
using (var server = new CouchDBServer("http://localhost:5984"))
{
    var allDbs = await server.GetAllDbNames();

    foreach (var dbName in allDbs)
    {
        Console.WriteLine(dbName);
    }
}
```



# Building & Running the code

I'm currently using Visual Studio 2015 Community Edition.

Solution contains a demo project in it, which is a console app.
Just run that demo, and it will ask you to enter one of the supported commands into the console.

Demo assumes that you have CouchDB running under [http://localhost:5984/](http://localhost:5984/).


# Contributing rules

First, check the Projects tab. I have tasked out things that need to be done.
If what you intend to do is not in the existing todo list, contact me to align your goals with the overall goals.
Once the task is in place, and we both know what you want to contribute, you can go on with next steps.

Fork -> Write code -> Commit to your fork -> Pull request -> wait until I review and merge.

I'm coming with ABOVE senior level engineering and software architecture backgrounds, so I will be reviewing every letter you wrote.
I welcome contributors, and I value quality over quantity.
