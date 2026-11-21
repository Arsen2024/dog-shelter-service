using HealthService.Domain.Interfaces;
using HealthService.Infrastructure.Mongo;
using HealthService.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HealthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, MongoDbSettings settings)
    {
        services.AddSingleton(new MongoDbContext(settings));

        services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
        services.AddScoped<IVaccinationRepository, VaccinationRepository>();
        services.AddScoped<IVisitRepository, VisitRepository>();

        return services;
    }
}
