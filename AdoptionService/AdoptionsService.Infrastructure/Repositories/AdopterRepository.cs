using AdoptionsService.Domain.Entities;
using AdoptionsService.Infrastructure.Abstractions;
using Dapper;

namespace AdoptionsService.Infrastructure.Repositories;

/// <summary>Dapper-репозиторій №1: спільний CRUD з бази + пошук, пагінація, оптимістичне оновлення.</summary>
public class AdopterRepository(IDbSession session)
    : BaseDapperRepository<AdopterEntity>(session, "dbo.Adopters", softDelete: true), IAdopterRepository
{
    public Task<AdopterEntity?> GetByEmailAsync(string email, CancellationToken ct) =>
        RunAsync(async (conn, tx) => await conn.QuerySingleOrDefaultAsync<AdopterEntity>(new CommandDefinition(
            "SELECT * FROM dbo.Adopters WHERE Email = @Email AND IsDeleted = 0",
            new { Email = email }, tx, cancellationToken: ct)), ct);

    public Task<IReadOnlyList<AdopterEntity>> GetPageAsync(int pageNumber, int pageSize, CancellationToken ct) =>
        RunAsync<IReadOnlyList<AdopterEntity>>(async (conn, tx) =>
        {
            var rows = await conn.QueryAsync<AdopterEntity>(new CommandDefinition(
                """
                SELECT * FROM dbo.Adopters
                WHERE IsDeleted = 0
                ORDER BY FullName, Id
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                """,
                new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize }, tx, cancellationToken: ct));
            return rows.ToList();
        }, ct);

    public Task<bool> UpdateAsync(AdopterEntity entity, CancellationToken ct) =>
        RunAsync(async (conn, tx) =>
        {
            var affected = await conn.ExecuteAsync(new CommandDefinition(
                """
                UPDATE dbo.Adopters
                SET FullName = @FullName, Email = @Email, Phone = @Phone, Address = @Address,
                    UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
                WHERE Id = @Id AND RowVersion = @RowVersion AND IsDeleted = 0
                """,
                entity, tx, cancellationToken: ct));
            return affected > 0;
        }, ct);
}
