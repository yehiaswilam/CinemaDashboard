using CinemaDashboard.Models;

namespace CinemaDashboard.ViewModels;

public class DashboardVM
{
    public int MoviesCount { get; set; }
    public int CategoriesCount { get; set; }
    public int CinemasCount { get; set; }
    public int ActorsCount { get; set; }

    public int UpcomingCount { get; set; }
    public int AvailableCount { get; set; }
    public int ExpiredCount { get; set; }

    public List<Movie> NextShows { get; set; } = [];
    public List<Movie> LatestMovies { get; set; } = [];
    public List<NameCount> MoviesPerCategory { get; set; } = [];
    public List<NameCount> MoviesPerCinema { get; set; } = [];
    public List<NameCount> TopActors { get; set; } = [];
}

public record NameCount(string Name, int Count);
