using System.Data;
using AdoptionsService.Domain.Entities;
using AdoptionsService.Infrastructure.Abstractions;
using AdoptionsService.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace AdoptionsService.Infrastructure.Repositories;

/// <summary>
/// Репозиторій на ЧИСТОМУ ADO.NET: SqlCommand, параметри, SqlDataReader.
/// Не успадковує BaseDapperRepository / IGenericRepository.
/// </summary>
public class ApplicationStatusHistoryRepository(IDbSession session)
    : SqlRepositoryBase(session), IApplicationStatusHistoryRepository
{
    public Task AddAsync(ApplicationStatusHistoryEntity entry, CancellationToken ct) =>
        RunAsync(async (conn, tx) =>
        {
            await using var cmd = new SqlCommand(
                """
                INSERT INTO dbo.ApplicationStatusHistory (ApplicationId, OldStatus, NewStatus, ChangedAt, ChangedBy, Comment)
                VALUES (@ApplicationId, @OldStatus, @NewStatus, @ChangedAt, @ChangedBy, @Comment)
                """,
                conn, tx);

            cmd.Parameters.Add("@ApplicationId", SqlDbType.UniqueIdentifier).Value = entry.ApplicationId;
            cmd.Parameters.Add("@OldStatus", SqlDbType.NVarChar, 30).Value = (object?)entry.OldStatus ?? DBNull.Value;
            cmd.Parameters.Add("@NewStatus", SqlDbType.NVarChar, 30).Value = entry.NewStatus;
            cmd.Parameters.Add("@ChangedAt", SqlDbType.DateTime2).Value = entry.ChangedAt;
            cmd.Parameters.Add("@ChangedBy", SqlDbType.NVarChar, 100).Value = (object?)entry.ChangedBy ?? DBNull.Value;
            cmd.Parameters.Add("@Comment", SqlDbType.NVarChar, 500).Value = (object?)entry.Comment ?? DBNull.Value;

            return await cmd.ExecuteNonQueryAsync(ct);
        }, ct);

    public Task<IReadOnlyList<ApplicationStatusHistoryEntity>> GetByApplicationAsync(Guid applicationId, CancellationToken ct) =>
        RunAsync<IReadOnlyList<ApplicationStatusHistoryEntity>>(async (conn, tx) =>
        {
            await using var cmd = new SqlCommand(
                """
                SELECT Id, ApplicationId, OldStatus, NewStatus, ChangedAt, ChangedBy, Comment
                FROM dbo.ApplicationStatusHistory
                WHERE ApplicationId = @ApplicationId
                ORDER BY ChangedAt, Id
                """,
                conn, tx);
            cmd.Parameters.Add("@ApplicationId", SqlDbType.UniqueIdentifier).Value = applicationId;

            var result = new List<ApplicationStatusHistoryEntity>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                result.Add(new ApplicationStatusHistoryEntity
                {
                    Id = reader.GetInt64(0),
                    ApplicationId = reader.GetGuid(1),
                    OldStatus = reader.IsDBNull(2) ? null : reader.GetString(2),
                    NewStatus = reader.GetString(3),
                    ChangedAt = reader.GetDateTime(4),
                    ChangedBy = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Comment = reader.IsDBNull(6) ? null : reader.GetString(6)
                });
            }

            return result;
        }, ct);
}
