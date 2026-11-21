using Microsoft.Data.SqlClient;

namespace AdoptionsService.Infrastructure.Data;

public interface ISqlConnectionFactory
{
    Task<SqlConnection> CreateOpenConnectionAsync(CancellationToken ct);
}

public class SqlConnectionFactory(string _connectionString) : ISqlConnectionFactory
{
    public async Task<SqlConnection> CreateOpenConnectionAsync(CancellationToken ct)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        return connection;
    }
}
