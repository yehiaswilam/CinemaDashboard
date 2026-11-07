using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CinemaDashboard.Models;

// Extends the built-in Identity user (stored in the AspNetUsers table)
public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relative URL of the profile picture, e.g. /images/profiles/abc.jpg
    [MaxLength(300)]
    public string? ProfileImage { get; set; }
}
