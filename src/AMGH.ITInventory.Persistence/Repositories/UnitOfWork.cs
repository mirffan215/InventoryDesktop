using AMGH.ITInventory.Application.Abstractions;
using AMGH.ITInventory.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AMGH.ITInventory.Persistence.Repositories;

public class EfRepository<T> : IRepository<T> where T : Entity
{
    private readonly DbSet<T> _set;
    public EfRepository(AmghDbContext db) => _set = db.Set<T>();
    public Task<T?> GetByIdAsync(int id, CancellationToken ct = default) => _set.FindAsync(new object[] { id }, ct).AsTask();
    public IQueryable<T> Query() => _set.AsQueryable();
    public async Task AddAsync(T entity, CancellationToken ct = default) => await _set.AddAsync(entity, ct);
    public void Remove(T entity) => _set.Remove(entity);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AmghDbContext _db;
    private readonly Dictionary<Type, object> _repos = new();
    public UnitOfWork(AmghDbContext db) => _db = db;
    public IRepository<T> Repository<T>() where T : Entity =>
        (IRepository<T>)(_repos.TryGetValue(typeof(T), out var r) ? r : _repos[typeof(T)] = new EfRepository<T>(_db));
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
