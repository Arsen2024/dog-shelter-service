using AdoptionsService.Infrastructure.Abstractions;
using AdoptionsService.Infrastructure.Data;
using AdoptionsService.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AdoptionsService.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Scoped: одна сесія (з'єднання), один Unit of Work і набір репозиторіїв на HTTP-запит.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IDbSession>(_ => new DbSession(connectionString));

        services.AddScoped<IAdopterRepository, AdopterRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IRequirementRepository, RequirementRepository>();
        services.AddScoped<IDogRepository, DogRepository>();
        services.AddScoped<IApplicationStatusHistoryRepository, ApplicationStatusHistoryRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
