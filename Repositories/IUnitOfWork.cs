using CinemaDashboard.Models;

namespace CinemaDashboard.Repositories;

/// <summary>
/// Groups the repositories and commits all their changes in one transaction.
/// </summary>
public interface IUnitOfWork
{
    IRepository<Category> Categories { get; }
    IRepository<Cinema> Cinemas { get; }
    IRepository<Actor> Actors { get; }
    IRepository<Movie> Movies { get; }
    IRepository<MovieImage> MovieImages { get; }

    Task<int> SaveChangesAsync();
}
