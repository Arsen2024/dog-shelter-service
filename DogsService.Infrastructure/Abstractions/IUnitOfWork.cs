namespace DogsService.Infrastructure.Abstractions;

/// <summary>
/// Unit of Work об'єднує репозиторії; SaveChangesAsync викликається один раз наприкінці бізнес-операції.
/// </summary>
public interface IUnitOfWork
{
    IDogRepository Dogs { get; }
    IBreedRepository Breeds { get; }
    IKennelRepository Kennels { get; }
    IFeatureRepository Features { get; }
    IDogFeatureRepository DogFeatures { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
