using HealthService.Domain.Entities;
using MongoDB.Driver;

namespace HealthService.Infrastructure.Mongo;

public class MongoDbContext
{
    public MongoDbContext(MongoDbSettings settings)
    {
        MongoMappings.Register();

        var client = new MongoClient(settings.ConnectionString);
        Database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoDatabase Database { get; }

    public IMongoCollection<MedicalRecord> MedicalRecords => Database.GetCollection<MedicalRecord>("medicalRecords");
    public IMongoCollection<Vaccination> Vaccinations => Database.GetCollection<Vaccination>("vaccinations");
    public IMongoCollection<VeterinaryVisit> VeterinaryVisits => Database.GetCollection<VeterinaryVisit>("veterinaryVisits");
}
