using System.Linq.Expressions;
using CinemaDashboard.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaDashboard.Repositories;

public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : class
{
    private readonly DbSet<T> _set = context.Set<T>();

    public async Task<T?> GetByIdAsync(params object[] keys) => await _set.FindAsync(keys);

    public Task<T?> GetOneAsync(
        Expression<Func<T, bool>> filter,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool tracked = false)
    {
        return BuildQuery(filter, include, tracked).FirstOrDefaultAsync();
    }

    public Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int? take = null,
        bool tracked = false)
    {
        var query = BuildQuery(filter, include, tracked);
        if (orderBy is not null) query = orderBy(query);
        if (take is not null) query = query.Take(take.Value);
        return query.ToListAsync();
    }

    public Task<List<TResult>> SelectAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int? take = null)
    {
        var query = BuildQuery(filter, null, tracked: false);
        if (orderBy is not null) query = orderBy(query);
        if (take is not null) query = query.Take(take.Value);
        return query.Select(selector).ToListAsync();
    }

    public Task<int> CountAsync(Expression<Func<T, bool>>? filter = null) =>
        filter is null ? _set.CountAsync() : _set.CountAsync(filter);

    public Task<bool> AnyAsync(Expression<Func<T, bool>> filter) => _set.AnyAsync(filter);

    public async Task AddAsync(T entity) => await _set.AddAsync(entity);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);

    private IQueryable<T> BuildQuery(
        Expression<Func<T, bool>>? filter,
        Func<IQueryable<T>, IQueryable<T>>? include,
        bool tracked)
    {
        IQueryable<T> query = tracked ? _set : _set.AsNoTracking();
        if (include is not null) query = include(query);
        if (filter is not null) query = query.Where(filter);
        return query;
    }
}
