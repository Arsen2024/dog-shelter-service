using HealthService.Application.Abstractions;
using HealthService.Domain.Common;
using HealthService.Domain.Entities;
using HealthService.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace HealthService.Infrastructure.Seeding;

/// <summary>
/// Початкові дані зберігаються в JSON (Mongo Extended JSON, вбудований ресурс) і вставляються
/// ОДНІЄЮ bulk-операцією InsertManyAsync — лише якщо колекція порожня (повторний запуск нічого не дублює).
/// </summary>
public abstract class JsonSeeder<T>(MongoDbContext context, ILogger logger) : IDataSeeder where T : BaseEntity
{
    protected MongoDbContext Context { get; } = context;

    protected abstract IMongoCollection<T> Collection { get; }
    protected abstract string ResourceName { get; }

    public string Name => typeof(T).Name;

    public async Task SeedAsync(CancellationToken ct)
    {
        var alreadySeeded = await Collection.CountDocumentsAsync(
            FilterDefinition<T>.Empty, new CountOptions { Limit = 1 }, ct) > 0;

        if (alreadySeeded)
        {
            logger.LogInformation("Seeding {Seeder} skipped: the collection is not empty.", Name);
            return;
        }

        var items = BsonSerializer.Deserialize<List<T>>(ReadResource());
        if (items.Count == 0)
            return;

        await Collection.InsertManyAsync(items, cancellationToken: ct);   // одна bulk-вставка
        logger.LogInformation("Seeded {Count} documents into {Seeder}.", items.Count, Name);
    }

    private string ReadResource()
    {
        var assembly = typeof(JsonSeeder<>).Assembly;
        using var stream = assembly.GetManifestResourceStream(ResourceName)
                           ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

public sealed class MedicalRecordSeeder(MongoDbContext context, ILogger<MedicalRecordSeeder> logger)
    : JsonSeeder<MedicalRecord>(context, logger)
{
    protected override IMongoCollection<MedicalRecord> Collection => Context.MedicalRecords;
    protected override string ResourceName => "Seeding.medicalRecords.json";
}

public sealed class VaccinationSeeder(MongoDbContext context, ILogger<VaccinationSeeder> logger)
    : JsonSeeder<Vaccination>(context, logger)
{
    protected override IMongoCollection<Vaccination> Collection => Context.Vaccinations;
    protected override string ResourceName => "Seeding.vaccinations.json";
}

public sealed class VisitSeeder(MongoDbContext context, ILogger<VisitSeeder> logger)
    : JsonSeeder<VeterinaryVisit>(context, logger)
{
    protected override IMongoCollection<VeterinaryVisit> Collection => Context.VeterinaryVisits;
    protected override string ResourceName => "Seeding.veterinaryVisits.json";
}
