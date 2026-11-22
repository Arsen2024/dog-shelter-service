namespace AdoptionsService.Infrastructure.Abstractions;

/// <summary>Спільний CRUD для всіх агрегатів. Конкретні інтерфейси додають лише специфічні запити.</summary>
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct);
    Task AddAsync(T entity, CancellationToken ct);

    /// <summary>Видаляє запис (soft delete, якщо в таблиці є IsDeleted). false — запису не знайдено.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
