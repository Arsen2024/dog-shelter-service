using HealthService.Domain.Entities;
using HealthService.Domain.Interfaces;
using HealthService.Domain.Models;
using HealthService.Infrastructure.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthService.Infrastructure.Repositories;

public class VaccinationRepository(MongoDbContext _context) : IVaccinationRepository
{
    public Task CreateAsync(Vaccination vaccination, CancellationToken ct) =>
        _context.Vaccinations.InsertOneAsync(vaccination, cancellationToken: ct);

    // Індекс ix_vaccinations_dogId_vaccinatedAt
    public async Task<List<Vaccination>> GetByDogIdAsync(Guid dogId, CancellationToken ct) =>
        await _context.Vaccinations
            .Find(v => v.DogId == dogId)
            .SortByDescending(v => v.VaccinatedAt)
            .ToListAsync(ct);

    /// <summary>
    /// Aggregation pipeline: береться ОСТАННЯ вакцинація кожної собаки, і лише якщо її nextDueAt уже минув.
    /// (Собака з новішою чинною вакцинацією не потрапляє у вибірку.)
    /// </summary>
    public async Task<List<OverdueVaccination>> GetOverdueAsync(DateTime asOfUtc, CancellationToken ct)
    {
        var pipeline = new[]
        {
            new BsonDocument("$sort", new BsonDocument("vaccinatedAt", -1)),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$dogId" },
                { "dogName", new BsonDocument("$first", "$dogName") },
                { "lastVaccinatedAt", new BsonDocument("$first", "$vaccinatedAt") },
                { "nextDueAt", new BsonDocument("$first", "$nextDueAt") }
            }),
            new BsonDocument("$match", new BsonDocument("nextDueAt", new BsonDocument("$lt", asOfUtc))),
            new BsonDocument("$sort", new BsonDocument("nextDueAt", 1))
        };

        var docs = await _context.Vaccinations.Aggregate<BsonDocument>(pipeline).ToListAsync(ct);

        return docs.Select(d => new OverdueVaccination
        {
            DogId = Guid.Parse(d["_id"].AsString),
            DogName = d["dogName"].AsString,
            LastVaccinatedAt = d["lastVaccinatedAt"].ToUniversalTime(),
            NextDueAt = d["nextDueAt"].ToUniversalTime()
        }).ToList();
    }

    public async Task UpdateDogNameAsync(Guid dogId, string dogName, CancellationToken ct)
    {
        await _context.Vaccinations.UpdateManyAsync(
            v => v.DogId == dogId,
            Builders<Vaccination>.Update.Set(v => v.DogName, dogName),
            cancellationToken: ct);
    }
}
