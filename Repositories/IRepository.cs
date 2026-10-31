using System.Linq.Expressions;

namespace CinemaDashboard.Repositories;

/// <summary>
/// Generic data-access contract (DIP): services depend on this abstraction, never on EF Core's DbContext directly.
/// One implementation works for every entity (OCP: new entities need no new repository code).
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(params object[] keys);

    Task<T?> GetOneAsync(
        Expression<Func<T, bool>> filter,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool tracked = false);

    Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int? take = null,
        bool tracked = false);

    Task<List<TResult>> SelectAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int? take = null);

    Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);

    Task<bool> AnyAsync(Expression<Func<T, bool>> filter);

    Task AddAsync(T entity);

    void Update(T entity);

    void Remove(T entity);
}
