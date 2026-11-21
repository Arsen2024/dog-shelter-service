using AdoptionsService.Domain.Interfaces;
using AdoptionsService.Infrastructure.Data;
using AdoptionsService.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AdoptionsService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<ISqlConnectionFactory>(new SqlConnectionFactory(connectionString));

        services.AddScoped<IAdopterRepository, AdopterRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IRequirementRepository, RequirementRepository>();
        services.AddScoped<IDogRepository, DogRepository>();

        return services;
    }
}
