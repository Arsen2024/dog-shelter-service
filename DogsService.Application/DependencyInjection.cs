using AutoMapper;
using DogsService.Application.Mappings;
using DogsService.Application.Services;
using DogsService.Infrastructure;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DogsService.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Реєструє BLL (сервіси, AutoMapper, валідатори) і DAL (DbContext, репозиторії, Unit of Work).
    /// Web/API бачить лише цей шар.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services, string connectionString)
    {
        services.AddInfrastructure(connectionString);

        services.AddAutoMapper(typeof(DogMappingProfile).Assembly);

        // валідатори FluentValidation реєструються явно; викликаються через IValidator<T> у сервісах
        services.AddValidatorsFromAssemblyContaining<DogMappingProfile>();

        services.AddScoped<IDogService, DogService>();
        services.AddScoped<IBreedService, BreedService>();
        services.AddScoped<IKennelService, KennelService>();
        services.AddScoped<IFeatureService, FeatureService>();

        return services;
    }

    /// <summary>Перевірка конфігурації мапінгу при старті сервісу.</summary>
    public static IServiceProvider AssertMappingConfiguration(this IServiceProvider provider)
    {
        provider.GetRequiredService<AutoMapper.IConfigurationProvider>().AssertConfigurationIsValid();
        return provider;
    }
}
