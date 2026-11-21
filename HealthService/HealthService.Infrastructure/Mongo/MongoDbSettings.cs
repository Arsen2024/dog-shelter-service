namespace HealthService.Infrastructure.Mongo;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "HealthDb";
}
