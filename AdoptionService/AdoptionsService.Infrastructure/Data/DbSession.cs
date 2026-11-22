using System.Data;
using AdoptionsService.Infrastructure.Abstractions;
using Microsoft.Data.SqlClient;

namespace AdoptionsService.Infrastructure.Data;

public sealed class DbSession(string connectionString) : IDbSession
{
    private SqlConnection? _connection;
    private SqlTransaction? _transaction;

    public SqlTransaction? Transaction => _transaction;

    public async Task<SqlConnection> GetOpenConnectionAsync(CancellationToken ct)
    {
        _connection ??= new SqlConnection(connectionString);

        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync(ct);

        return _connection;
    }

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("A transaction is already active for this unit of work.");

        var connection = await GetOpenConnectionAsync(ct);
        _transaction = (SqlTransaction)await connection.BeginTransactionAsync(isolationLevel, ct);
    }

    public async Task CommitAsync(CancellationToken ct)
    {
        if (_transaction is null)
            throw new InvalidOperationException("There is no active transaction to commit.");

        try
        {
            await _transaction.CommitAsync(ct);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken ct)
    {
        if (_transaction is null)
            return;

        try
        {
            await _transaction.RollbackAsync(ct);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        // незавершена транзакція завжди відкочується
        if (_transaction is not null)
        {
            try { await _transaction.RollbackAsync(); } catch { /* з'єднання могло вже бути розірване */ }
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
