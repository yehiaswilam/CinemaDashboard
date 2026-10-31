using CinemaDashboard.Data;
using CinemaDashboard.Models;

namespace CinemaDashboard.Repositories;

public class UnitOfWork(
    ApplicationDbContext context,
    IRepository<Category> categories,
    IRepository<Cinema> cinemas,
    IRepository<Actor> actors,
    IRepository<Movie> movies,
    IRepository<MovieImage> movieImages) : IUnitOfWork
{
    public IRepository<Category> Categories { get; } = categories;
    public IRepository<Cinema> Cinemas { get; } = cinemas;
    public IRepository<Actor> Actors { get; } = actors;
    public IRepository<Movie> Movies { get; } = movies;
    public IRepository<MovieImage> MovieImages { get; } = movieImages;

    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();
}
