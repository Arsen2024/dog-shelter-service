using System.Data;

namespace AdoptionsService.Infrastructure.Abstractions;

/// <summary>
/// Unit of Work: одне з'єднання на HTTP-запит (Scoped), транзакція з обраним рівнем ізоляції,
/// спільна для всіх репозиторіїв, гарантований Commit або Rollback.
/// </summary>
public interface IUnitOfWork
{
    IAdopterRepository Adopters { get; }
    IApplicationRepository Applications { get; }
    IRequirementRepository Requirements { get; }
    IDogRepository Dogs { get; }
    IApplicationStatusHistoryRepository History { get; }

    Task BeginAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);

    /// <summary>Begin → action → Commit; будь-який виняток → Rollback і повторне викидання.</summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default);
}
