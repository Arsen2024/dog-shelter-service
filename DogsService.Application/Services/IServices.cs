using DogsService.Application.DTOs;

namespace DogsService.Application.Services;

public interface IDogService
{
    Task<PagedResult<DogDto>> GetPageAsync(DogListQuery query, CancellationToken ct);
    Task<DogDetailsDto> GetByIdAsync(Guid id, CancellationToken ct);
    Task<DogDetailsDto> CreateAsync(CreateDogDto dto, CancellationToken ct);
    Task ChangeWeightAsync(Guid id, ChangeWeightDto dto, CancellationToken ct);
    Task ChangeStatusAsync(Guid id, ChangeStatusDto dto, CancellationToken ct);
    Task AssignKennelAsync(Guid id, AssignKennelDto dto, CancellationToken ct);
    Task RemoveFromKennelAsync(Guid id, CancellationToken ct);
    Task AddFeatureAsync(Guid id, AddFeatureDto dto, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}

public interface IBreedService
{
    Task<List<BreedDto>> GetAllAsync(CancellationToken ct);

    /// <summary>includeDogs = false → 1 запит; includeDogs = true → Explicit Loading, 2 запити.</summary>
    Task<BreedWithDogsDto> GetByIdAsync(Guid id, bool includeDogs, CancellationToken ct);
}

public interface IKennelService
{
    Task<List<KennelDto>> GetAllAsync(CancellationToken ct);
}

public interface IFeatureService
{
    Task<List<FeatureDto>> GetAllAsync(CancellationToken ct);
    Task<List<FeatureAssignmentDto>> GetDogsAddedAfterAsync(Guid featureId, DateTime? addedAfter, CancellationToken ct);
}
