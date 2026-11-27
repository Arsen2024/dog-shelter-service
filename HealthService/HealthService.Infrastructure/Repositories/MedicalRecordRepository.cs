using HealthService.Domain.Entities;
using HealthService.Domain.Interfaces;
using HealthService.Infrastructure.Mongo;
using MongoDB.Driver;

namespace HealthService.Infrastructure.Repositories;

public class MedicalRecordRepository(MongoDbContext context)
    : MongoRepository<MedicalRecord>(context.MedicalRecords), IMedicalRecordRepository
{
    public Task<MedicalRecord?> GetByDogIdAsync(Guid dogId, CancellationToken ct = default) =>
        MongoGuard.RunAsync(async () =>
            await Collection.Find(Builders<MedicalRecord>.Filter.Eq(r => r.DogId, dogId)).FirstOrDefaultAsync(ct))!;

    public Task<bool> ExistsByDogIdAsync(Guid dogId, CancellationToken ct = default) =>
        MongoGuard.RunAsync(async () =>
            await Collection.CountDocumentsAsync(
                Builders<MedicalRecord>.Filter.Eq(r => r.DogId, dogId),
                new CountOptions { Limit = 1 }, ct) > 0);

    public Task<long> RenameDogAsync(Guid dogId, string dogName, CancellationToken ct = default) =>
        MongoGuard.RunAsync(async () =>
        {
            var result = await Collection.UpdateManyAsync(
                Builders<MedicalRecord>.Filter.Eq(r => r.DogId, dogId),
                Builders<MedicalRecord>.Update.Set(r => r.DogName, dogName),
                cancellationToken: ct);
            return result.ModifiedCount;
        });
}
