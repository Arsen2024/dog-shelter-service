using DogsService.Infrastructure.Abstractions;
using DogsService.Infrastructure.DbContext;

namespace DogsService.Infrastructure.Repositories;

public class UnitOfWork(
    AppDbContext context,
    IDogRepository dogs,
    IBreedRepository breeds,
    IKennelRepository kennels,
    IFeatureRepository features,
    IDogFeatureRepository dogFeatures) : IUnitOfWork
{
    public IDogRepository Dogs => dogs;
    public IBreedRepository Breeds => breeds;
    public IKennelRepository Kennels => kennels;
    public IFeatureRepository Features => features;
    public IDogFeatureRepository DogFeatures => dogFeatures;

    // Єдиний SaveChangesAsync на бізнес-операцію: усі репозиторії працюють з одним AppDbContext
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
