using Ardalis.Specification;

namespace DogsService.Infrastructure.Abstractions;

/// <summary>
/// Async Generic Repository над DbSet&lt;T&gt;. Update і Delete лише змінюють стан у контексті;
/// зберігає зміни Unit of Work (один SaveChangesAsync наприкінці бізнес-операції).
/// </summary>
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    // Specification pattern: репозиторій лише застосовує специфікацію (WithSpecification)
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification, CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken ct = default);
    Task<int> CountAsync(ISpecification<T> specification, CancellationToken ct = default);
    Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken ct = default);
}
