using GameStore.Api.Data;
using GameStore.Api.Data.Mapping;
using GameStore.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

public static class GenresEndpoints
{
    public static RouteGroupBuilder MapGenresEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("genres").WithParameterValidation();

        // GET /genres
        group.MapGet("/", async (GameStoreContext dbContext) =>
            await dbContext.Genres
            .Select(genre => genre.ToDto())
            .AsNoTracking()
            .ToListAsync()
        );

        return group;
    }
}
