namespace CinemaDashboard.Models;

// One row per sub image of a movie
public class MovieImage
{
    public int Id { get; set; }

    public string Img { get; set; } = string.Empty;

    public int MovieId { get; set; }
    public Movie? Movie { get; set; }
}
