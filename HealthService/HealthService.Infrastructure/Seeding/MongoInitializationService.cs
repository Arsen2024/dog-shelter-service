using HealthService.Application.Abstractions;
using HealthService.Infrastructure.Mongo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthService.Infrastructure.Seeding;

/// <summary>
/// Фоновий сервіс старту: спершу створює індекси (CreateManyAsync), потім — якщо ввімкнено MongoDb:SeedOnStartup —
/// запускає IDataSeeder для кожної колекції. Помилка (наприклад, MongoDB ще недоступна) логується і не зупиняє застосунок;
/// стан MongoDB показує health check /health.
/// </summary>
public class MongoInitializationService(
    IServiceProvider services,
    MongoIndexInitializer indexes,
    IOptions<MongoDbSettings> options,
    ILogger<MongoInitializationService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await indexes.EnsureAsync(cancellationToken);

            if (!options.Value.SeedOnStartup)
            {
                logger.LogInformation("Seeding is disabled (MongoDb:SeedOnStartup = false).");
                return;
            }

            using var scope = services.CreateScope();

            // порядок реєстрації: спершу медичні картки, потім залежні від них колекції
            foreach (var seeder in scope.ServiceProvider.GetServices<IDataSeeder>())
                await seeder.SeedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MongoDB initialization (indexes / seeding) failed. The service keeps running.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
