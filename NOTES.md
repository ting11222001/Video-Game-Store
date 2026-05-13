# NOTES

## Install these extensions when running .NET code in the VS code

```
.NET Install Tool
C#
C# Dev Kit
```

## Set the .NET SDK version

Run this command in the terminal:
```
dotnet new globaljson --sdk-version 8.0.419
```

## Create the project

Open the command palette. Selet `.NET: New Project`.

Select `ASP.NET Core Empty` with tag `Web/Empty`.

Set my project name:
```
GameStore.Api
```

## Build the code into binary

Either in the terminal, go to the project root, run:
```
dotnet build
```

Or Open the SOLUTION EXPLORER, right click on `GameStore.Api` and click `Build`.

This `.dll` file will show up in the folder:
```
C:\Users\Li-Ting\Documents\Projects\Video-Game-Store\Game
  Store.Api\bin\Debug\net8.0\GameStore.Api.dll
```

## Run the application in the debug mode

Go to a `.cs` project.

Hit `F5`.

Select `C#`, and select `Default configuration` which is the `http` profile in the `launchSettings.json`.

It will build and run the application up.

A browser will open at `http://localhost:5135/` and print `Hello World!` at this point.

### Try debugging

In `Program.cs`, put the red dot at this line, it should trigger the breakpoint:
```
app.MapGet("/", () => "Hello World!");
```

## Test APIs

Games REST API I'm adding: 
```
GET /games
GET /games/1
POST /games
PUT /games/1
DELETE /games/1
```

Used `REST Client` extension.

Created `games.http` in `GameStore.Api` folder.

Add this:
```
GET http://localhost:5135
```

Then, `cd GameStore.Api` and run it up using `dotnet run`, and right-click a request URL in the `games.http` and select `send requests`.

It will show response similar to Postman in a new window:
```
HTTP/1.1 200 OK
Connection: close
Content-Type: application/json; charset=utf-8
Date: Tue, 12 May 2026 05:24:55 GMT
Server: Kestrel
Transfer-Encoding: chunked

[
  {
    "id": 1,
    "name": "Street Fighter II",
    "genre": "Fighting",
    "price": 19.99,
    "releaseDate": "1992-07-15"
  },
  {
    "id": 2,
    "name": "Final Fantasy XIV",
    "genre": "Roleplaying",
    "price": 59.99,
    "releaseDate": "2010-09-30"
  }
]
```

In  `games.http`, add `###` to add another requests.

And add payload like this:
```
###
POST http://localhost:5135/games
Content-Type: application/json

{
    "name": "Minecraft",
    "genre": "Kids and Family",
    "price": 19.99,
    "releaseDate": "2011-11-18"
}
```

Then, right click and `send request`.

Now it should show:
```
HTTP/1.1 201 Created
Connection: close
Content-Type: application/json; charset=utf-8
Date: Tue, 12 May 2026 05:25:57 GMT
Server: Kestrel
Location: http://localhost:5135/games/3
Transfer-Encoding: chunked

{
  "id": 3,
  "name": "Minecraft",
  "genre": "Kids and Family",
  "price": 19.99,
  "releaseDate": "2011-11-18"
}
```

## For the put endpoint

I will need to consider concurrency issue if I jsut write this in the `Program.cs`:
```
// PUT /games
app.MapPut("games/{id}", (int id, UpdateGameDto udpatedGame) =>
{
    var index = games.FindIndex(game => game.Id == id);

    games[index] = new GameDto(
        id,
        udpatedGame.Name,
        udpatedGame.Genre,
        udpatedGame.Price,
        udpatedGame.ReleaseDate
    );

    return Results.NoContent();
});
```

It's a simple way to practice but not thread safe.

### What is DTO

DTO is like a contract between the API and the client. It defines the fields of the data passing in between them.

## Always consider the extra case

Note that for the `GET {id}` and `PUT` methods, a lot of times it could encounter `cannot find a record with a specific ID` case.

So add exceptions for that always.

## `this WebApplication app` - the extension method

In C#, an extension method lets you add a new method to an existing type without changing that type's source code.
You do this by:

1. Writing a static method in a static class
2. Making the first parameter use the this keyword

So this line:
```
public static WebApplication MapGamesEndpoints(this WebApplication app)
```
means: "add a method called MapGamesEndpoints to the WebApplication type."

