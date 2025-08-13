// Repositories/Repository.cs
namespace Flightmanagement.Repositories;

using Flightmanagement.Data;
using Flightmanagement.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly FlightContext _db;
    protected readonly DbSet<T> _set;

    public Repository(FlightContext db)
    {
        _db = db;
        _set = _db.Set<T>();
    }

    public Task<T?> GetByIdAsync(int id) => _set.FindAsync(id).AsTask();

    public async Task<List<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null)
    {
        IQueryable<T> q = _set.AsNoTracking();
        if (predicate != null) q = q.Where(predicate);
        if (include != null) q = include(q);
        return await q.ToListAsync();
    }

    public Task AddAsync(T entity) => _set.AddAsync(entity).AsTask();
    public void Update(T entity) => _set.Update(entity);
    public void Delete(T entity) => _set.Remove(entity);
    public Task<int> SaveAsync() => _db.SaveChangesAsync();
}
