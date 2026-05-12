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