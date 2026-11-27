using HealthService.Domain.Entities;
using HealthService.Domain.Interfaces;
using HealthService.Infrastructure.Mongo;
using MongoDB.Driver;

namespace HealthService.Infrastructure.Repositories;

public class VisitRepository(MongoDbContext context)
    : MongoRepository<VeterinaryVisit>(context.VeterinaryVisits), IVisitRepository
{
    public Task<long> RenameDogAsync(Guid dogId, string dogName, CancellationToken ct = default) =>
        MongoGuard.RunAsync(async () =>
        {
            var result = await Collection.UpdateManyAsync(
                Builders<VeterinaryVisit>.Filter.Eq(v => v.DogId, dogId),
                Builders<VeterinaryVisit>.Update.Set(v => v.DogName, dogName),
                cancellationToken: ct);
            return result.ModifiedCount;
        });
}
