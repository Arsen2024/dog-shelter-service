namespace HealthService.Infrastructure.Mongo;

public class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    /// <summary>Пул і таймаути задаються в рядку підключення (maxPoolSize, serverSelectionTimeoutMS).</summary>
    public string ConnectionString { get; set; } =
        "mongodb://localhost:27017/?maxPoolSize=100&serverSelectionTimeoutMS=5000";

    public string DatabaseName { get; set; } = "HealthDb";

    /// <summary>Наповнювати порожні колекції початковими даними при старті (в Production вимкнено).</summary>
    public bool SeedOnStartup { get; set; }
}

public static class CollectionNames
{
    public const string MedicalRecords = "medicalRecords";
    public const string Vaccinations = "vaccinations";
    public const string VeterinaryVisits = "veterinaryVisits";
}
