using Microsoft.Data.SqlClient;

namespace AdoptionsService.Infrastructure.Data;

public abstract class SqlRepositoryBase(ISqlConnectionFactory _factory)
{
    protected async Task<T> RunAsync<T>(Func<SqlConnection, Task<T>> action, CancellationToken ct)
    {
        try
        {
            await using var connection = await _factory.CreateOpenConnectionAsync(ct);
            return await action(connection);
        }
        catch (SqlException ex)
        {
            var translated = SqlExceptionTranslator.Translate(ex);
            if (ReferenceEquals(translated, ex)) throw;
            throw translated;
        }
    }

    protected Task RunAsync(Func<SqlConnection, Task> action, CancellationToken ct) =>
        RunAsync<bool>(async c => { await action(c); return true; }, ct);
}
