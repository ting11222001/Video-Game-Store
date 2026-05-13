namespace GameStore.Api.Entities;

public class Game
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public DateOnly ReleaseDate { get; set; }
    
    // Define a many-to-1 relationship
    public int GenreId { get; set; }
    public Genre? Genre { get; set; }
}
