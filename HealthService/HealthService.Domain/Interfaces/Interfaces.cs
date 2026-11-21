using HealthService.Domain.Entities;
using HealthService.Domain.Enums;
using HealthService.Domain.Models;

namespace HealthService.Domain.Interfaces;

public interface ICurrentUser
{
    string? UserName { get; }
}

public interface IMedicalRecordRepository
{
    Task CreateAsync(MedicalRecord record, CancellationToken ct);
    Task<MedicalRecord?> GetByDogIdAsync(Guid dogId, CancellationToken ct);
    Task<bool> AddAllergyAsync(Guid dogId, Allergy allergy, CancellationToken ct);
    Task<bool> AddNoteAsync(Guid dogId, MedicalNote note, CancellationToken ct);
    Task UpdateDogNameAsync(Guid dogId, string dogName, CancellationToken ct);
}

public interface IVaccinationRepository
{
    Task CreateAsync(Vaccination vaccination, CancellationToken ct);
    Task<List<Vaccination>> GetByDogIdAsync(Guid dogId, CancellationToken ct);
    Task<List<OverdueVaccination>> GetOverdueAsync(DateTime asOfUtc, CancellationToken ct);
    Task UpdateDogNameAsync(Guid dogId, string dogName, CancellationToken ct);
}

public interface IVisitRepository
{
    Task CreateAsync(VeterinaryVisit visit, CancellationToken ct);
    Task<VeterinaryVisit?> GetByIdAsync(string id, CancellationToken ct);
    Task<List<VeterinaryVisit>> GetByDogIdAsync(Guid dogId, VisitType? type, int pageNumber, int pageSize, CancellationToken ct);
    Task UpdateDogNameAsync(Guid dogId, string dogName, CancellationToken ct);
}