After that, any WebApplication object can call it like a normal method:
```
app.MapGamesEndpoints(); // looks like it belongs to app, but it's defined in your class
```

### Why WebApplication specifically

WebApplication is the object that ASP.NET gives you when you call `builder.Build()`. It is the core of your web app. It knows how to register routes, run middleware, and start the server.

Your extension method receives app, registers all your game routes on it, then returns it. That is all it does.

### Why use this pattern
It keeps Program.cs clean. Instead of defining 5 endpoints inline there, you move them to a dedicated class and call one line. As your app grows, you can add more endpoint classes (`OrdersEndpoints`, `UsersEndpoints`) and each gets its own file.

## The benefit of using `RouteGroupBuilder` instead of `WebApplication` in the GamesEndpoints

Before, every route had to repeat it:
```
app.MapGet("games", ...);
app.MapGet("games/{id}", ...);
app.MapPost("games", ...);
```

With `MapGroup("games")`, you set the prefix once, and all routes under that group inherit it:
```
var group = app.MapGroup("games");
group.MapGet("/", ...);      // becomes GET /games
group.MapGet("/{id}", ...);  // becomes GET /games/{id}
```

### Other benefits of grouping
You can apply middleware, auth, or rate limiting to all routes in the group in one place, instead of adding it to each route individually. For example:
```
var group = app.MapGroup("games").RequireAuthorization();
```
That applies auth to every route in the group automatically.

### Why RouteGroupBuilder as the return type
The method now returns the group object instead of `app`. This still allows chaining, and the caller can add more config to the group if needed.

## Handle invalid inputs

Other than data annotation, I also need this `Nuget` package called `MinimalApis.Extensions`.

Go to https://www.nuget.org/

Type in the extension's name and copy:
```
dotnet add package MinimalApis.Extensions --version 0.11.0
```

And run inthe `GameStore.Api` route.

Once installed, this will show in `GameStore.Api.csproj`:
```
  <ItemGroup>
    <PackageReference Include="MinimalApis.Extensions" Version="0.11.0" />
  </ItemGroup>
```

so then I can use `WithParameterValidation` in `GamesEndpoints` to make those data annotations in `CreateGameDto` work.

Now if I `POST` a payload with "name" missing:
```
POST http://localhost:5135/games
Content-Type: application/json

{
    "genre": "Kids and Family",
    "price": 19.99,
    "releaseDate": "2011-11-18"
}
```

It will return a proper error message:
```
HTTP/1.1 400 Bad Request
Connection: close
Content-Type: application/problem+json
Date: Tue, 12 May 2026 11:28:20 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": [
      "The Name field is required."
    ]
  }
}
```

I can add `.WithParameterValidation()` to a specific method like to the `POST` method:
```
// POST /games
        group.MapPost("/", (CreateGameDto newGame) =>
        {
            GameDto game = new (
                games.Count + 1,
                newGame.Name,
                newGame.Genre,
                newGame.Price,
                newGame.ReleaseDate
            );
            games.Add(game);

            return Results.CreatedAtRoute(GetGameEndpointName, new { id = game.Id }, game);
        })
        .WithParameterValidation();
```

Or add `.WithParameterValidation();` to the `MapGroup` like this:
```
var group = app.MapGroup("games").WithParameterValidation();
```

Test again - this time with a very long "name":
```
###
POST http://localhost:5135/games
Content-Type: application/json

{
    "name": "MinecraftMinecraftMinecraftMinecraftMinecraftMinecraftMinecraft",
    "genre": "Kids and Family",
    "price": 19.99,
    "releaseDate": "2011-11-18"
}
```

I can see the validation error message properly:
```
HTTP/1.1 400 Bad Request
Connection: close
Content-Type: application/problem+json
Date: Tue, 12 May 2026 11:33:57 GMT
Server: Kestrel
Transfer-Encoding: chunked

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": [
      "The field Name must be a string with a maximum length of 50."
    ]
  }
}
```

So till this point, I'm learning how to use in-memory resources to have an idea how the CRUD endpoints work in `.NET core` with the help of `REST Client` extension - bascially just running `dotnet run` in the `GameStore.Api` folder.

## Setup Entity Framework Core

### The Need For Object-Relational Mapping (O/RM)
A C# REST API talks to a SQL database. The flow works like this:

From C# REST API to SQL Database:
- Translate the Web API request to a SQL query
- Send the SQL query to the database server

