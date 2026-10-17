using System.ComponentModel.DataAnnotations;

namespace CinemaDashboard.Models;

public class Actor
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public string? Img { get; set; }

    // Many-to-Many with Movie through MovieActor
    public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
}
