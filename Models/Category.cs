using System.ComponentModel.DataAnnotations;

namespace CinemaDashboard.Models;

public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
