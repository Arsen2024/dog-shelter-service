using System.Reflection;
using AdoptionsService.Infrastructure.Abstractions;
using AdoptionsService.Infrastructure.Data;
using Dapper;

namespace AdoptionsService.Infrastructure.Repositories;

/// <summary>
/// Спільний CRUD на Dapper. Назви таблиці/стовпців походять лише з коду; усі ЗНАЧЕННЯ передаються параметрами.
/// </summary>
public abstract class BaseDapperRepository<T> : SqlRepositoryBase, IGenericRepository<T> where T : class
{
    // властивості-скаляри, які записуються в INSERT (навігаційні списки та RowVersion виключено)
    private static readonly PropertyInfo[] InsertProperties = typeof(T)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.CanRead && p.CanWrite && p.Name != "RowVersion" && IsScalar(p.PropertyType))
        .ToArray();

    private readonly string _table;
    private readonly string _keyColumn;
    private readonly bool _softDelete;

    protected BaseDapperRepository(IDbSession session, string table, bool softDelete = false, string keyColumn = "Id")
        : base(session)
    {
        _table = table;
        _keyColumn = keyColumn;
        _softDelete = softDelete;
    }

    protected string Table => _table;
    protected string LiveFilter => _softDelete ? " AND IsDeleted = 0" : string.Empty;

    private static bool IsScalar(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive || type == typeof(string) || type == typeof(Guid)
               || type == typeof(DateTime) || type == typeof(decimal);
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct) =>
        RunAsync(async (conn, tx) => await conn.QuerySingleOrDefaultAsync<T>(new CommandDefinition(
            $"SELECT * FROM {_table} WHERE {_keyColumn} = @Id{LiveFilter}",
            new { Id = id }, tx, cancellationToken: ct)), ct);

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct) =>
        RunAsync<IReadOnlyList<T>>(async (conn, tx) =>
        {
            var rows = await conn.QueryAsync<T>(new CommandDefinition(
                $"SELECT * FROM {_table} WHERE 1 = 1{LiveFilter} ORDER BY {_keyColumn}",
                transaction: tx, cancellationToken: ct));
            return rows.ToList();
        }, ct);

    public Task AddAsync(T entity, CancellationToken ct) =>
        RunAsync(async (conn, tx) =>
        {
            var columns = string.Join(", ", InsertProperties.Select(p => $"[{p.Name}]"));
            var values = string.Join(", ", InsertProperties.Select(p => $"@{p.Name}"));

            return await conn.ExecuteAsync(new CommandDefinition(
                $"INSERT INTO {_table} ({columns}) VALUES ({values})",
                entity, tx, cancellationToken: ct));
        }, ct);

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct) =>
        RunAsync(async (conn, tx) =>
        {
            var sql = _softDelete
                ? $"UPDATE {_table} SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME() WHERE {_keyColumn} = @Id AND IsDeleted = 0"
                : $"DELETE FROM {_table} WHERE {_keyColumn} = @Id";

            var affected = await conn.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, tx, cancellationToken: ct));
            return affected > 0;
        }, ct);
}
