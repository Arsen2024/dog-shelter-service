using HealthService.Domain.Common;
using HealthService.Domain.Exceptions;
using HealthService.Domain.Interfaces;
using HealthService.Infrastructure.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthService.Infrastructure.Repositories;

/// <summary>Базовий репозиторій запису з оптимістичною конкурентністю за полем version.</summary>
public abstract class MongoRepository<T>(IMongoCollection<T> collection) : IRepository<T> where T : BaseEntity
{
    protected IMongoCollection<T> Collection { get; } = collection;

    public Task<T?> GetByIdAsync(ObjectId id, CancellationToken ct = default) =>
        MongoGuard.RunAsync(async () =>
            await Collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync(ct))!;

    public Task AddAsync(T entity, CancellationToken ct = default) =>
        MongoGuard.RunAsync(() => Collection.InsertOneAsync(entity, cancellationToken: ct));

    public Task UpdateAsync(T entity, CancellationToken ct = default) =>
        MongoGuard.RunAsync(async () =>
        {
            var expectedVersion = entity.Version;
            entity.IncrementVersion();

            // документи, створені до введення version (початкові дані Лаб. 01), не мають цього поля — вважаємо їх версією 0
            var versionFilter = expectedVersion == 0
                ? Builders<T>.Filter.Or(
                    Builders<T>.Filter.Eq("version", 0L),
                    Builders<T>.Filter.Exists("version", false))
                : Builders<T>.Filter.Eq("version", expectedVersion);

            var filter = Builders<T>.Filter.Eq("_id", entity.Id) & versionFilter;

            var result = await Collection.ReplaceOneAsync(filter, entity, cancellationToken: ct);

            if (result.MatchedCount == 0)
            {
                var exists = await Collection.CountDocumentsAsync(
                    Builders<T>.Filter.Eq("_id", entity.Id), cancellationToken: ct) > 0;

                if (!exists)
                    throw new NotFoundException(typeof(T).Name, entity.Id);

                throw new ConflictException(
                    $"Version conflict: the document was modified by someone else (expected version {expectedVersion}).");
            }
        });

    public Task<bool> DeleteAsync(ObjectId id, CancellationToken ct = default) =>
        MongoGuard.RunAsync(async () =>
        {
            var result = await Collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id), ct);
            return result.DeletedCount > 0;
        });
}
