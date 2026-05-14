using GameStore.Api.Dtos;
using GameStore.Api.Entities;

namespace GameStore.Api.Data.Mapping;

public static class GenreMapping
{
    public static GenreDto ToDto(this Genre genre)
    {
        return new GenreDto(
            genre.Id,
            genre.Name
        );
    }
}