namespace CinemaDashboard.Models;

// Join table for the Movie <-> Actor many-to-many relationship
public class MovieActor
{
    public int MovieId { get; set; }
    public Movie? Movie { get; set; }

    public int ActorId { get; set; }
    public Actor? Actor { get; set; }
}
