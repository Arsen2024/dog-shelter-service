using HealthService.Domain.Entities;
using HealthService.Domain.Enums;
using HealthService.Domain.Interfaces;
using HealthService.Infrastructure.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthService.Infrastructure.Repositories;

public class VisitRepository(MongoDbContext _context) : IVisitRepository
{
    public Task CreateAsync(VeterinaryVisit visit, CancellationToken ct) =>
        _context.VeterinaryVisits.InsertOneAsync(visit, cancellationToken: ct);

    public async Task<VeterinaryVisit?> GetByIdAsync(string id, CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _context.VeterinaryVisits.Find(v => v.Id == id).FirstOrDefaultAsync(ct);
    }

    // Індекс ix_visits_dogId_visitDate
    public async Task<List<VeterinaryVisit>> GetByDogIdAsync(
        Guid dogId, VisitType? type, int pageNumber, int pageSize, CancellationToken ct)
    {
        var builder = Builders<VeterinaryVisit>.Filter;
        var filter = builder.Eq(v => v.DogId, dogId);

        if (type.HasValue)
            filter &= builder.Eq(v => v.VisitType, type.Value);

        return await _context.VeterinaryVisits
            .Find(filter)
            .SortByDescending(v => v.VisitDate)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);
    }

    // Оновлюється лише dogName; dogSnapshot — знімок, не чіпаємо
    public async Task UpdateDogNameAsync(Guid dogId, string dogName, CancellationToken ct)
    {
        await _context.VeterinaryVisits.UpdateManyAsync(
            v => v.DogId == dogId,
            Builders<VeterinaryVisit>.Update.Set(v => v.DogName, dogName),
            cancellationToken: ct);
    }
}
