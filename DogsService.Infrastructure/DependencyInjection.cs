using DogsService.Infrastructure.Abstractions;
using DogsService.Infrastructure.DbContext;
using DogsService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DogsService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IDogRepository, DogRepository>();
        services.AddScoped<IBreedRepository, BreedRepository>();
        services.AddScoped<IKennelRepository, KennelRepository>();
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<IDogFeatureRepository, DogFeatureRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
