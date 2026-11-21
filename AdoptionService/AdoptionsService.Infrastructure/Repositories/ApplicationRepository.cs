using System.Data;
using AdoptionsService.Domain.Interfaces;
using AdoptionsService.Domain.Models;
using AdoptionsService.Infrastructure.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AdoptionsService.Infrastructure.Repositories;

public class ApplicationRepository(ISqlConnectionFactory factory) : SqlRepositoryBase(factory), IApplicationRepository
{
    // Dapper + збережена процедура (транзакція всередині процедури)
    public Task<Guid> CreateAsync(Guid adopterId, Guid dogId, string? createdBy, CancellationToken ct) =>
        RunAsync(conn => conn.QuerySingleAsync<Guid>(new CommandDefinition(
            "dbo.usp_CreateApplication",
            new { AdopterId = adopterId, DogId = dogId, CreatedBy = createdBy },
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct)), ct);

    public Task AddDogAsync(Guid applicationId, Guid dogId, int priority, CancellationToken ct) =>
        RunAsync(conn => conn.ExecuteAsync(new CommandDefinition(
            "dbo.usp_AddDogToApplication",
            new { ApplicationId = applicationId, DogId = dogId, Priority = priority },
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct)), ct);

    // «Чистий» ADO.NET: SqlCommand з явними параметрами (демонстрація обох підходів лабораторної)
    public Task ChangeStatusAsync(Guid applicationId, string newStatus, string? changedBy, string? comment, CancellationToken ct) =>
        RunAsync(async conn =>
        {
            await using var cmd = new SqlCommand("dbo.usp_ChangeApplicationStatus", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@ApplicationId", SqlDbType.UniqueIdentifier).Value = applicationId;
            cmd.Parameters.Add("@NewStatus", SqlDbType.NVarChar, 30).Value = newStatus;
            cmd.Parameters.Add("@ChangedBy", SqlDbType.NVarChar, 100).Value = (object?)changedBy ?? DBNull.Value;
            cmd.Parameters.Add("@Comment", SqlDbType.NVarChar, 500).Value = (object?)comment ?? DBNull.Value;

            await cmd.ExecuteNonQueryAsync(ct);
        }, ct);

    public Task SignAgreementAsync(Guid applicationId, Guid dogId, string agreementNumber, string signedBy, string? terms, CancellationToken ct) =>
        RunAsync(conn => conn.ExecuteAsync(new CommandDefinition(
            "dbo.usp_SignAgreement",
            new
            {
                ApplicationId = applicationId,
                DogId = dogId,
                AgreementNumber = agreementNumber,
                SignedBy = signedBy,
                Terms = terms
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct)), ct);

    // Dapper QueryMultiple: 4 result set-и однієї процедури
    public Task<ApplicationDetailsModel?> GetDetailsAsync(Guid applicationId, CancellationToken ct) =>
        RunAsync<ApplicationDetailsModel?>(async conn =>
        {
            using var grid = await conn.QueryMultipleAsync(new CommandDefinition(
                "dbo.usp_GetApplicationDetails",
                new { ApplicationId = applicationId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

            var header = await grid.ReadSingleOrDefaultAsync<ApplicationHeaderModel>();
            if (header is null)
                return null;

            return new ApplicationDetailsModel
            {
                Header = header,
                Dogs = (await grid.ReadAsync<ApplicationDogModel>()).ToList(),
                Requirements = (await grid.ReadAsync<ApplicationRequirementModel>()).ToList(),
                History = (await grid.ReadAsync<StatusHistoryModel>()).ToList()
            };
        }, ct);

    // Використовує IX_Applications_Status_SubmittedAt / IX_Applications_AdopterId_Status
    public Task<List<ApplicationListItemModel>> ListAsync(string? status, Guid? adopterId, int pageNumber, int pageSize, CancellationToken ct) =>
        RunAsync(async conn =>
        {
            var rows = await conn.QueryAsync<ApplicationListItemModel>(new CommandDefinition(
                """
                SELECT a.Id, a.Status, a.SubmittedAt, a.AdopterId, ad.FullName AS AdopterName
                FROM dbo.AdoptionApplications a
                JOIN dbo.Adopters ad ON ad.Id = a.AdopterId
                WHERE a.IsDeleted = 0
                  AND (@Status IS NULL OR a.Status = @Status)
                  AND (@AdopterId IS NULL OR a.AdopterId = @AdopterId)
                ORDER BY a.SubmittedAt DESC, a.Id
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                OPTION (RECOMPILE)
                """,
                new { Status = status, AdopterId = adopterId, Offset = (pageNumber - 1) * pageSize, PageSize = pageSize },
                cancellationToken: ct));
            return rows.ToList();
        }, ct);

    public Task<bool> FulfillRequirementAsync(Guid applicationId, Guid requirementId, string? checkedBy, string? notes, CancellationToken ct) =>
        RunAsync(async conn =>
        {
            var affected = await conn.ExecuteAsync(new CommandDefinition(
                """
                UPDATE ar
                SET IsFulfilled = 1, CheckedAt = SYSUTCDATETIME(), CheckedBy = @CheckedBy, Notes = @Notes
                FROM dbo.ApplicationRequirements ar
                JOIN dbo.AdoptionApplications a ON a.Id = ar.ApplicationId
                WHERE ar.ApplicationId = @ApplicationId
                  AND ar.RequirementId = @RequirementId
                  AND a.IsDeleted = 0
                  AND a.Status IN ('Submitted', 'UnderReview')
                """,
                new { ApplicationId = applicationId, RequirementId = requirementId, CheckedBy = checkedBy, Notes = notes },
                cancellationToken: ct));
            return affected > 0;
        }, ct);
}
