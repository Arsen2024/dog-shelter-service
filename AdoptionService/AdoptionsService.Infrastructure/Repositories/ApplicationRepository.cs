using System.Data;
using AdoptionsService.Domain.Entities;
using AdoptionsService.Domain.Models;
using AdoptionsService.Infrastructure.Abstractions;
using Dapper;

namespace AdoptionsService.Infrastructure.Repositories;

/// <summary>
/// Dapper-репозиторій №2: multi-mapping (заявка + собаки одним запитом),
/// виклики збережених процедур з CommandType.StoredProcedure, QueryMultiple.
/// </summary>
public class ApplicationRepository(IDbSession session)
    : BaseDapperRepository<AdoptionApplicationEntity>(session, "dbo.AdoptionApplications", softDelete: true),
      IApplicationRepository
{
    // ------------------------- збережені процедури -------------------------

    public Task<Guid> CreateWithDogAsync(Guid adopterId, Guid dogId, string? createdBy, CancellationToken ct) =>
        RunAsync(async (conn, tx) => await conn.QuerySingleAsync<Guid>(new CommandDefinition(
            "dbo.usp_CreateApplication",
            new { AdopterId = adopterId, DogId = dogId, CreatedBy = createdBy },
            tx, commandType: CommandType.StoredProcedure, cancellationToken: ct)), ct);

    public Task AddDogAsync(Guid applicationId, Guid dogId, int priority, CancellationToken ct) =>
        RunAsync(async (conn, tx) => await conn.ExecuteAsync(new CommandDefinition(
            "dbo.usp_AddDogToApplication",
            new { ApplicationId = applicationId, DogId = dogId, Priority = priority },
            tx, commandType: CommandType.StoredProcedure, cancellationToken: ct)), ct);

    public Task ChangeStatusAsync(Guid applicationId, string newStatus, string? changedBy, string? comment, CancellationToken ct) =>
        RunAsync(async (conn, tx) => await conn.ExecuteAsync(new CommandDefinition(
            "dbo.usp_ChangeApplicationStatus",
            new { ApplicationId = applicationId, NewStatus = newStatus, ChangedBy = changedBy, Comment = comment },
            tx, commandType: CommandType.StoredProcedure, cancellationToken: ct)), ct);

    public Task<ApplicationDetailsModel?> GetDetailsAsync(Guid applicationId, CancellationToken ct) =>
        RunAsync<ApplicationDetailsModel?>(async (conn, tx) =>
        {
            using var grid = await conn.QueryMultipleAsync(new CommandDefinition(
                "dbo.usp_GetApplicationDetails",
                new { ApplicationId = applicationId },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));

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

    // ------------------------- multi-mapping -------------------------

    public Task<AdoptionApplicationEntity?> GetWithDogsAsync(Guid applicationId, CancellationToken ct) =>
        RunAsync<AdoptionApplicationEntity?>(async (conn, tx) =>
        {
            var applications = new Dictionary<Guid, AdoptionApplicationEntity>();

            // Dapper не має CommandDefinition-перевантаження для multi-mapping, тому токен перевіряємо вручну
            ct.ThrowIfCancellationRequested();
            await conn.QueryAsync<AdoptionApplicationEntity, ApplicationDogEntity, AdoptionApplicationEntity>(
                """
                SELECT a.Id, a.AdopterId, a.Status, a.SubmittedAt, a.DecisionNote,
                       a.CreatedAt, a.CreatedBy, a.UpdatedAt, a.UpdatedBy, a.IsDeleted, a.RowVersion,
                       d.ApplicationId, d.DogId, d.DogName, d.Priority, d.AddedAt
                FROM dbo.AdoptionApplications a
                LEFT JOIN dbo.ApplicationDogs d ON d.ApplicationId = a.Id
                WHERE a.Id = @Id AND a.IsDeleted = 0
                ORDER BY d.Priority
                """,
                (application, dog) =>
                {
                    if (!applications.TryGetValue(application.Id, out var current))
                    {
                        current = application;
                        applications[application.Id] = current;
                    }

                    if (dog is not null)
                        current.Dogs.Add(dog);

                    return current;
                },
                new { Id = applicationId },
                tx,
                splitOn: "ApplicationId");

            return applications.Values.FirstOrDefault();
        }, ct);

    // ------------------------- специфічні запити -------------------------

    // індекси: IX_Applications_AdopterId_Status / IX_Applications_Status_SubmittedAt
    public Task<IReadOnlyList<ApplicationListItemModel>> GetPageAsync(
        string? status, Guid? adopterId, int pageNumber, int pageSize, CancellationToken ct) =>
        RunAsync<IReadOnlyList<ApplicationListItemModel>>(async (conn, tx) =>
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
                tx, cancellationToken: ct));
            return rows.ToList();
        }, ct);

    /// <summary>UPDLOCK + HOLDLOCK: блокує рядок до кінця транзакції (має викликатися всередині Unit of Work).</summary>
    public Task<AdoptionApplicationEntity?> GetForUpdateAsync(Guid applicationId, CancellationToken ct) =>
        RunAsync(async (conn, tx) => await conn.QuerySingleOrDefaultAsync<AdoptionApplicationEntity>(new CommandDefinition(
            "SELECT * FROM dbo.AdoptionApplications WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id AND IsDeleted = 0",
            new { Id = applicationId }, tx, cancellationToken: ct)), ct);

    public Task<bool> HasDogAsync(Guid applicationId, Guid dogId, CancellationToken ct) =>
        RunAsync(async (conn, tx) => await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(1) FROM dbo.ApplicationDogs WHERE ApplicationId = @ApplicationId AND DogId = @DogId",
            new { ApplicationId = applicationId, DogId = dogId }, tx, cancellationToken: ct)) > 0, ct);

    public Task<bool> FulfillRequirementAsync(Guid applicationId, Guid requirementId, string? checkedBy, string? notes, CancellationToken ct) =>
        RunAsync(async (conn, tx) =>
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
                tx, cancellationToken: ct));
            return affected > 0;
        }, ct);

    public Task AddAgreementAsync(AdoptionAgreementEntity agreement, CancellationToken ct) =>
        RunAsync(async (conn, tx) => await conn.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO dbo.AdoptionAgreements (ApplicationId, DogId, AgreementNumber, SignedAt, SignedBy, Terms)
            VALUES (@ApplicationId, @DogId, @AgreementNumber, @SignedAt, @SignedBy, @Terms)
            """,
            agreement, tx, cancellationToken: ct)), ct);

    public Task<bool> SetStatusAsync(Guid applicationId, string newStatus, string? updatedBy, CancellationToken ct) =>
        RunAsync(async (conn, tx) =>
        {
            var affected = await conn.ExecuteAsync(new CommandDefinition(
                """
                UPDATE dbo.AdoptionApplications
                SET Status = @Status, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
                WHERE Id = @Id AND IsDeleted = 0
                """,
                new { Id = applicationId, Status = newStatus, UpdatedBy = updatedBy }, tx, cancellationToken: ct));
            return affected > 0;
        }, ct);
}