From SQL Database to C# REST API:
- Read the resulting database rows
- Translate the database rows to a Web API response

Problems:
- Need to learn a new language (SQL)
- Need a lot of additional data-access code
- Error prone
- Need to manually keep C# models in sync with DB tables

### What is ORM

ORM works as a middle layer between your code and a database.

Your program uses objects (like Song, Artist, Playlist classes). 

Your database stores the same data in tables. The ORM translates between the two, so you can write code like `Song.find(1)` instead of `SELECT * FROM songs WHERE id = 1`.

The arrows go both ways, meaning the ORM handles both reading from and writing to the database.

So Entity Framework Core is a lightweight, open source object-relational mapper for .NET.

Benefits of using EF Core:
```
You write C# instead of SQL. Query data using LINQ, which feels natural in C# code.

Your classes become your schema. You define C# models, and EF Core can create the database tables from them.

Migrations. When you change a model, EF Core generates migration files to update the database schema without manual SQL scripts.

Less boilerplate. No need to manually map database rows to objects. EF Core does that automatically.

Database portability. You can switch between SQL Server, PostgreSQL, SQLite, etc. with minimal code changes.
```

### Create data models

Data models are not the same as DTOs.

DTOs are rarely changed once defined.

`Entities` are going to my data models.

Entity is the data model that maps directly to a database table. It represents the structure of your stored data.

DTO (Data Transfer Object) is a simple object used to carry data between layers or over the network. It has no database mapping.

Why have both?

Your database table and your API response are rarely the same shape. Using entities directly causes problems:
- You might expose sensitive fields (like PasswordHash)
- You might send too much data (unused fields waste bandwidth)
- You couple your API contract to your database schema, so a DB change breaks your API

DTOs let you control exactly what goes in and out.

#### Use `prop` shortcut to create the properties in an Entity

Type `prop` in the `Genre` class, and it will generate:
```
public int MyProperty { get; set; }
```

### Add the datbase engine for this project

Search this `NuGet` package called `Microsoft.EntityFrameworkCore.Sqlite`:
https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite/11.0.0-preview.3.26207.106#readme-body-tab


Copy:
```
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.*
```

If installed correctly, this will show in `GameStore.Api.csproj`:
```
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.*" />
</ItemGroup>
```

### Create a DbContext

DbContext is an object that represents a session with the datbase and can be used to query and save instances of your entities.

Think of DbContext like a shopping cart at a supermarket.

You pick up items (read from the database), put things in the cart (add new records), remove items, or change what's in the cart (update records). When you're done, you go to the checkout (call SaveChanges()), and only then do the real changes happen in the database.

```
// Create a session with the database
using var context = new AppDbContext();

// Query: get all users
var users = context.Users.ToList();

// Add: put a new user in the "cart"
context.Users.Add(new User { Name = "Alice" });

// Checkout: save changes to the database
context.SaveChanges();
```

DbContext manages all of that for you. You don't write raw SQL. You work with C# objects, and it translates that into SQL behind the scenes.

`Repository Pattern` is about hiding database code behind a simple interface.

Instead of writing `context.Users.Where(...).ToList()` everywhere in your app, you put that code in one place called a "repository". The rest of your app just calls `userRepository.GetAll()` and does not care how it works.

Unit of Work Pattern is about grouping multiple changes into one save operation.

DbContext behaves like both patterns out of the box.

It is a Unit of Work because it collects all your changes and only writes to the database when you call SaveChanges().

It is also a Repository because context.Users, context.Orders, etc. each act like a repository for that table.

#### What the options do

`DbContextOptions<GameStoreContext>` is a configuration object. It holds settings like:

Which database to connect to (e.g. SQL Server, SQLite)
The connection string
Any EF Core behaviours you want to turn on/off

#### Why pass it in

`DbContext` (the EF Core base class) needs those settings to work. It does not know which database to use unless you tell it.

By accepting `options` in your constructor and passing it up to `: DbContext(options)`, you are letting the caller decide the configuration. This is called dependency injection.

The "caller" is usually your `Program.cs`, where you write something like:
```
builder.Services.AddDbContext<GameStoreContext>(options =>
    options.UseSqlite("Data Source=GameStore.db"));
```
ASP.NET Core then builds the options object and passes it to GameStoreContext for you automatically.

#### Why the constructor looks like that
`DbContext` has a constructor that accepts `DbContextOptions`. Your class must pass the options up to it using `: DbContext(options)`. If you skip this, EF Core has no configuration and will throw an error.

