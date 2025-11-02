using DogsService.Domain.Entities;
using DogsService.Domain.Interfaces;
using DogsService.Infrastructure.DbContext;

namespace DogsService.Infrastructure.Repositories;

public class DogRepository(
    AppDbContext context
    ) : IDogRepository
{
    public IQueryable<DogEntity> Query()
        => context.Dogs.AsQueryable();

    public async Task CreateAsync(DogEntity entity, CancellationToken cancellationToken)
        => await context.Dogs.AddAsync(entity, cancellationToken);
}