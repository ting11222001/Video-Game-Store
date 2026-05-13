using GameStore.Api.Dtos;
using GameStore.Api.Entities;

namespace GameStore.Api.Data.Mapping;

// Instead of using AutoMapper, we can create extension methods to convert between DTOs and entities. This is a simple approach for small projects.
public static class GameMapping
{
    public static Game ToEntity(this CreateGameDto game)
    {
        return new Game()
        {
            Name = game.Name,
            GenreId = game.GenreId,
            Price = game.Price,
            ReleaseDate = game.ReleaseDate
        };
    }

    public static GameDto ToDto(this Game game)
    {
        return new GameDto(
            game.Id,
            game.Name,
            game.Genre?.Name ?? "Unknown",
            game.Price,
            game.ReleaseDate
        );
    }
}
