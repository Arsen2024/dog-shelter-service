using HealthService.Application.Abstractions;
using HealthService.Application.DTOs;
using HealthService.Infrastructure.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthService.Infrastructure.Queries;

public class MedicalRecordQueries(MongoDbContext context) : IMedicalRecordQueries
{
    public Task<MedicalRecordDto?> GetByDogIdAsync(Guid dogId, CancellationToken ct) =>
        MongoGuard.RunAsync(async () =>
        {
            var doc = await context.Raw(CollectionNames.MedicalRecords)
                .Find(Builders<BsonDocument>.Filter.Eq("dogId", dogId.ToString()))
                .FirstOrDefaultAsync(ct);

            return doc is null ? null : DocumentMapper.MedicalRecord(doc);
        });
}
