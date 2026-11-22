using AdoptionsService.Application.DTOs;

namespace AdoptionsService.Application.Services;

public interface IAdopterService
{
    Task<List<AdopterDto>> GetPageAsync(int pageNumber, int pageSize, CancellationToken ct);
    Task<AdopterDto> GetByIdAsync(Guid id, CancellationToken ct);
    Task<AdopterDto> CreateAsync(CreateAdopterDto dto, CancellationToken ct);
    Task<AdopterDto> UpdateAsync(Guid id, UpdateAdopterDto dto, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}

public interface IApplicationService
{
    Task<List<ApplicationListItemDto>> GetPageAsync(ApplicationListQuery query, CancellationToken ct);
    Task<ApplicationDto> GetByIdAsync(Guid id, CancellationToken ct);
    Task<ApplicationDetailsDto> GetDetailsAsync(Guid id, CancellationToken ct);
    Task<List<StatusHistoryDto>> GetHistoryAsync(Guid id, CancellationToken ct);

    Task<ApplicationDto> CreateAsync(CreateApplicationDto dto, CancellationToken ct);
    Task AddDogAsync(Guid id, AddDogDto dto, CancellationToken ct);
    Task ChangeStatusAsync(Guid id, ChangeStatusDto dto, CancellationToken ct);
    Task FulfillRequirementAsync(Guid id, Guid requirementId, FulfillRequirementDto dto, CancellationToken ct);

    /// <summary>Транзакційна операція: договір + статус заявки + історія в одній транзакції.</summary>
    Task SignAgreementAsync(Guid id, SignAgreementDto dto, bool simulateFailure, CancellationToken ct);
}

public interface IReferenceService
{
    Task<List<RequirementDto>> GetRequirementsAsync(CancellationToken ct);
    Task<List<DogDto>> GetDogsAsync(string? status, CancellationToken ct);
    Task SyncDogAsync(Guid id, SyncDogDto dto, CancellationToken ct);
}
