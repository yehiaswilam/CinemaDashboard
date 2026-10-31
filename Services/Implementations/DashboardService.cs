using CinemaDashboard.Models;
using CinemaDashboard.Repositories;
using CinemaDashboard.Services.Interfaces;
using CinemaDashboard.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CinemaDashboard.Services.Implementations;

public class DashboardService(IUnitOfWork uow) : IDashboardService
{
    public async Task<DashboardVM> GetDashboardAsync()
    {
        var now = DateTime.Now;

        return new DashboardVM
        {
            MoviesCount = await uow.Movies.CountAsync(),
            CategoriesCount = await uow.Categories.CountAsync(),
            CinemasCount = await uow.Cinemas.CountAsync(),
            ActorsCount = await uow.Actors.CountAsync(),

            UpcomingCount = await uow.Movies.CountAsync(m => m.Status == MovieStatus.Upcoming),
            AvailableCount = await uow.Movies.CountAsync(m => m.Status == MovieStatus.Available),
            ExpiredCount = await uow.Movies.CountAsync(m => m.Status == MovieStatus.Expired),

            NextShows = await uow.Movies.GetAllAsync(
                filter: m => m.DateTime >= now && m.Status != MovieStatus.Expired,
                include: q => q.Include(m => m.Cinema),
                orderBy: q => q.OrderBy(m => m.DateTime),
                take: 5),

            LatestMovies = await uow.Movies.GetAllAsync(
                include: q => q.Include(m => m.Category),
                orderBy: q => q.OrderByDescending(m => m.Id),
                take: 5),

            MoviesPerCategory = await uow.Categories.SelectAsync(
                c => new NameCount(c.Name, c.Movies.Count),
                orderBy: q => q.OrderByDescending(c => c.Movies.Count)),

            MoviesPerCinema = await uow.Cinemas.SelectAsync(
                c => new NameCount(c.Name, c.Movies.Count),
                orderBy: q => q.OrderByDescending(c => c.Movies.Count),
                take: 6),

            TopActors = await uow.Actors.SelectAsync(
                a => new NameCount(a.Name, a.MovieActors.Count),
                orderBy: q => q.OrderByDescending(a => a.MovieActors.Count),
                take: 5)
        };
    }
}
