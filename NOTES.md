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
