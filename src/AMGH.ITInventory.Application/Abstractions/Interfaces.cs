using AMGH.ITInventory.Domain.Common;

namespace AMGH.ITInventory.Application.Abstractions;

public interface ICurrentUser { string? UserName { get; } string? IpAddress { get; } }

public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    IQueryable<T> Query();
    Task AddAsync(T entity, CancellationToken ct = default);
    void Remove(T entity);
}

public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : Entity;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IClock { DateTime UtcNow { get; } }
