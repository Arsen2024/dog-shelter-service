using HealthService.Domain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HealthService.Infrastructure.Mongo;

/// <summary>Єдине місце, де створюються IMongoCollection. Налаштування — через IOptions.</summary>
public class MongoDbContext
{
    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;

        Client = new MongoClient(settings.ConnectionString);
        Database = Client.GetDatabase(settings.DatabaseName);
    }

    public IMongoClient Client { get; }
    public IMongoDatabase Database { get; }

    // типізовані колекції (запис)
    public IMongoCollection<MedicalRecord> MedicalRecords => Database.GetCollection<MedicalRecord>(CollectionNames.MedicalRecords);
    public IMongoCollection<Vaccination> Vaccinations => Database.GetCollection<Vaccination>(CollectionNames.Vaccinations);
    public IMongoCollection<VeterinaryVisit> VeterinaryVisits => Database.GetCollection<VeterinaryVisit>(CollectionNames.VeterinaryVisits);

    // «сирі» колекції (читання з проєкціями одразу в DTO)
    public IMongoCollection<MongoDB.Bson.BsonDocument> Raw(string collectionName) =>
        Database.GetCollection<MongoDB.Bson.BsonDocument>(collectionName);
}
