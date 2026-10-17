using System.ComponentModel.DataAnnotations;

namespace CinemaDashboard.Models;

public enum MovieStatus
{
    [Display(Name = "Upcoming")]
    Upcoming = 0,

    [Display(Name = "Now Showing")]
    Available = 1,

    [Display(Name = "Expired")]
    Expired = 2
}

public static class MovieStatusExtensions
{
    public static string Label(this MovieStatus status) => status switch
    {
        MovieStatus.Upcoming => "Upcoming",
        MovieStatus.Available => "Now Showing",
        MovieStatus.Expired => "Expired",
        _ => status.ToString()
    };

    public static string BadgeClass(this MovieStatus status) => status switch
    {
        MovieStatus.Upcoming => "bg-info-subtle text-info-emphasis",
        MovieStatus.Available => "bg-success-subtle text-success-emphasis",
        MovieStatus.Expired => "bg-secondary-subtle text-secondary-emphasis",
        _ => "bg-light text-dark"
    };
}