### OOP concepts involved in `GameStoreContext.cs`

For this line particularly, `public class GameStoreContext(DbContextOptions<GameStoreContext> options) : DbContext(options)`.

Inheritance: GameStoreContext extends DbContext. It gets all of DbContext's database abilities for free.

Constructor: the part in (...) after the class name. It runs when the object is first created. Here it receives the options.

Base constructor call: `: DbContext(options)` means "when creating this object, also run the parent class constructor with these options." The parent needs the options to set itself up, so you must pass them up.

Dependency Injection: instead of `GameStoreContext` creating its own configuration, it receives it from outside. This makes it easier to test and change later. For example, you can pass a real database in production and an in-memory database in tests.

#### `public DbSet<Game> Games => Set<Game>();` in GameStoreContext

This line creates a property called `Games` that returns the `Games` table from the database.

The `=>` means it runs `Set<Game>()` every time you access Games.

Example:
```
// Get all games
var games = await context.Games.ToListAsync();

// Find one game
var game = await context.Games.FindAsync(1);

// Add a game
context.Games.Add(new Game { Name = "Halo" });
await context.SaveChangesAsync();
```

When you write `context.Games`, it calls `Set<Game>()` behind the scenes, which returns the EF Core object that talks to the `Games` table in your database.

`DbSet<Game>` is the EF Core object that maps to the `Games` table, and `Game` is the C# class that maps to a single row in that table.

`Set<T>()` is a method from DbContext that returns the `DbSet<T>` for that entity type.

Using it here instead of just writing ` get; set; }` has two benefits:

Null safety: `Set<T>()` always returns a non-null `DbSet<T>`, so the `=>` expression avoids the compiler warning you'd get with an auto-property that isn't initialized.

Consistency: it matches how EF Core resolves the set internally anyway, so there's no extra overhead.

### Configuration System

Connection string is used to connect the REST API with the Database.

It's not ideal to hard code the connection string in the code.

Better to put it in `appsettings.json`.

`IConfiguration` is the central object that ASP.NET Core builds at startup. It reads from all those sources (appsettings.json, environment variables, user secrets, etc.) and merges them together.

So in your code, you just inject `IConfiguration` and read from it. You don't call the sources directly.

```
// You inject IConfiguration
public MyService(IConfiguration config)
{
    var connStr = config.GetConnectionString("GameStore");
}
```
ASP.NET Core automatically loaded that value from whichever source provided it, appsettings.json, user secrets, or environment variables.

user secrets is the recommended place for connection strings that contain passwords, because appsettings.json might get committed to source control.

Look at `ConnectionStrings` in `appsettings.json`:
```
"ConnectionStrings": {
  "GameStore": "Data Source=GameStore.db"
}
```

Then, in `Program.cs`, use this instead to read from `appsettings.json`:
```
var connString = builder.Configuration.GetConnectionString("GameStore");
```

Add a breakpoint at the next line:
```
builder.Services.AddSqlite<GameStoreContext>(connString);
```

Hit `F5` to run debug mode. By now, the debug variables should show the `connString` has read int the defined value:
```
connString [string] = Data Source=GameStore.db
```

### Setup database migrations

Go to `NuGet` and find a package called `dotnet-ef`.

Copy:
```
dotnet tool install --global dotnet-ef --version 8.0.26
```

Run it in `GameStore.Api`.

Also install `Microsoft.EntityFrameworkCore.Design`.
- This is to generate entity framework migrations later.

Copy:
```
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.26
```

Also, run it in `GameStore.Api`.

So:
- `dotnet-ef` is the CLI tool. It gives you commands like `dotnet ef migrations add` and `dotnet ef database update`. 
- `Microsoft.EntityFrameworkCore.Design` is a `NuGet` package. The CLI tool calls into it at build time to generate migration files. Without it, `dotnet ef` cannot do anything useful in your project.

Run both to check if installed correctly:
```
dotnet ef --version
dotnet ef migrations list
```

Inside `GameStore.Api`, run:
```
dotnet ef migrations add InitialCreate --output-dir Data\Migrations
```

### Inside the migration file

The `Up` method runs when you apply the migration. It builds the database structure.

#### Step 1: Create `Genres` table

Creates a simple table with two columns. `Id` is the primary key, and SQLite will auto-increment it.

#### Step 2: Create `Games` table

