using HealthService.Application.Abstractions;
using HealthService.Domain.Interfaces;
using HealthService.Infrastructure.HealthChecks;
using HealthService.Infrastructure.Mongo;
using HealthService.Infrastructure.Queries;
using HealthService.Infrastructure.Repositories;
using HealthService.Infrastructure.Seeding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HealthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // BSON-мапінг налаштовується один раз під час старту
        MongoMappings.Register();

        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
        services.AddSingleton<MongoDbContext>();
        services.AddSingleton<MongoIndexInitializer>();

        // запис
        services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
        services.AddScoped<IVaccinationRepository, VaccinationRepository>();
        services.AddScoped<IVisitRepository, VisitRepository>();

        // читання
        services.AddScoped<IMedicalRecordQueries, MedicalRecordQueries>();
        services.AddScoped<IVaccinationQueries, VaccinationQueries>();
        services.AddScoped<IVisitQueries, VisitQueries>();

        // seeding: порядок реєстрації = порядок виконання
        services.AddScoped<IDataSeeder, MedicalRecordSeeder>();
        services.AddScoped<IDataSeeder, VaccinationSeeder>();
        services.AddScoped<IDataSeeder, VisitSeeder>();

        services.AddHostedService<MongoInitializationService>();
        services.AddHealthChecks().AddCheck<MongoHealthCheck>("mongodb");

        return services;
    }
}
