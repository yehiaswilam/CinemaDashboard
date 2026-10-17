using System.Diagnostics;
using CinemaDashboard.Data;
using CinemaDashboard.Models;
using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaDashboard.Controllers;

public class HomeController(ApplicationDbContext db) : Controller
{
    // Dashboard home page
    public async Task<IActionResult> Index()
    {
        var statusCounts = await db.Movies
            .GroupBy(m => m.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        int CountOf(MovieStatus s) => statusCounts.FirstOrDefault(x => x.Status == s)?.Count ?? 0;

        var now = DateTime.Now;

        var vm = new DashboardVM
        {
            MoviesCount = await db.Movies.CountAsync(),
            CategoriesCount = await db.Categories.CountAsync(),
            CinemasCount = await db.Cinemas.CountAsync(),
            ActorsCount = await db.Actors.CountAsync(),

            UpcomingCount = CountOf(MovieStatus.Upcoming),
            AvailableCount = CountOf(MovieStatus.Available),
            ExpiredCount = CountOf(MovieStatus.Expired),

            NextShows = await db.Movies
                .AsNoTracking()
                .Include(m => m.Cinema)
                .Where(m => m.DateTime >= now && m.Status != MovieStatus.Expired)
                .OrderBy(m => m.DateTime)
                .Take(5)
                .ToListAsync(),

            LatestMovies = await db.Movies
                .AsNoTracking()
                .Include(m => m.Category)
                .OrderByDescending(m => m.Id)
                .Take(5)
                .ToListAsync(),

            MoviesPerCategory = await db.Categories
                .OrderByDescending(c => c.Movies.Count)
                .Select(c => new NameCount(c.Name, c.Movies.Count))
                .ToListAsync(),

            MoviesPerCinema = await db.Cinemas
                .OrderByDescending(c => c.Movies.Count)
                .Take(6)
                .Select(c => new NameCount(c.Name, c.Movies.Count))
                .ToListAsync(),

            TopActors = await db.Actors
                .OrderByDescending(a => a.MovieActors.Count)
                .Take(5)
                .Select(a => new NameCount(a.Name, a.MovieActors.Count))
                .ToListAsync()
        };

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
