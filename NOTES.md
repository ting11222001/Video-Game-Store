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