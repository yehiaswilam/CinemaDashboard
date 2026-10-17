using System.ComponentModel.DataAnnotations;

namespace CinemaDashboard.Models;

public class Movie
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    // "Des" in the task
    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public MovieStatus Status { get; set; }

    // Show date & time
    public DateTime DateTime { get; set; }

    public string MainImg { get; set; } = string.Empty;

    public ICollection<MovieImage> SubImages { get; set; } = new List<MovieImage>();

    // List<Actor> in the task -> Many-to-Many via MovieActor
    public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int CinemaId { get; set; }
    public Cinema? Cinema { get; set; }
}
