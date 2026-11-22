using AdoptionsService.Application.Mappings;
using AdoptionsService.Application.Services;
using AdoptionsService.Infrastructure;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace AdoptionsService.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Реєструє BLL (сервіси, AutoMapper) і DAL (Unit of Work, репозиторії).
    /// Web/API бачить лише цей шар, а не Infrastructure.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services, string connectionString)
    {
        services.AddInfrastructure(connectionString);

        var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        services.AddSingleton(mapperConfiguration);
        services.AddScoped<IMapper>(sp => new Mapper(sp.GetRequiredService<MapperConfiguration>(), sp.GetService));

        services.AddScoped<IAdopterService, AdopterService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IReferenceService, ReferenceService>();

        return services;
    }

    /// <summary>Перевірка конфігурації мапінгу при старті: сервіс не запуститься з некоректним профілем.</summary>
    public static IServiceProvider AssertMappingConfiguration(this IServiceProvider provider)
    {
        provider.GetRequiredService<MapperConfiguration>().AssertConfigurationIsValid();
        return provider;
    }
}
