using AdoptionsService.Domain.Entities;
using AdoptionsService.Domain.Models;

namespace AdoptionsService.Infrastructure.Abstractions;

public interface IAdopterRepository : IGenericRepository<AdopterEntity>
{
    Task<AdopterEntity?> GetByEmailAsync(string email, CancellationToken ct);
    Task<IReadOnlyList<AdopterEntity>> GetPageAsync(int pageNumber, int pageSize, CancellationToken ct);

    /// <summary>Оптимістична конкурентність: UPDATE ... WHERE RowVersion = @RowVersion. false — версія застаріла.</summary>
    Task<bool> UpdateAsync(AdopterEntity entity, CancellationToken ct);
}

public interface IApplicationRepository : IGenericRepository<AdoptionApplicationEntity>
{
    // ---- виклики збережених процедур (CommandType.StoredProcedure) ----
    Task<Guid> CreateWithDogAsync(Guid adopterId, Guid dogId, string? createdBy, CancellationToken ct);   // usp_CreateApplication
    Task AddDogAsync(Guid applicationId, Guid dogId, int priority, CancellationToken ct);                  // usp_AddDogToApplication
    Task ChangeStatusAsync(Guid applicationId, string newStatus, string? changedBy, string? comment, CancellationToken ct); // usp_ChangeApplicationStatus
    Task<ApplicationDetailsModel?> GetDetailsAsync(Guid applicationId, CancellationToken ct);              // usp_GetApplicationDetails (QueryMultiple)

    // ---- multi-mapping: заявка + собаки одним запитом ----
    Task<AdoptionApplicationEntity?> GetWithDogsAsync(Guid applicationId, CancellationToken ct);

    // ---- специфічні запити ----
    Task<IReadOnlyList<ApplicationListItemModel>> GetPageAsync(string? status, Guid? adopterId, int pageNumber, int pageSize, CancellationToken ct);
    Task<AdoptionApplicationEntity?> GetForUpdateAsync(Guid applicationId, CancellationToken ct);          // UPDLOCK
    Task<bool> HasDogAsync(Guid applicationId, Guid dogId, CancellationToken ct);
    Task<bool> FulfillRequirementAsync(Guid applicationId, Guid requirementId, string? checkedBy, string? notes, CancellationToken ct);
    Task AddAgreementAsync(AdoptionAgreementEntity agreement, CancellationToken ct);
    Task<bool> SetStatusAsync(Guid applicationId, string newStatus, string? updatedBy, CancellationToken ct);
}

public interface IRequirementRepository : IGenericRepository<RequirementEntity>
{
}

public interface IDogRepository : IGenericRepository<DogEntity>
{
    Task UpsertAsync(Guid id, string name, string status, CancellationToken ct);
    Task<IReadOnlyList<DogEntity>> GetByStatusAsync(string? status, CancellationToken ct);
}

/// <summary>
/// Репозиторій на чистому ADO.NET (SqlCommand + SqlDataReader).
/// Свідомо НЕ успадковує IGenericRepository.
/// </summary>
public interface IApplicationStatusHistoryRepository
{
    Task AddAsync(ApplicationStatusHistoryEntity entry, CancellationToken ct);
    Task<IReadOnlyList<ApplicationStatusHistoryEntity>> GetByApplicationAsync(Guid applicationId, CancellationToken ct);
}
