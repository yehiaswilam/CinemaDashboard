using System.ComponentModel.DataAnnotations;

namespace CinemaDashboard.Models;

public class Cinema
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    // Relative path of the uploaded image, e.g. /images/cinemas/abc.jpg
    public string? Img { get; set; }

    public ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
