using HealthService.Domain.Entities;
using HealthService.Domain.Interfaces;
using HealthService.Infrastructure.Mongo;
using MongoDB.Driver;

namespace HealthService.Infrastructure.Repositories;

public class VaccinationRepository(MongoDbContext context)
    : MongoRepository<Vaccination>(context.Vaccinations), IVaccinationRepository
{
    public Task<long> RenameDogAsync(Guid dogId, string dogName, CancellationToken ct = default) =>
        MongoGuard.RunAsync(async () =>
        {
            var result = await Collection.UpdateManyAsync(
                Builders<Vaccination>.Filter.Eq(v => v.DogId, dogId),
                Builders<Vaccination>.Update.Set(v => v.DogName, dogName),
                cancellationToken: ct);
            return result.ModifiedCount;
        });
}