Creates the `Games` table, then adds two constraints:

**Primary key**: same as Genres, `Id` is the primary key.

**Foreign key**: this is the relationship part:
- `GenreId` in `Games` must match an existing `Id` in `Genres`
- `onDelete: Cascade` means if you delete a Genre, all Games with that `GenreId` are also deleted automatically
- The name `FK_Games_Genres_GenreId` is just a label, following EF Core's naming convention

#### Step 3: Create an index on `GenreId`

An index is a lookup structure the database builds to make queries faster. Without it, a query like "find all games with `GenreId = 3`" would scan every row. With the index, the database jumps straight to the matching rows.

EF Core adds this index automatically whenever it sees a foreign key, because you will almost always query or join on that column.

The name `IX_Games_GenreId` follows the convention: `IX_<table>_<column>`.

Then, run:
```
dotnet ef database update
```

### About SQLite database in this project

The `GameStore.db` file on disk is proof that EF Core is using SQLite in file-based mode. The migration wrote the schema to that file, and your app reads/writes from it.

You can confirm this by looking at your connection string in `appsettings.json`. It will look something like:
```
"ConnectionStrings": {
  "DefaultConnection": "Data Source=GameStore.db"
}
```

### Add migration automated setup

So that the migration starts as soon as the app is up.

Created `DataExtensions` and there is this:
```
public static void MigrateDb(this WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<GameStoreContext>();
    dbContext.Database.Migrate();
}
```

#### The concept: Dependency Injection (DI) Container

When you write `builder.Services.AddSqlite<GameStoreContext>(connString)`, you are registering `GameStoreContext` into a container. Think of it as a registry, where you say "if anyone needs a `GameStoreContext`, here is how to build one."

Later, when you need a `GameStoreContext`, you ask the container to give you one instead of using `new GameStoreContext()` yourself.

#### The concept: Scopes

Some services are designed to live for a short time, like one web request. `GameStoreContext` is one of these. The container will refuse to give you one unless you are inside a "scope", which is just a controlled lifetime boundary.

During a normal web request, a scope is created and destroyed automatically. But `MigrateDb` runs at startup, outside any request. So you must create a scope manually.

#### Line 1

```csharp
using var scope = app.Services.CreateScope();
```

This manually creates that short-lived boundary. The `using` keyword means the scope is automatically cleaned up when the method finishes.

#### Line 2

```csharp
var dbContext = scope.ServiceProvider.GetRequiredService<GameStoreContext>();
```

This asks the container inside that scope to give you a `GameStoreContext`. It will throw an exception if one cannot be provided, which is a safe default.

#### The full picture in your code

```
Program.cs registers GameStoreContext → container knows how to build it
app.MigrateDb() is called → needs a GameStoreContext
  → creates a scope manually (because we're outside a request)
  → asks the container for a GameStoreContext
  → runs the migration
  → scope is disposed, GameStoreContext is cleaned up
```

### Once added DataExtensions for automatically running migration to the database when app is up

In `GameStore.Api`, run:
```
dotnet run
```

Note that when using SQLite, it's easy to just delete the database i.e. right-click on `GameStore.db` in the EXPLORER, and run `dotnet run` to generate the database again.

### How to seed the data

I can do it in the `GameStoreContext.cs`.

As soon as the migration started, I can start seeding the database.

Add these in `GameStoreContext.cs`:
```
// Seeding initial data for Genres 
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Genre>().HasData(
        new Genre { Id = 1, Name = "Fighting" },
        new Genre { Id = 2, Name = "Roleplaying" },
        new Genre { Id = 3, Name = "Sports" },
        new Genre { Id = 4, Name = "Racing" },
        new Genre { Id = 5, Name = "Kids and Family" }
    );
}
```

Then, run this again to let database be updated:
```
dotnet ef migrations add SeedGenres --output-dir Data\Migrations
```

The new migration file will now be in `Data\Migrations`.

Then, run `dotnet run`.

This will show that the migration is applied:
```
Building...
info: Microsoft.EntityFrameworkCore.Migrations[20402]
      Applying migration '20260513054400_SeedGenres'.
```

And in `GameStore.db` there should be the seeded data present in the `Genres` table too.

## Change the logging information

I can change what to log in the terminal by adding this to `appsettings.json` in the `"Logging"` block:
```
"Microsoft.EntityFrameworkCore.Database.Command": "Warning"
```