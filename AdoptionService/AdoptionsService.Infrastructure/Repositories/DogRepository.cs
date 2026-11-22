using AdoptionsService.Domain.Entities;
using AdoptionsService.Infrastructure.Abstractions;
using Dapper;

namespace AdoptionsService.Infrastructure.Repositories;

/// <summary>Локальна копія довідника собак. Власник даних — DogsService.</summary>
public class DogRepository(IDbSession session)
    : BaseDapperRepository<DogEntity>(session, "dbo.Dogs"), IDogRepository
{
    public Task UpsertAsync(Guid id, string name, string status, CancellationToken ct) =>
        RunAsync(async (conn, tx) => await conn.ExecuteAsync(new CommandDefinition(
            """
            MERGE dbo.Dogs AS t
            USING (SELECT @Id AS Id, @Name AS Name, @Status AS Status) AS s ON t.Id = s.Id
            WHEN MATCHED THEN
                UPDATE SET Name = s.Name, Status = s.Status, SyncedAt = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (Id, Name, Status) VALUES (s.Id, s.Name, s.Status);
            """,
            new { Id = id, Name = name, Status = status }, tx, cancellationToken: ct)), ct);

    public Task<IReadOnlyList<DogEntity>> GetByStatusAsync(string? status, CancellationToken ct) =>
        RunAsync<IReadOnlyList<DogEntity>>(async (conn, tx) =>
        {
            var rows = await conn.QueryAsync<DogEntity>(new CommandDefinition(
                "SELECT Id, Name, Status, SyncedAt FROM dbo.Dogs WHERE (@Status IS NULL OR Status = @Status) ORDER BY Name",
                new { Status = status }, tx, cancellationToken: ct));
            return rows.ToList();
        }, ct);
}
