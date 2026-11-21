using AdoptionsService.Domain.Entities;
using AdoptionsService.Domain.Models;

namespace AdoptionsService.Domain.Interfaces;

public interface ICurrentUser
{
    string? UserName { get; }
}

public interface IAdopterRepository
{
    Task<Guid> CreateAsync(string fullName, string email, string? phone, string? address, string? createdBy, CancellationToken ct);
    Task<AdopterEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<AdopterEntity>> ListAsync(int pageNumber, int pageSize, CancellationToken ct);
}

public interface IApplicationRepository
{
    Task<Guid> CreateAsync(Guid adopterId, Guid dogId, string? createdBy, CancellationToken ct);
    Task AddDogAsync(Guid applicationId, Guid dogId, int priority, CancellationToken ct);
    Task ChangeStatusAsync(Guid applicationId, string newStatus, string? changedBy, string? comment, CancellationToken ct);
    Task SignAgreementAsync(Guid applicationId, Guid dogId, string agreementNumber, string signedBy, string? terms, CancellationToken ct);
    Task<ApplicationDetailsModel?> GetDetailsAsync(Guid applicationId, CancellationToken ct);
    Task<List<ApplicationListItemModel>> ListAsync(string? status, Guid? adopterId, int pageNumber, int pageSize, CancellationToken ct);
    Task<bool> FulfillRequirementAsync(Guid applicationId, Guid requirementId, string? checkedBy, string? notes, CancellationToken ct);
}

public interface IRequirementRepository
{
    Task<List<RequirementEntity>> GetAllAsync(CancellationToken ct);
}

public interface IDogRepository
{
    Task UpsertAsync(Guid id, string name, string status, CancellationToken ct);
    Task<List<DogEntity>> ListAsync(string? status, CancellationToken ct);
}
