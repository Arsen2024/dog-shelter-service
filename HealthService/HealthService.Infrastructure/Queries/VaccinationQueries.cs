using HealthService.Application.Abstractions;
using HealthService.Application.DTOs;
using HealthService.Infrastructure.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthService.Infrastructure.Queries;

public class VaccinationQueries(MongoDbContext context) : IVaccinationQueries
{
    private IMongoCollection<BsonDocument> Vaccinations => context.Raw(CollectionNames.Vaccinations);

    // індекс ix_vaccinations_dogId_vaccinatedAt
    public Task<List<VaccinationDto>> GetByDogIdAsync(Guid dogId, CancellationToken ct) =>
        MongoGuard.RunAsync(async () =>
        {
            var docs = await Vaccinations
                .Find(Builders<BsonDocument>.Filter.Eq("dogId", dogId.ToString()))
                .Sort(Builders<BsonDocument>.Sort.Descending("vaccinatedAt"))
                .ToListAsync(ct);

            return docs.Select(DocumentMapper.Vaccination).ToList();
        });

    /// <summary>
    /// Aggregation pipeline: береться ОСТАННЯ вакцинація кожної собаки, і лише якщо її nextDueAt уже минув.
    /// </summary>
    public Task<List<OverdueVaccinationDto>> GetOverdueAsync(DateTime asOfUtc, CancellationToken ct) =>
        MongoGuard.RunAsync(async () =>
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

            var docs = await Vaccinations.Aggregate<BsonDocument>(pipeline).ToListAsync(ct);

            return docs.Select(d => new OverdueVaccinationDto
            {
                DogId = Guid.Parse(d["_id"].AsString),
                DogName = d["dogName"].AsString,
                LastVaccinatedAt = d["lastVaccinatedAt"].ToUniversalTime(),
                NextDueAt = d["nextDueAt"].ToUniversalTime()
            }).ToList();
        });
}
