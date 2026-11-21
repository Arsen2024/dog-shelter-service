using System.Data;
using AdoptionsService.Domain.Entities;
using AdoptionsService.Domain.Interfaces;
using AdoptionsService.Infrastructure.Data;
using Dapper;

namespace AdoptionsService.Infrastructure.Repositories;

/// <summary>Dapper: створення — через збережену процедуру, читання — inline SQL.</summary>
public class AdopterRepository(ISqlConnectionFactory factory) : SqlRepositoryBase(factory), IAdopterRepository
{
    public Task<Guid> CreateAsync(string fullName, string email, string? phone, string? address, string? createdBy, CancellationToken ct) =>
        RunAsync(conn => conn.QuerySingleAsync<Guid>(new CommandDefinition(
            "dbo.usp_CreateAdopter",
            new { FullName = fullName, Email = email, Phone = phone, Address = address, CreatedBy = createdBy },
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct)), ct);

    public Task<AdopterEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        RunAsync(conn => conn.QuerySingleOrDefaultAsync<AdopterEntity>(new CommandDefinition(
            """
            SELECT Id, FullName, Email, Phone, Address, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted, RowVersion
            FROM dbo.Adopters
            WHERE Id = @Id AND IsDeleted = 0
            """,
            new { Id = id }, cancellationToken: ct)), ct);

    public Task<List<AdopterEntity>> ListAsync(int pageNumber, int pageSize, CancellationToken ct) =>
        RunAsync(async conn =>
        {
            var rows = await conn.QueryAsync<AdopterEntity>(new CommandDefinition(
                """
                SELECT Id, FullName, Email, Phone, Address, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted, RowVersion
                FROM dbo.Adopters
                WHERE IsDeleted = 0
                ORDER BY FullName, Id
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                """,
                new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize },
                cancellationToken: ct));
            return rows.ToList();
        }, ct);
}
