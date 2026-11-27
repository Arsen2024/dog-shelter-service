using FluentValidation;
using HealthService.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace HealthService.Application;

public static class DependencyInjection
{
    /// <summary>Реєструє MediatR (зі скануванням обробників), Pipeline Behaviors, валідатори й AutoMapper.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // порядок реєстрації = порядок виконання (від зовнішнього до внутрішнього):
            // ExceptionHandling → Logging → Performance → Validation → обробник
            cfg.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(assembly);

        return services;
    }

    /// <summary>Перевірка конфігурації AutoMapper при старті сервісу.</summary>
    public static IServiceProvider AssertMappingConfiguration(this IServiceProvider provider)
    {
        provider.GetRequiredService<AutoMapper.IConfigurationProvider>().AssertConfigurationIsValid();
        return provider;
    }
}
