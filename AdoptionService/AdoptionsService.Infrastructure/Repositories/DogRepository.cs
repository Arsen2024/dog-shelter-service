using AdoptionsService.Domain.Entities;
using AdoptionsService.Domain.Interfaces;
using AdoptionsService.Infrastructure.Data;
using Dapper;

namespace AdoptionsService.Infrastructure.Repositories;

/// <summary>Локальна копія довідника собак. Власник даних — DogsService.</summary>
public class DogRepository(ISqlConnectionFactory factory) : SqlRepositoryBase(factory), IDogRepository
{
    public Task UpsertAsync(Guid id, string name, string status, CancellationToken ct) =>
        RunAsync(conn => conn.ExecuteAsync(new CommandDefinition(
            """
            MERGE dbo.Dogs AS t
            USING (SELECT @Id AS Id, @Name AS Name, @Status AS Status) AS s ON t.Id = s.Id
            WHEN MATCHED THEN
                UPDATE SET Name = s.Name, Status = s.Status, SyncedAt = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (Id, Name, Status) VALUES (s.Id, s.Name, s.Status);
            """,
            new { Id = id, Name = name, Status = status },
            cancellationToken: ct)), ct);

    public Task<List<DogEntity>> ListAsync(string? status, CancellationToken ct) =>
        RunAsync(async conn =>
        {
            var rows = await conn.QueryAsync<DogEntity>(new CommandDefinition(
                """
                SELECT Id, Name, Status, SyncedAt FROM dbo.Dogs
                WHERE (@Status IS NULL OR Status = @Status)
                ORDER BY Name
                """,
                new { Status = status }, cancellationToken: ct));
            return rows.ToList();
        }, ct);
}
