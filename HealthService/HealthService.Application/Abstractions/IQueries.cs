using HealthService.Application.DTOs;
using HealthService.Domain.Enums;
using MongoDB.Bson;

namespace HealthService.Application.Abstractions;

// Інтерфейси читання (як IOrderQueries в eShop): обробники запитів звертаються лише до них,
// а реалізація в Infrastructure використовує проєкції, $match, skip/limit і агрегації MongoDB.

public interface IVisitQueries
{
    Task<VisitDto?> GetByIdAsync(ObjectId id, CancellationToken ct);

    Task<PagedResult<VisitListItemDto>> GetListAsync(
        Guid? dogId, VisitType? type, string? search, int page, int pageSize, CancellationToken ct);
}

public interface IMedicalRecordQueries
{
    Task<MedicalRecordDto?> GetByDogIdAsync(Guid dogId, CancellationToken ct);
}

public interface IVaccinationQueries
{
    Task<List<VaccinationDto>> GetByDogIdAsync(Guid dogId, CancellationToken ct);

    /// <summary>Aggregation: остання вакцинація кожної собаки, якщо її nextDueAt уже минув.</summary>
    Task<List<OverdueVaccinationDto>> GetOverdueAsync(DateTime asOfUtc, CancellationToken ct);
}

/// <summary>Початкове наповнення однієї колекції (перевірка «чи наповнена» + одна bulk-вставка).</summary>
public interface IDataSeeder
{
    string Name { get; }

    Task SeedAsync(CancellationToken ct);
}
