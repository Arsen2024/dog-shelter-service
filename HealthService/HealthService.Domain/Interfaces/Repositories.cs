using HealthService.Domain.Common;
using HealthService.Domain.Entities;
using MongoDB.Bson;

namespace HealthService.Domain.Interfaces;

/// <summary>Репозиторій запису (команди). Читання для запитів — через IXxxQueries в Application.</summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(ObjectId id, CancellationToken ct = default);

    Task AddAsync(T entity, CancellationToken ct = default);

    /// <summary>
    /// ReplaceOne лише для документа з очікуваною версією (entity.Version до виклику).
    /// Інша версія → ConflictException, документа немає → NotFoundException.
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken ct = default);

    Task<bool> DeleteAsync(ObjectId id, CancellationToken ct = default);
}

public interface IVisitRepository : IRepository<VeterinaryVisit>
{
    /// <summary>Оновлює денормалізоване dogName у всіх візитах собаки (dogSnapshot — знімок, не чіпається).</summary>
    Task<long> RenameDogAsync(Guid dogId, string dogName, CancellationToken ct = default);
}

public interface IMedicalRecordRepository : IRepository<MedicalRecord>
{
    Task<MedicalRecord?> GetByDogIdAsync(Guid dogId, CancellationToken ct = default);
    Task<bool> ExistsByDogIdAsync(Guid dogId, CancellationToken ct = default);
    Task<long> RenameDogAsync(Guid dogId, string dogName, CancellationToken ct = default);
}

public interface IVaccinationRepository : IRepository<Vaccination>
{
    Task<long> RenameDogAsync(Guid dogId, string dogName, CancellationToken ct = default);
}
