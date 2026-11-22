using System.Data;
using Microsoft.Data.SqlClient;

namespace AdoptionsService.Infrastructure.Abstractions;

/// <summary>
/// Сесія БД на один HTTP-запит: єдине з'єднання і поточна транзакція.
/// Репозиторії отримують з'єднання та транзакцію звідси, а не рядок підключення.
/// </summary>
public interface IDbSession : IAsyncDisposable
{
    SqlTransaction? Transaction { get; }
    Task<SqlConnection> GetOpenConnectionAsync(CancellationToken ct);
    Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct);
    Task CommitAsync(CancellationToken ct);
    Task RollbackAsync(CancellationToken ct);
}
