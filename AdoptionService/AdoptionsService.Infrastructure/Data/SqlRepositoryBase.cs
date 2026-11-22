using AdoptionsService.Infrastructure.Abstractions;
using Microsoft.Data.SqlClient;

namespace AdoptionsService.Infrastructure.Data;

/// <summary>Спільна база для всіх репозиторіїв (і Dapper, і ADO.NET): з'єднання та транзакція беруться з IDbSession.</summary>
public abstract class SqlRepositoryBase(IDbSession session)
{
    protected IDbSession Session { get; } = session;

    protected async Task<TResult> RunAsync<TResult>(
        Func<SqlConnection, SqlTransaction?, Task<TResult>> action, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var connection = await Session.GetOpenConnectionAsync(ct);
            return await action(connection, Session.Transaction);
        }
        catch (SqlException ex)
        {
            var translated = SqlExceptionTranslator.Translate(ex);
            if (ReferenceEquals(translated, ex)) throw;
            throw translated;
        }
    }
}
